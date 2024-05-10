using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cloud.Client;
using Cloud.Models;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Model;

namespace VirtoCommerce.Build;

internal partial class Build
{
    [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
    [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }

    [Parameter("Config file for Argo Application Service")]
    public string ArgoConfigFile { get; set; }

    [Parameter("Path to the manifest of environment")] public string Manifest { get; set; }

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
    [Parameter("Path for the file with SaaS Token")] public string CloudTokenFile { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud");
    [Parameter("SaaS Auth Provider")]
    public string CloudAuthProvider
    {
        get
        {
            if (AzureAD)
            {
                return nameof(AzureAD);
            }

            return cloudAuthProvider;
        }
        set { cloudAuthProvider = value; }
    }
    [Parameter("Use Azure AD as SaaS Auth Provider")] public bool AzureAD = false;
    private string cloudAuthProvider = "GitHub";

    [Parameter("App Project Name")] public string AppProject { get; set; }
    [Parameter("Cloud Environment Name")] public string EnvironmentName { get; set; }
    [Parameter("Cloud Environment Service Plan")] public string ServicePlan { get; set; } = "F1";
    [Parameter("Cloud Environment Cluster Name")] public string ClusterName { get; set; }
    [Parameter("Cloud Environment Db Provider")] public string DbProvider { get; set; }
    [Parameter("Cloud Environment Db Name")] public string DbName { get; set; }


    [Parameter("Organization name", Name = "Organization")] public string SaaSOrganizationName { get; set; }


    [Parameter("Url to Dockerfile which will use to build the docker image in the CloudDeploy/CloudUp Target")]
    public static string DockerfileUrl { get; set; } = "https://raw.githubusercontent.com/VirtoCommerce/vc-docker/feat/net8/linux/platform/Dockerfile";
    [Parameter("Url to Wake-script which will use to build the docker image in the CloudDeploy/CloudUp Target")]
    public static string WaitScriptUrl { get; set; } = "https://raw.githubusercontent.com/VirtoCommerce/vc-docker/feat/net8/linux/platform/wait-for-it.sh";

    public Target CloudEnvStatus => _ => _
        .Executes(async () =>
        {
            var isSuccess = false;
            var cloudClient = new VirtoCloudClient(CloudUrl, await GetCloudTokenAsync());
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


    public Target CloudEnvSetParameter => _ => _
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, await GetCloudTokenAsync());
            var env = await cloudClient.GetEnvironment(EnvironmentName, SaaSOrganizationName);

            var envHelmParameters = env.Helm.Parameters;
            foreach (var parameter in HelmParameters)
            {
                envHelmParameters[parameter.Name] = parameter.Value;
            }

            await cloudClient.UpdateEnvironmentAsync(env);
        });

    public Target CloudEnvUpdate => _ => _
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, await GetCloudTokenAsync());
            var rawYaml = await File.ReadAllTextAsync(string.IsNullOrWhiteSpace(Manifest) ? ArgoConfigFile : Manifest);
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
    public Target PrepareDockerContext => _ => _
        .Before(DockerLogin, BuildImage, PushImage, CloudInit)
        .Triggers(InitPlatform)
        .Executes(async () => await PrepareDockerContextMethod());

    private async Task PrepareDockerContextMethod()
    {
        var dockerBuildContext = ArtifactsDirectory / "docker";
        var platformDirectory = dockerBuildContext / "publish";
        var modulesPath = platformDirectory / "modules";
        var dockerfilePath = dockerBuildContext / "Dockerfile";
        var waitScriptPath = dockerBuildContext / "wait-for-it.sh";

        dockerBuildContext.CreateOrCleanDirectory();

        await HttpTasks.HttpDownloadFileAsync(DockerfileUrl, dockerfilePath);
        await HttpTasks.HttpDownloadFileAsync(WaitScriptUrl, waitScriptPath);

        var modulesSourcePath = Solution?.Path != null
            ? WebProject.Directory / "modules"
            : RootDirectory / "modules";

        CopyPlatformDirectory(platformDirectory, modulesSourcePath);

        CopyModules(modulesPath, modulesSourcePath);

        DockerBuildContextPath = dockerBuildContext;

        if (string.IsNullOrWhiteSpace(DockerImageName))
        {
            DockerImageName = $"{DockerUsername}/{EnvironmentName.ToLowerInvariant()}";
        }

        DockerImageTag ??= new string[] { DateTime.Now.ToString("MMddyyHHmmss") };
        DockerfilePath = dockerfilePath;
        DiscoveryPath = modulesPath;
        ProbingPath = Path.Combine(platformDirectory, "app_data", "modules");
        AppsettingsPath = Path.Combine(platformDirectory, "appsettings.json");
    }

    private static void CopyModules(AbsolutePath modulesPath, AbsolutePath modulesSourcePath)
    {
        // Copy modules
        var modulesDirectories = Directory.EnumerateDirectories(modulesSourcePath);
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
                var solution = SolutionModelTasks.ParseSolution(solutionPath);
                var webProject = solution.AllProjects.First(p => p.Name.EndsWith(".Web"));

                WebPackBuildMethod(webProject);
                PublishMethod(webProject, Path.Combine(moduleDestinationPath, "bin"), Configuration);
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
    }

    private static void CopyPlatformDirectory(AbsolutePath platformDirectory, AbsolutePath modulesSourcePath)
    {
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

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                FileSystemTasks.CopyDirectoryRecursively(dir, Path.Combine(platformDirectory, dirName), DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
            }

            foreach (var file in files)
            {
                FileSystemTasks.CopyFileToDirectory(file, platformDirectory);
            }
        }
    }

    public Target CloudDeploy => _ => _
        .DependsOn(PrepareDockerContext, BuildAndPush)
        .Executes(async () =>
        {
            var cloudClient = new VirtoCloudClient(CloudUrl, await GetCloudTokenAsync());
            var env = await cloudClient.GetEnvironment(EnvironmentName, SaaSOrganizationName);

            var envHelmParameters = env.Helm.Parameters;

            envHelmParameters["platform.image.repository"] = DockerImageName;
            envHelmParameters["platform.image.tag"] = DockerImageTag[0];

            await cloudClient.UpdateEnvironmentAsync(env);
        });

    public Target CloudAuth => _ => _
        .Executes(async () =>
        {
            var port = "60123";
            var listenerPrefix = $"http://localhost:{port}/";

            var listener = new HttpListener() {
                Prefixes = { listenerPrefix }
            };
            listener.Start();

            Log.Information("Openning browser window");
            var authUrl = $"{CloudUrl}/externalsignin?authenticationType={CloudAuthProvider}&returnUrl=/api/saas/token/{port}";
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

            var context = await listener.GetContextAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var apiKey = context.Request.QueryString["apiKey"];
            context.Response.Redirect($"{CloudUrl}/vcbuild/login/success");
            context.Response.Close();

            SaveCloudToken(apiKey);
        });

    private async Task<string> GetCloudTokenAsync()
    {
        if (!string.IsNullOrEmpty(CloudToken))
        {
            return CloudToken;
        }

        if (File.Exists(CloudTokenFile))
        {
            return await File.ReadAllTextAsync(CloudTokenFile);
        }

        Assert.Fail("Parameter CloudToken is required.");
        return string.Empty; // for sonar
    }

    private void SaveCloudToken(string token)
    {
        AbsolutePath cloudTokenDir = Path.GetDirectoryName(CloudTokenFile);
        cloudTokenDir.CreateDirectory();
        File.WriteAllText(CloudTokenFile, token);
    }

    public Target CloudDown => _ => _
        .Requires(() => EnvironmentName)
        .Executes(async () =>
        {
            var cloudClient = CreateVirtocloudClient(CloudUrl, await GetCloudTokenAsync());
            var envName = EnvironmentName;
            if (!string.IsNullOrWhiteSpace(AppProject))
            {
                envName = $"{AppProject}-{EnvironmentName}";
            }
            await cloudClient.EnvironmentsDeleteAsync(new List<string> { envName });
        });

    public Target CloudEnvList => _ => _
        .Executes(async () =>
        {
            var cloudClient = CreateVirtocloudClient(CloudUrl, await GetCloudTokenAsync());
            var envList = await cloudClient.EnvironmentsListAsync();
            Log.Information("There are {0} environments.", envList.Count);
            foreach ( var env in envList)
            {
                Log.Information("{0} - {1}", env.MetadataName, env.Status);
            }
        });

    public Target CloudEnvRestart => _ => _
        .Requires(() => EnvironmentName)
        .Executes(async () =>
        {
            var cloudClient = CreateVirtocloudClient(CloudUrl, await GetCloudTokenAsync());
            var env = await cloudClient.EnvironmentsGetEnvironmentAsync(EnvironmentName);
            Assert.NotNull(env, $"Environment {EnvironmentName} not found.");

            var parameterName = "platform.system.reload";
            env.Helm.Parameters[parameterName] = DateTime.Now.ToString();

            await cloudClient.EnvironmentsUpdateAsync(env);
        });

    public Target CloudInit => _ => _
        .After(PrepareDockerContext, DockerLogin, BuildImage, PushImage, BuildAndPush)
        .Requires(() => EnvironmentName)
        .Executes(async () =>
        {
            var model = new NewEnvironmentModel
            {
                Name = EnvironmentName,
                AppProjectId = AppProject,
                ServicePlan = ServicePlan,
                Cluster = ClusterName,
                DbProvider = DbProvider,
                DbName = DbName,
                Helm = new HelmObject(new Dictionary<string, string> ())
            };

            if(!string.IsNullOrEmpty(DockerImageName))
            {
                model.Helm.Parameters["platform.image.repository"] = DockerImageName;
            }

            if(!DockerImageTag.IsNullOrEmpty())
            {
                model.Helm.Parameters["platform.image.tag"] = DockerImageTag[0];
            }


            var cloudClient = CreateVirtocloudClient(CloudUrl, await GetCloudTokenAsync());
            await cloudClient.EnvironmentsCreateAsync(model);
        });

    public Target CloudEnvLogs => _ => _
        .Requires(() => EnvironmentName)
        .Executes(async () =>
        {
            var cloudClient = CreateVirtocloudClient(CloudUrl, await GetCloudTokenAsync());
            var logs = await cloudClient.EnvironmentsGetEnvironmentLogsAsync(EnvironmentName);
            logs = string.Join(Environment.NewLine, logs.Split("\\n"));
            Console.WriteLine(logs);
        });

    public Target CloudUp => _ => _
        .DependsOn(PrepareDockerContext, BuildAndPush, CloudInit);

    private static SaaSDeploymentApi CreateVirtocloudClient(string url, string token)
    {
        var config = new VirtoCloud.Client.Client.Configuration();
        config.BasePath = url;
        config.AccessToken = token;
        config.DefaultHeaders.Add("api_key", token);
        return new SaaSDeploymentApi(config);
    }
}
