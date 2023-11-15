using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cloud.Client;
using Cloud.Models;
using Microsoft.Build.Evaluation;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Serilog;
using VirtoCommerce.Platform.Core.Common;
using Project = Nuke.Common.ProjectModel.Project;

namespace VirtoCommerce.Build;

internal partial class Build
{
    [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
    [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }

    [Parameter("Config file for Argo Application Service")]
    public string ArgoConfigFile { get; set; }

    [Parameter("Array of Helm parameters")]
    public HelmParameter[] HelmParameters { get; set; }

    [Parameter("Argo Application Name")] public string ArgoAppName { get; set; }

    [Parameter("Health Status that need to be awaited")]
    public string HealthStatus { get; set; }

    [Parameter("SyncStatus that need to be awaited")]
    public string SyncStatus { get; set; }

    [Parameter("Delay between requests in seconds")]
    public int Delay { get; set; } = 10;

    [Parameter("Number of attempts before fail")]
    public int AttemptsNumber { get; set; } = 100;

    [Parameter("SaaS Portal")] public string CloudUrl { get; set; } = "https://portal.virtocommerce.cloud";
    [Parameter("SaaS Token")] public string CloudToken { get; set; }
    [Parameter("App Project Name")] public string AppProject { get; set; }
    [Parameter("Cloud Environment Name")] public string EnvironmentName { get; set; }

    [Parameter("Organization name", Name = "Organization")] public string SaaSOrganizationName { get; set; }

    public Target WaitForStatus => _ => _
        .Executes(async () =>
        {
            var isSuccess = false;
            var cloudClient = new VirtoCloudClient(CloudUrl, CloudToken);
            for (var i = 0; i < AttemptsNumber; i++)
            {
                Log.Information($"Attempt #{i + 1}");
                var env = await cloudClient.GetEnvironment(EnvironmentName);
                Log.Information(
                    $"Actual Health Status is {env.Status} - expected is {HealthStatus ?? "Not expected"}\n Actual Sync Status is {env.SyncStatus} - expected is {SyncStatus ?? "Not expected"}");
                if (CheckAppServiceStatus(HealthStatus, env.Status) &&
                    CheckAppServiceStatus(SyncStatus, env.SyncStatus))
                {
                    isSuccess = true;
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(Delay));
            }
            Assert.True(isSuccess, $"Statuses {HealthStatus} {SyncStatus} were not obtained for the number of attempts: {AttemptsNumber}");
        });

    public Target SetEnvParameter => _ => _
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, CloudToken);
            var env = await cloudClient.GetEnvironment(EnvironmentName, SaaSOrganizationName);

            var envHelmParameters = env.Helm.Parameters;
            foreach (var parameter in HelmParameters)
            {
                envHelmParameters[parameter.Name] = parameter.Value;
            }

            await cloudClient.UpdateEnvironmentAsync(env);
        });

    public Target UpdateCloudEnvironment => _ => _
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, CloudToken);
            var rawYaml = await File.ReadAllTextAsync(ArgoConfigFile);
            await cloudClient.UpdateEnvironmentAsync(rawYaml, AppProject);
        });

    private static bool CheckAppServiceStatus(string expected, string actual)
    {
        if (expected == actual || string.IsNullOrEmpty(expected))
        {
            return true;
        }

        return false;
    }

    public static string DockerfileUrl { get; set; } = "https://raw.githubusercontent.com/krankenbro/vc-ci-test/master/Dockerfile";
    public Target PrepareDockerContext => _ => _
        .Before(InitPlatform, DockerLogin, BuildImage, PushImage, BuildAndPush)
        .Triggers(InitPlatform)
        .Executes(async () =>
        {
            var dockerBuildContext = ArtifactsDirectory / "docker";
            var platformDirectory = dockerBuildContext / "platform";
            var modulesPath = platformDirectory / "modules";
            var dockerfilePath = dockerBuildContext / "Dockerfile";

            FileSystemTasks.EnsureCleanDirectory(dockerBuildContext);

            await HttpTasks.HttpDownloadFileAsync(DockerfileUrl, dockerfilePath);

            var modulesSourcePath = Solution?.Path != null
                ? WebProject.Directory / "modules"
                : RootDirectory / "modules";

            // Copy the platform
            if (Solution?.Path != null)
            {
                DotNetTasks.DotNetPublish(settings => settings
                .SetConfiguration(Configuration)
                .SetProcessWorkingDirectory(WebProject.Directory)
                .SetOutput(platformDirectory));
            }
            else
            {
                var nukeDir = Path.Combine(RootDirectory, ".nuke");
                var directories = Directory.GetDirectories(RootDirectory).Where(d => !PathConstruction.IsDescendantPath(modulesSourcePath, d)
                        && !PathConstruction.IsDescendantPath(nukeDir, d)
                        && !PathConstruction.IsDescendantPath(ArtifactsDirectory, d)).ToArray();
                var files = Directory.GetFiles(RootDirectory);

                foreach ( var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    FileSystemTasks.CopyDirectoryRecursively(dir, Path.Combine(platformDirectory, dirName), DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
                }

                foreach( var file in files)
                {
                    FileSystemTasks.CopyFileToDirectory(file, platformDirectory);
                }
            }

            // Copy modules
            var modulesDirectories =  Directory.EnumerateDirectories(modulesSourcePath);
            foreach (var directory in modulesDirectories)
            {
                var webProjects = Directory.EnumerateFiles(directory, $"*.Web.csproj");
                var moduleDirectoryName = Path.GetFileName(directory);
                var moduleDestinationPath = Path.Combine(modulesPath, moduleDirectoryName);
                if (!webProjects.Any())
                {
                    FileSystemTasks.CopyDirectoryRecursively(directory, moduleDestinationPath, DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
                }
                else
                {
                    var webProjectPath = webProjects.FirstOrDefault();
                    var directoryInfo = new DirectoryInfo(directory);
                    if (directoryInfo.LinkTarget != null)
                    {
                        webProjectPath = Path.Combine(directoryInfo.LinkTarget, Path.GetFileName(webProjectPath));
                    }

                    var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(webProjectPath)));
                    var solutions = Directory.EnumerateFiles(solutionDir, "*.sln");
                    Assert.True(solutions.Count() == 1, $"Solutions found: {solutions.Count()}");
                    var solutionPath = solutions.FirstOrDefault();
                    var solution = ProjectModelTasks.ParseSolution(solutionPath);
                    var webProject = solution.AllProjects.FirstOrDefault(p => p.Name.EndsWith(".Web"));

                    WebPackBuildBody(webProject);
                    PublishBody(webProject, Path.Combine(moduleDestinationPath, "bin"), Configuration);
                    foreach (var contentDirectory in _moduleContentFolders)
                    {
                        var contentDestination = Path.Combine(moduleDestinationPath, contentDirectory);
                        var contentSource = Path.Combine(webProject.Directory, contentDirectory);
                        FileSystemTasks.CopyDirectoryRecursively(contentSource, contentDestination, DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
                    }

                    var moduleManifestPath = webProject.Directory / "module.manifest";
                    FileSystemTasks.CopyFileToDirectory(moduleManifestPath, moduleDestinationPath, FileExistsPolicy.OverwriteIfNewer);
                }
            }

            DockerBuildContextPath = dockerBuildContext;

            if (string.IsNullOrWhiteSpace(DockerImageName))
            {
                DockerImageName = $"{DockerUsername}/{EnvironmentName}";
            }

            DockerImageTag ??= DateTime.Now.ToString("MMddyyHHmmss");
            DockerfilePath = dockerfilePath;
            DiscoveryPath = modulesPath;
            ProbingPath = Path.Combine(platformDirectory, "app_data", "modules");
            AppsettingsPath = Path.Combine(platformDirectory, "appsettings.json");
        });

    public Target DeployImage => _ => _
        .DependsOn(PrepareDockerContext, BuildAndPush)
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, CloudToken);
            var env = await cloudClient.GetEnvironment(EnvironmentName, SaaSOrganizationName);

            var envHelmParameters = env.Helm.Parameters;

            envHelmParameters["platform.image.repository"] = DockerImageName;
            envHelmParameters["platform.image.tag"] = DockerImageTag;

            await cloudClient.UpdateEnvironmentAsync(env);
        });
}
