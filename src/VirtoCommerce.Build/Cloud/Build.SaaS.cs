using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cloud.Client;
using Cloud.Models;
using Extensions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using VirtoCloud.Client.Api;
using VirtoCloud.Client.Model;

namespace VirtoCommerce.Build;

internal partial class Build
{
    // Missing auth provider variable
    private static string _cloudAuthProvider = "GitHub";

    private static string _saasOrganizationName;

    [Parameter("Use Azure AD as SaaS Auth Provider")]
    public bool AzureAD;

    private static string _environmentName;
    [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
    [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }

    [Parameter("Config file for Argo Application Service")]
    public string ArgoConfigFile { get; set; }

    [Parameter("Path to the manifest of environment")]
    public string Manifest { get; set; }

    [Parameter("Routes file for the environment")]
    public string RoutesFile { get; set; }

    [Parameter("Array of Helm parameters")]
    public HelmParameter[] HelmParameters { get; set; }

    [Parameter("Argo Application Name")] public string ArgoAppName { get; set; }

    [Parameter("Health Status that need to be awaited")]
    public string HealthStatus { get; set; }

    [Parameter("SyncStatus that need to be awaited")]
    public string SyncStatus { get; set; }

    [Parameter("Delay between requests in seconds")]
    public static int Delay { get; set; } = 10;

    [Parameter("Number of attempts before fail")]
    public static int AttemptsNumber { get; set; } = 100;

    [Parameter("SaaS Portal")] public static string CloudUrl { get; set; } = "https://portal.virtocommerce.cloud";
    [Parameter("SaaS Token")] public static string CloudToken { get; set; }

    [Parameter("Path for the file with SaaS Token")]
    public static string CloudTokenFile { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud");

    [Parameter("App Project Name")]
    public string AppProject { get => SaaSOrganizationName; set => SaaSOrganizationName = value?.ToLowerInvariant(); }

    [Parameter("Cloud Environment Name")]
    public static string EnvironmentName { get => _environmentName; set => _environmentName = value?.ToLowerInvariant(); }

    [Parameter("Cloud Environment Service Plan")]
    public static string ServicePlan { get; set; } = "F1";

    [Parameter("Cloud Environment Cluster Name")]
    public static string ClusterName { get; set; }

    [Parameter("Cloud Environment Db Provider")]
    public static string DbProvider { get; set; }

    [Parameter("Cloud Environment Db Name")]
    public static string DbName { get; set; }

    [Parameter("Organization name", Name = "Organization")]
    public static string SaaSOrganizationName
    {
        get => _saasOrganizationName;
        set => _saasOrganizationName = value?.ToLowerInvariant();
    }

    [Parameter("Url to Dockerfile which will use to build the docker image in the CloudDeploy/CloudUp Target")]
    public static string DockerfileUrl { get; set; } =
        "https://raw.githubusercontent.com/VirtoCommerce/vc-docker/feat/net8/linux/platform/Dockerfile";

    [Parameter("Url to Wake-script which will use to build the docker image in the CloudDeploy/CloudUp Target")]
    public static string WaitScriptUrl { get; set; } =
        "https://raw.githubusercontent.com/VirtoCommerce/vc-docker/feat/net8/linux/platform/wait-for-it.sh";

    // Refactored targets - using methods instead of inline execution
    public Target CloudEnvStatus
    {
        get => _ => _
            .Executes(async () => await CloudEnvStatusMethod(CloudUrl, EnvironmentName, HealthStatus, SyncStatus));
    }

    public Target CloudEnvSetParameter
    {
        get => _ => _
            .Executes(async () => await CloudEnvSetParameterMethod(CloudUrl, EnvironmentName,
                HelmParameters?.Select(h => $"{h.Name}={h.Value}").ToArray(), SaaSOrganizationName));
    }

    public Target CloudEnvUpdate
    {
        get => _ => _
            .Executes(async () => await CloudEnvUpdateMethod(Manifest ?? ArgoConfigFile, RoutesFile, AppProject, CloudUrl));
    }

    public Target CloudDeploy
    {
        get => _ => _
            .DependsOn(PrepareDockerContext, BuildAndPush)
            .Executes(async () => await CloudDeployMethod(EnvironmentName, DockerUsername, DockerPassword,
                DockerRegistryUrl, DockerImageName, DockerImageTag?[0], SaaSOrganizationName, CloudUrl, CloudToken));
    }

    public Target CloudAuth
    {
        get => _ => _
            .Executes(async () => await CloudAuthMethod(CloudUrl, CloudTokenFile, CloudToken, AzureAD));
    }

    public Target CloudDown
    {
        get => _ => _
            .Requires(() => EnvironmentName)
            .Executes(async () => await CloudDownMethod(CloudUrl, EnvironmentName, SaaSOrganizationName));
    }

    public Target CloudEnvList
    {
        get => _ => _
            .Executes(async () => await CloudEnvListMethod(CloudUrl));
    }

    public Target CloudEnvRestart
    {
        get => _ => _
            .Requires(() => EnvironmentName)
            .Executes(async () => await CloudEnvRestartMethod(CloudUrl, EnvironmentName));
    }

    public Target CloudInit
    {
        get => _ => _
            .After(PrepareDockerContext, DockerLogin, BuildImage, PushImage, BuildAndPush)
            .Requires(() => EnvironmentName)
            .Executes(async () =>
                await CloudInitMethod(EnvironmentName, ServicePlan, ClusterName, SaaSOrganizationName, AppProject,
                    DbProvider, DbName, CloudUrl));
    }

    public Target CloudEnvLogs
    {
        get => _ => _
            .Requires(() => EnvironmentName)
            .Executes(async () => await CloudEnvLogsMethod(CloudUrl, EnvironmentName, null, 0, null));
    }

    public Target CloudUp
    {
        get => _ => _
            .DependsOn(PrepareDockerContext, BuildAndPush, CloudInit);
    }

    public Target CloudDownloadManifest
    {
        get => _ => _
            .Executes(async () =>
            {
                var cloudClient = new VirtoCloudClient(CloudUrl, await GetCloudTokenAsync());
                var manifest = await cloudClient.GetManifest(EnvironmentName, SaaSOrganizationName);
                File.WriteAllText(
                    string.IsNullOrWhiteSpace(Manifest)
                        ? Path.Combine(Directory.GetCurrentDirectory(), $"{EnvironmentName}.yml")
                        : Manifest, manifest);
            });
    }

    public Target PrepareDockerContext
    {
        get => _ => _
            .Before(DockerLogin, BuildImage, PushImage, CloudInit)
            .Triggers(InitPlatform)
            .Executes(async () => await PrepareDockerContextMethod());
    }

    private static async Task PrepareDockerContextMethod()
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

        DockerImageTag ??= [DateTime.Now.ToString("MMddyyHHmmss")];
        DockerfilePath = dockerfilePath;
        DiscoveryPath = modulesPath;
        ProbingPath = Path.Combine(platformDirectory, "app_data", "modules");
        AppsettingsPath = Path.Combine(platformDirectory, "appsettings.json");
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
            var directories = Directory.GetDirectories(RootDirectory).Where(d =>
                !modulesSourcePath.Contains(d) && !PathConstruction.IsDescendantPath(nukeDir, d)
                                               && !ArtifactsDirectory.Contains(d)).ToArray();
            var files = Directory.GetFiles(RootDirectory);

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                dir.ToAbsolutePath().Copy(Path.Combine(platformDirectory, dirName),
                    ExistsPolicy.MergeAndOverwriteIfNewer);
            }

            foreach (var file in files)
            {
                file.ToAbsolutePath().CopyToDirectory(platformDirectory);
            }
        }
    }

    private static void CopyModules(AbsolutePath modulesPath, AbsolutePath modulesSourcePath)
    {
        // Copy modules
        var modulesDirectories = Directory.EnumerateDirectories(modulesSourcePath);
        foreach (var directory in modulesDirectories)
        {
            var webProjects = Directory.EnumerateFiles(directory, "*.Web.csproj");
            var moduleDirectoryName = Path.GetFileName(directory);
            var moduleDestinationPath = Path.Combine(modulesPath, moduleDirectoryName);
            if (!webProjects.Any())
            {
                directory.ToAbsolutePath().Copy(moduleDestinationPath, ExistsPolicy.MergeAndOverwriteIfNewer);
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
                    contentSource.ToAbsolutePath().Copy(contentDestination, ExistsPolicy.MergeAndOverwriteIfNewer);
                }

                var moduleManifestPath = webProject.Directory / "module.manifest";
                moduleManifestPath.CopyToDirectory(moduleDestinationPath, ExistsPolicy.FileOverwriteIfNewer);
            }
        }
    }

    // Extracted method implementations
    public static async Task CloudAuthMethod(string cloudUrl, string cloudTokenFile, string cloudToken, bool azureAd)
    {
        string apiKey;
        if (string.IsNullOrWhiteSpace(cloudToken))
        {
            var port = "60123";
            var listenerPrefix = $"http://localhost:{port}/";
            var listener = new HttpListener { Prefixes = { listenerPrefix } };
            listener.Start();
            Log.Information("Opening browser window");
            if (azureAd)
            {
                _cloudAuthProvider = "AzureAD";
            }

            var authUrl =
                $"{cloudUrl}/externalsignin?authenticationType={_cloudAuthProvider}&returnUrl=/api/saas/token/{port}";
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });
            var context = await listener.GetContextAsync();
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            apiKey = context.Request.QueryString["apiKey"];
            context.Response.Redirect($"{cloudUrl}/vcbuild/login/success");
            context.Response.Close();
        }
        else
        {
            apiKey = cloudToken;
        }

        AbsolutePath cloudTokenDir = Path.GetDirectoryName(cloudTokenFile);
        cloudTokenDir.CreateDirectory();
        await File.WriteAllTextAsync(cloudTokenFile, apiKey);
    }

    private static SaaSDeploymentApi CreateVirtocloudClient(string url, string token)
    {
        var config = new VirtoCloud.Client.Client.Configuration();
        config.BasePath = url;
        config.AccessToken = token;
        config.DefaultHeaders.Add("api_key", token);
        return new SaaSDeploymentApi(config);
    }

    private static async Task<string> GetCloudTokenAsync()
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

    public static async Task CloudInitMethod(string environmentName, string servicePlan, string clusterName,
        string organization, string appProject, string dbProvider, string dbName, string cloudUrl)
    {
        var model = new NewEnvironmentModel
        {
            Name = environmentName,
            AppProjectId = organization ?? appProject,
            ServicePlan = servicePlan,
            Cluster = clusterName,
            DbProvider = dbProvider,
            DbName = dbName,
            Helm = new HelmObject(new Dictionary<string, string>())
        };

        if (!string.IsNullOrEmpty(DockerImageName))
        {
            model.Helm.Parameters["platform.image.repository"] = DockerImageName;
        }

        if (!DockerImageTag.IsNullOrEmpty())
        {
            model.Helm.Parameters["platform.image.tag"] = DockerImageTag[0];
        }

        var cloudClient = CreateVirtocloudClient(cloudUrl, await GetCloudTokenAsync());
        await cloudClient.EnvironmentsCreateAsync(model);
    }

    public static async Task CloudUpMethod(string cloudUrl, string environmentName, string dockerUsername, string dockerPassword,
        string servicePlan, string dockerRegistryUrl, string dockerImageName, string dockerImageTag,
        string clusterName, string organization, string dbProvider, string dbName)
    {
        //PrepareDockerContext, BuildAndPush, CloudInit
        await PrepareDockerContextMethod();
        // BuildAndPush
        await CloudInitMethod(environmentName, servicePlan, clusterName, organization, null, dbProvider, dbName, cloudUrl);
    }

    public static async Task CloudDeployMethod(string environmentName, string dockerUsername, string dockerPassword,
        string dockerRegistryUrl, string dockerImageName, string dockerImageTag, string organization, string cloudUrl,
        string cloudToken = null)
    {
        // Set the parameters for PrepareDockerContext and build workflow
        EnvironmentName = environmentName;
        DockerUsername = dockerUsername;
        DockerPassword = dockerPassword;
        DockerRegistryUrl = dockerRegistryUrl;

        if (!string.IsNullOrEmpty(dockerImageName))
        {
            DockerImageName = dockerImageName;
        }

        if (!string.IsNullOrEmpty(dockerImageTag))
        {
            DockerImageTag = [dockerImageTag];
        }

        // Execute the workflow: PrepareDockerContext -> BuildAndPush -> Update Environment
        await PrepareDockerContextMethod();

        // Call the BuildAndPush target with proper type specification
        Execute<Build>(x => x.BuildAndPush);

        var cloudClient = new VirtoCloudClient(cloudUrl, cloudToken ?? await GetCloudTokenAsync());
        var env = await cloudClient.GetEnvironmentAsync(environmentName, organization);

        var envHelmParameters = env.Helm.Parameters;
        envHelmParameters["platform.image.repository"] = DockerImageName;
        envHelmParameters["platform.image.tag"] = DockerImageTag[0];

        await cloudClient.UpdateEnvironmentAsync(env);
    }

    public static async Task CloudDownMethod(string cloudUrl, string environmentName, string organization)
    {
        var cloudClient = CreateVirtocloudClient(cloudUrl, await GetCloudTokenAsync());
        var envName = environmentName;
        if (!string.IsNullOrWhiteSpace(organization))
        {
            envName = $"{organization}-{environmentName}";
        }

        await cloudClient.EnvironmentsDeleteAsync([envName]);
    }

    public static async Task CloudEnvListMethod(string cloudUrl)
    {
        var cloudClient = CreateVirtocloudClient(cloudUrl, await GetCloudTokenAsync());
        var envList = await cloudClient.EnvironmentsListAsync();
        Log.Information("There are {0} environments.", envList.Count);
        foreach (var env in envList)
        {
            Log.Information("{0} - {1}", env.MetadataName, env.Status);
        }
    }

    public static async Task CloudEnvRestartMethod(string cloudUrl, string environmentName)
    {
        var cloudClient = CreateVirtocloudClient(cloudUrl, await GetCloudTokenAsync());
        var env = await cloudClient.EnvironmentsGetEnvironmentAsync(environmentName);
        env.NotNull($"Environment {environmentName} not found.");

        const string parameterName = "platform.system.reload";
        env.Helm.Parameters[parameterName] = DateTime.Now.ToString();

        await cloudClient.EnvironmentsUpdateAsync(env);
    }

    public static async Task CloudEnvLogsMethod(string cloudUrl, string environmentName, string filter, int tail, string resourceName)
    {
        var cloudClient = CreateVirtocloudClient(cloudUrl, await GetCloudTokenAsync());
        var logs = await cloudClient.EnvironmentsGetEnvironmentLogsAsync(environmentName, filter,
            tail == 0 ? null : tail, resourceName);
        logs = string.Join(Environment.NewLine, logs.Split("\\n"));
        Console.WriteLine(logs);
    }

    public static async Task CloudEnvStatusMethod(string cloudUrl, string environmentName, string healthStatus, string syncStatus)
    {
        var isSuccess = false;
        var cloudClient = new VirtoCloudClient(cloudUrl, await GetCloudTokenAsync());
        for (var i = 0; i < AttemptsNumber; i++)
        {
            Log.Information("Attempt #{I}", i + 1);
            var env = await cloudClient.GetEnvironmentAsync(environmentName);
            Log.Information(
                "Actual Health Status is {EnvStatus} - expected is {NotExpected}\n Actual Sync Status is {EnvSyncStatus} - expected is {S}", env.Status, healthStatus ?? "Not expected", env.SyncStatus, syncStatus ?? "Not expected");
            if (CheckAppServiceStatus(healthStatus, env.Status) &&
                CheckAppServiceStatus(syncStatus, env.SyncStatus))
            {
                isSuccess = true;
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(Delay));
        }

        Assert.True(isSuccess,
            $"Statuses {healthStatus} {syncStatus} were not obtained for the number of attempts: {AttemptsNumber}");
    }

    private static bool CheckAppServiceStatus(string expected, string actual)
    {
        return expected == actual || string.IsNullOrEmpty(expected);
    }

    public static async Task CloudEnvSetParameterMethod(string cloudUrl, string environmentName, string[] helmParameters,
        string organization)
    {
        var cloudClient = new VirtoCloudClient(cloudUrl, await GetCloudTokenAsync());

        var helmParameterObjects = helmParameters?.Select(p =>
        {
            var parts = p.Split('=', 2);
            return new HelmParameter { Name = parts[0], Value = parts.Length > 1 ? parts[1] : "" };
        }).ToArray() ?? [];

        var isProgressing = await WaitForEnvironmentState(
            async () => await cloudClient.GetEnvironmentAsync(environmentName, organization),
            env => env.Status != "Progressing", Delay, AttemptsNumber);

        if (!isProgressing)
        {
            Assert.Fail("Environment is in 'Progressing' status for too long.");
        }

        var env = await cloudClient.GetEnvironmentAsync(environmentName, organization);

        var envHelmParameters = env.Helm.Parameters;
        foreach (var parameter in helmParameterObjects)
        {
            envHelmParameters[parameter.Name] = parameter.Value;
        }

        await cloudClient.UpdateEnvironmentAsync(env);
    }

    private static async Task<bool> WaitForEnvironmentState(Func<Task<CloudEnvironment>> cloudEnvProvider, Func<CloudEnvironment, bool> condition, int delay, int attempts)
    {
        for (var i = 0; i < attempts; i++)
        {
            var env = await cloudEnvProvider();
            Log.Information("Attempt #{I}", i + 1);
            Log.Information("Health Status is {EnvStatus}\nSync Status is {EnvSyncStatus}", env.Status, env.SyncStatus);
            if (condition(env))
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromSeconds(delay));
        }

        return false;
    }

    public static async Task CloudEnvUpdateMethod(string manifest, string routesFile, string appProject, string cloudUrl, string cloudToken = null)
    {
        var cloudClient = new VirtoCloudClient(cloudUrl, cloudToken ?? await GetCloudTokenAsync());
        var rawYaml = await File.ReadAllTextAsync(manifest);
        string routesFileContent = null;
        if (!string.IsNullOrWhiteSpace(routesFile))
        {
            routesFileContent = await File.ReadAllTextAsync(routesFile);
        }

        await cloudClient.UpdateEnvironmentAsync(rawYaml, appProject, routesFileContent);
    }
}
