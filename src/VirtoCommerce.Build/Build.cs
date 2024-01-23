using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Locator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.SonarScanner;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using Utils;
using VirtoCommerce.Build.Utils;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Formatting = Newtonsoft.Json.Formatting;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Project = Nuke.Common.ProjectModel.Project;

namespace VirtoCommerce.Build;

[UnsetVisualStudioEnvironmentVariables]
internal partial class Build : NukeBuild
{
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    private static readonly string[] _moduleContentFolders = { "dist", "Localizations", "Scripts", "Content" };

    private static readonly string[] _sonarLongLiveBranches = { "master", "develop", "dev", "main" };
    private static readonly HttpClient _httpClient = new();
    private static int? _exitCode;

    private enum ExitCodes
    {
        ModuleCouldNotBeLoaded = 126,
        GithubNoModuleFound = 404,
        HttpRequestConflict = 409,
        GithubReleaseAlreadyExists = 422,
        GitNothingToCommit = 423
    }

    private static bool ClearTempBeforeExit { get; set; }

    public static Solution Solution
    {
        get
        {
            var solutions = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln", SearchOption.TopDirectoryOnly);
            if (solutions.Any())
            {
                return ProjectModelTasks.ParseSolution(solutions[0]);
            }

            Log.Warning("No solution files found in the current directory");
            return new Solution();
        }
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public static Configuration Configuration { get; set; } =
        IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("API key for the specified source")]
    public static string ApiKey { get; set; }

    [Parameter] public static string Source { get; set; } = "https://api.nuget.org/v3/index.json";

    [Parameter]
    public static string GlobalModuleIgnoreFileUrl { get; set; } =
        "https://raw.githubusercontent.com/VirtoCommerce/vc-platform/dev/module.ignore";

    [Parameter] public static string SonarAuthToken { get; set; } = "";

    [Parameter] public static string SonarUrl { get; set; } = "https://sonarcloud.io";

    [Parameter] public static AbsolutePath CoverageReportPath { get; set; } = RootDirectory / ".tmp" / "coverage.xml";

    [Parameter] public static string TestsFilter { get; set; } = "Category!=IntegrationTest";

    [Parameter("URL of Swagger Validation API")]
    public static string SwaggerValidatorUri { get; set; } = "https://validator.swagger.io/validator/debug";

    [Parameter("GitHub user for release creation")]
    public static string GitHubUser { get; set; }

    [Parameter("GitHub user security token for release creation")]
    public static string GitHubToken { get; set; }

    [Parameter("True - prerelease, False - release")]
    public static bool PreRelease { get; set; }

    [Parameter("True - Pull Request")] public static bool PullRequest { get; set; }

    [Parameter("Path to folder with git clones of modules repositories")]
    public static AbsolutePath ModulesFolderPath { get; set; }

    [Parameter("Repo Organization/User")] public static string RepoOrg { get; set; } = "VirtoCommerce";

    [Parameter("Repo Name")] public static string RepoName { get; set; }

    [Parameter("Sonar Organization (\"virto-commerce\" by default)")]
    public static string SonarOrg { get; set; } = "virto-commerce";

    [Parameter("Path to NuGet config")] public static AbsolutePath NugetConfig { get; set; }

    [Parameter("Swagger schema path")] public static AbsolutePath SwaggerSchemaPath { get; set; }

    [Parameter("Path to modules.json")] public static string ModulesJsonName { get; set; } = "modules_v3.json";

    [Parameter("Full URI of module artifact")]
    public static string CustomModulePackageUri { get; set; }

    [Parameter("Path to packageJson")] public static string PackageJsonPath { get; set; } = "package.json";

    [Parameter("Path to Release Notes File")]
    public static AbsolutePath ReleaseNotes { get; set; }

    [Parameter("VersionTag for module.manifest and Directory.Build.props")]
    public static string CustomVersionPrefix { get; set; }

    [Parameter("VersionSuffix for module.manifest and Directory.Build.props")]
    public static string CustomVersionSuffix { get; set; }

    [Parameter("Release branch")] public static string ReleaseBranch { get; set; }

    [Parameter("Branch Name for SonarQube")]
    public static string SonarBranchName { get; set; }

    [Parameter("Target Branch Name for SonarQube")]
    public static string SonarBranchNameTarget { get; set; } = "dev";

    [Parameter("PR Base for SonarQube")] public static string SonarPRBase { get; set; }

    [Parameter("PR Branch for SonarQube")] public static string SonarPRBranch { get; set; }

    [Parameter("PR Number for SonarQube")] public static string SonarPRNumber { get; set; }

    [Parameter("GitHub Repository for SonarQube")]
    public static string SonarGithubRepo { get; set; }

    [Parameter("PR Provider for SonarQube")]
    public static string SonarPRProvider { get; set; }

    [Parameter("Modules.json repo URL")]
    public static string ModulesJsonRepoUrl { get; set; } = "https://github.com/VirtoCommerce/vc-modules.git";

    [Parameter("Force parameter for git checkout")]
    public static bool Force { get; set; }

    [Parameter("Path to Artifacts Directory")]
    public static AbsolutePath ArtifactsDirectory { get; set; } = RootDirectory / "artifacts";

    [Parameter("Directory containing modules.json")]
    public static string ModulesJsonDirectoryName { get; set; } = "vc-modules";

    [Parameter("Default (start) project name")]
    public static string DefaultProject { get; set; } = ".Web";

    [Parameter("Main branch")] public static string MainBranch { get; set; } = "master";

    [Parameter("Http tasks timeout in seconds")]
    public static int HttpTimeout { get; set; } = 15;

    protected static GitRepository GitRepository => GitRepository.FromLocalDirectory(RootDirectory / ".git");

    protected static AbsolutePath SourceDirectory => RootDirectory / "src";
    protected static AbsolutePath TestsDirectory => RootDirectory / "tests";
    protected static AbsolutePath SamplesDirectory => RootDirectory / "samples";

    protected static AbsolutePath ModulesLocalDirectory => ArtifactsDirectory / ModulesJsonDirectoryName;

    protected static Project WebProject => Solution?.AllProjects.FirstOrDefault(x =>
        x.Name.EndsWith(DefaultProject) || x.Name.EndsWith("VirtoCommerce.Storefront") ||
        x.Name.EndsWith("VirtoCommerce.Build"));

    protected static AbsolutePath ModuleManifestFile => WebProject?.Directory / "module.manifest";
    protected static AbsolutePath ModuleIgnoreFile => RootDirectory / "module.ignore";
    protected static AbsolutePath ModuleKeepFile => RootDirectory / "module.keep";
    protected static AbsolutePath WebDirectory => WebProject?.Directory;

    protected static Microsoft.Build.Evaluation.Project MSBuildProject => WebProject?.GetMSBuildProject();

    protected string VersionPrefix => IsTheme
        ? GetThemeVersion(PackageJsonPath)
        : MSBuildProject.GetProperty("VersionPrefix")?.EvaluatedValue;

    protected static string VersionSuffix => MSBuildProject?.GetProperty("VersionSuffix")?.EvaluatedValue;

    protected static string ReleaseVersion => MSBuildProject?.GetProperty("PackageVersion")?.EvaluatedValue ??
                                       WebProject.GetProperty("Version");

    protected static bool IsTheme => string.IsNullOrEmpty(Solution?.Directory);

    protected static ModuleManifest ModuleManifest => ManifestReader.Read(ModuleManifestFile);

    protected static AbsolutePath ModuleOutputDirectory => ArtifactsDirectory / ModuleManifest.Id;

    protected static AbsolutePath DirectoryBuildPropsPath => Solution.Directory / "Directory.Build.props";

    protected string ZipFileName => IsModule
        ? $"{ModuleManifest.Id}_{ReleaseVersion}.zip"
        : $"{WebProject.Solution.Name}.{ReleaseVersion}.zip";

    protected string ZipFilePath => ArtifactsDirectory / ZipFileName;
    protected static string GitRepositoryName => GitRepository.Identifier.Split('/')[1];

    protected static string ModulePackageUrl => CustomModulePackageUri.IsNullOrEmpty()
        ? $"https://github.com/VirtoCommerce/{GitRepositoryName}/releases/download/{ReleaseVersion}/{ModuleManifest.Id}_{ReleaseVersion}.zip"
        : CustomModulePackageUri;

    protected GitRepository ModulesRepository => GitRepository.FromUrl(ModulesJsonRepoUrl);

    protected bool IsModule => ModuleManifestFile.FileExists();

    public Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            var searchPattern = new[] { "**/bin", "**/obj" };
            CleanSolution(searchPattern);
        });

    public Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(settings => settings
                .SetProjectFile(Solution)
                .When(NugetConfig != null, c => c
                    .SetConfigFile(NugetConfig))
            );
        });

    public Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            //For platform take nuget package description from Directory.Build.props
            var settings = new DotNetPackSettings()
                .SetProject(Solution)
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .EnableIncludeSymbols()
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                .SetProperty("PackageOutputPath", ArtifactsDirectory)
                .SetVersion(ReleaseVersion);
            DotNetPack(settings);
        });

    public Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjects = Solution.GetProjects("*.Test|*.Tests|*.Testing");
            var outPath = RootDirectory / ".tmp";

            foreach (var testProjectPath in testProjects.Select(p=> p.Path).ToArray())
            {
                DotNet($"add \"{testProjectPath}\" package coverlet.collector");

                var testSetting = new DotNetTestSettings()
                    .SetProjectFile(testProjectPath)
                    .SetConfiguration(Configuration)
                    .SetFilter(TestsFilter)
                    .SetNoBuild(true)
                    .SetProcessLogOutput(true)
                    .SetResultsDirectory(outPath)
                    .SetDataCollector("XPlat Code Coverage");

                DotNetTest(testSetting);
            }

            var coberturaReports = outPath.GlobFiles("**/coverage.cobertura.xml");

            if (coberturaReports.Count > 0)
            {
                var reportGenerator = ToolResolver.GetPackageTool("dotnet-reportgenerator-globaltool",
                    "ReportGenerator.dll", "4.8.8", "netcoreapp3.0");
                reportGenerator.Invoke(
                    $"-reports:{outPath / "**/coverage.cobertura.xml"} -targetdir:{outPath} -reporttypes:SonarQube");
                var sonarCoverageReportPath = outPath.GlobFiles("SonarQube.xml").FirstOrDefault();

                if (sonarCoverageReportPath == null)
                {
                    Assert.Fail("No Coverage Report found");
                }

                FileSystemTasks.MoveFile(sonarCoverageReportPath, CoverageReportPath, FileExistsPolicy.Overwrite);
            }
            else
            {
                Log.Warning("No Coverage Reports found");
            }
        });

    public Target PublishPackages => _ => _
        .DependsOn(Pack)
        .Requires(() => ApiKey)
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg", "*.snupkg").OrderBy(p => p.ToString());

            DotNetLogger = CustomDotnetLogger;

            DotNetNuGetPush(settings => settings
                    .SetSource(Source)
                    .SetApiKey(ApiKey)
                    .SetSkipDuplicate(true)
                    .CombineWith(
                        packages, (cs, v) => cs
                            .SetTargetPath(v)),
                1,
                true);
        });

    public Target ChangeVersion => _ => _
        .Executes(() =>
        {
            if ((string.IsNullOrEmpty(VersionSuffix) && !CustomVersionSuffix.IsNullOrEmpty()) ||
                !CustomVersionPrefix.IsNullOrEmpty())
            {
                ChangeProjectVersion(CustomVersionPrefix, CustomVersionSuffix);
            }
        });

    public Target StartRelease => _ => _
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;
            var disableApproval = Environment.GetEnvironmentVariable("VCBUILD_DISABLE_RELEASE_APPROVAL");

            if (disableApproval.IsNullOrEmpty() && !Force)
            {
                Console.Write($"Are you sure you want to release {GitRepository.Identifier}? (y/N): ");
                var response = Console.ReadLine();

                if (string.Compare(response, "y", true, CultureInfo.InvariantCulture) != 0)
                {
                    Assert.Fail("Aborted");
                }
            }

            var checkoutCommand = new StringBuilder("checkout dev");

            if (Force)
            {
                checkoutCommand.Append(" --force");
            }

            GitTasks.Git(checkoutCommand.ToString());
            GitTasks.Git("pull");

            var version = IsTheme
                ? GetThemeVersion(PackageJsonPath)
                : ReleaseVersion;

            var releaseBranchName = $"release/{version}";
            Log.Information(Directory.GetCurrentDirectory());
            GitTasks.Git($"checkout -B {releaseBranchName}");
            GitTasks.Git($"push -u origin {releaseBranchName}");
        });

    public Target CompleteRelease => _ => _
        .After(StartRelease)
        .Executes(() =>
        {
            var currentBranch = GitTasks.GitCurrentBranch();
            //Master
            GitTasks.Git($"checkout {MainBranch}");
            GitTasks.Git("pull");
            GitTasks.Git($"merge {currentBranch}");
            GitTasks.Git($"push origin {MainBranch}");
            //Dev
            GitTasks.Git("checkout dev");
            GitTasks.Git($"merge {currentBranch}");
            IncrementVersionMinor();
            ChangeProjectVersion(CustomVersionPrefix);
            string filesToAdd;

            if (IsTheme)
            {
                filesToAdd = $"{PackageJsonPath}";
            }
            else
            {
                var manifestPath = IsModule ? RootDirectory.GetRelativePathTo(ModuleManifestFile) : "";
                filesToAdd = $"Directory.Build.props {manifestPath}";
            }

            GitTasks.Git($"add {filesToAdd}");
            GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
            GitTasks.Git("push origin dev");
            //remove release branch
            GitTasks.Git($"branch -d {currentBranch}");
            GitTasks.Git($"push origin --delete {currentBranch}");
        });

    public Target QuickRelease => _ => _
        .DependsOn(StartRelease, CompleteRelease);

    public Target StartHotfix => _ => _
        .Executes(() =>
        {
            GitTasks.Git($"checkout {MainBranch}");
            GitTasks.Git("pull");
            IncrementVersionPatch();
            var hotfixBranchName = $"hotfix/{CustomVersionPrefix}";
            Log.Information(Directory.GetCurrentDirectory());
            GitTasks.Git($"checkout -b {hotfixBranchName}");
            ChangeProjectVersion(CustomVersionPrefix);
            var manifestPath = IsModule ? RootDirectory.GetRelativePathTo(ModuleManifestFile) : "";
            GitTasks.Git($"add Directory.Build.props {manifestPath}");
            GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
            GitTasks.Git($"push -u origin {hotfixBranchName}");
        });

    public Target CompleteHotfix => _ => _
        .After(StartHotfix)
        .Executes(() =>
        {
            //workaround for run from sources
            var currentBranch = GitTasks.GitCurrentBranch();
            //Master
            GitTasks.Git($"checkout {MainBranch}");
            GitTasks.Git($"merge {currentBranch}");
            GitTasks.Git($"tag {VersionPrefix}");
            GitTasks.Git($"push origin {MainBranch}");
            //remove hotfix branch
            GitTasks.Git($"branch -d {currentBranch}");
            GitTasks.Git($"push origin --delete {currentBranch}");
        });

    public Target IncrementMinor => _ => _
        .Triggers(ChangeVersion)
        .Executes(IncrementVersionMinor);

    public Target IncrementPatch => _ => _
        .Triggers(ChangeVersion)
        .Executes(IncrementVersionPatch);

    public Target Publish => _ => _
        .DependsOn(Compile)
        .After(WebPackBuild, Test)
        .Executes(() => PublishMethod(WebProject,
                                    IsModule ? ModuleOutputDirectory / "bin" : ArtifactsDirectory / "publish",
                                    Configuration));

    private static void PublishMethod(Project webProject, string output, Configuration configuration)
    {
        Assert.NotNull(webProject, "Web Project is not found!");
        DotNetPublish(settings => settings
            .SetProcessWorkingDirectory(webProject.Directory)
            .EnableNoRestore()
            .SetOutput(output)
            .SetConfiguration(configuration));
    }

    public Target WebPackBuild => _ => _
        .Executes(() => WebPackBuildMethod(WebProject));

    private static void WebPackBuildMethod(Project webProject)
    {
        if (webProject != null && (webProject.Directory / "package.json").FileExists())
        {
            NpmTasks.Npm("ci", webProject.Directory);
            NpmTasks.NpmRun(settings =>
                settings.SetProcessWorkingDirectory(webProject.Directory).SetCommand("webpack:build"));
        }
        else
        {
            Log.Information("Nothing to build.");
        }
    }

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(settings => settings
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    public Target Compress => _ => _
        .DependsOn(Clean, WebPackBuild, Test, Publish)
        .Executes(CompressExecuteMethod);

    public Target GetManifestGit => _ => _
        .Before(UpdateManifest)
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;

            if (!ModulesLocalDirectory.DirectoryExists())
            {
                GitTasks.Git($"clone {ModulesRepository.HttpsUrl} {ModulesLocalDirectory}");
            }
            else
            {
                GitTasks.Git("pull", ModulesLocalDirectory);
            }
        });

    public Target UpdateManifest => _ => _
        .Before(PublishManifestGit)
        .After(GetManifestGit)
        .Executes(() =>
        {
            var manifest = ModuleManifest;
            manifest.PackageUrl = ModulePackageUrl;

            var modulesJsonFilePath = ModulesLocalDirectory / ModulesJsonName;
            var externalManifests =
                JsonConvert.DeserializeObject<List<ExternalModuleManifest>>(TextTasks.ReadAllText(modulesJsonFilePath));
            var externalManifest = externalManifests?.Find(x => x.Id == manifest.Id);

            if (externalManifest != null)
            {
                if (!manifest.VersionTag.IsNullOrEmpty() || !CustomVersionSuffix.IsNullOrEmpty())
                {
                    manifest.VersionTag = manifest.VersionTag.EmptyToNull() ?? CustomVersionSuffix;

                    var externalPrereleaseVersion =
                        externalManifest.Versions.FirstOrDefault(v => !v.VersionTag.IsNullOrEmpty());

                    if (externalPrereleaseVersion != null)
                    {
                        externalPrereleaseVersion.Dependencies = manifest.Dependencies;
                        externalPrereleaseVersion.Incompatibilities = manifest.Incompatibilities;
                        externalPrereleaseVersion.PlatformVersion = manifest.PlatformVersion;
                        externalPrereleaseVersion.ReleaseNotes = manifest.ReleaseNotes;
                        externalPrereleaseVersion.Version = manifest.Version;
                        externalPrereleaseVersion.VersionTag = manifest.VersionTag;
                        externalPrereleaseVersion.PackageUrl = manifest.PackageUrl;
                    }
                    else
                    {
                        externalManifest.Versions.Add(ExternalModuleManifestVersion.FromManifest(manifest));
                    }
                }
                else
                {
                    externalManifest.PublishNewVersion(manifest);
                }

                externalManifest.Title = manifest.Title;
                externalManifest.Description = manifest.Description;
                externalManifest.Authors = manifest.Authors;
                externalManifest.Copyright = manifest.Copyright;
                externalManifest.Groups = manifest.Groups;
                externalManifest.IconUrl = manifest.IconUrl;
                externalManifest.Id = manifest.Id;
                externalManifest.LicenseUrl = manifest.LicenseUrl;
                externalManifest.Owners = manifest.Owners;
                externalManifest.ProjectUrl = manifest.ProjectUrl;
                externalManifest.RequireLicenseAcceptance = manifest.RequireLicenseAcceptance;
                externalManifest.Tags = manifest.Tags;
            }
            else
            {
                externalManifests?.Add(ExternalModuleManifest.FromManifest(manifest));
            }

            TextTasks.WriteAllText(modulesJsonFilePath,
                JsonConvert.SerializeObject(externalManifests, Formatting.Indented));
        });

    public Target PublishManifestGit => _ => _
        .After(UpdateManifest)
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;
            GitTasks.Git($"commit -am \"{ModuleManifest.Id} {ReleaseVersion}\"", ModulesLocalDirectory);
            GitTasks.Git("push origin HEAD:master -f", ModulesLocalDirectory);
        });

    public Target PublishModuleManifest => _ => _
        .DependsOn(GetManifestGit, UpdateManifest, PublishManifestGit);

    public Target SwaggerValidation => _ => _
        .DependsOn(Publish)
        .Requires(() => !IsModule)
        .Executes(async () =>
        {
            var swashbuckle = ToolResolver.GetPackageTool("Swashbuckle.AspNetCore.Cli", "dotnet-swagger.dll",
                framework: "netcoreapp3.0");
            var projectPublishPath = ArtifactsDirectory / "publish" / $"{WebProject.Name}.dll";
            var swaggerJsonPath = ArtifactsDirectory / "swagger.json";
            var currentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(RootDirectory / "src" / "VirtoCommerce.Platform.Web");
            swashbuckle.Invoke($"tofile --output {swaggerJsonPath} {projectPublishPath} VirtoCommerce.Platform");
            Directory.SetCurrentDirectory(currentDirectory);

            var responseContent = await SendSwaggerSchemaToValidator(_httpClient, swaggerJsonPath, SwaggerValidatorUri);
            var responseObject = JObject.Parse(responseContent);
            var schemaValidationMessages = responseObject["schemaValidationMessages"];

            if (schemaValidationMessages != null)
            {
                foreach (var message in schemaValidationMessages)
                {
                    Log.Information(message.ToString());
                }

                if (schemaValidationMessages.Any(t => (string)t["level"] == "error"))
                {
                    Assert.Fail("Schema Validation Messages contains error");
                }
            }
        });

    public Target ValidateSwaggerSchema => _ => _
        .Requires(() => SwaggerSchemaPath != null)
        .Executes(async () =>
        {
            var responseContent =
                await SendSwaggerSchemaToValidator(_httpClient, SwaggerSchemaPath, SwaggerValidatorUri);
            var responseObject = JObject.Parse(responseContent);

            if (responseObject.TryGetValue("schemaValidationMessages", out var validationMessages))
            {
                foreach (var message in validationMessages)
                {
                    Log.Information(message.ToString());
                }

                if (validationMessages.Any(t => (string)t["level"] == "error"))
                {
                    Assert.Fail("Schema Validation Messages contains error");
                }
            }
            else
            {
                Log.Warning("There are no validation messages from validator");
            }
        });

    public Target SonarQubeStart => _ => _
        .Executes(() =>
        {
            Log.Information($"IsServerBuild = {IsServerBuild}");
            var branchName = SonarBranchName ?? GitRepository.Branch;
            var branchNameTarget = SonarBranchNameTarget ?? GitRepository.Branch;
            Log.Information($"BRANCH_NAME = {branchName}");

            SonarScannerTasks.SonarScannerBegin(c => c
                .SetFramework("net5.0")
                .SetName(RepoName)
                .SetProjectKey($"{RepoOrg}_{RepoName}")
                .SetVersion(ReleaseVersion)
                .SetServer(SonarUrl)
                .SetLogin(SonarAuthToken)
                .SetOrganization(SonarOrg)
                .SetGenericCoveragePaths(CoverageReportPath)
                .When(PullRequest, cc => cc
                    .SetPullRequestBase(SonarPRBase ?? Environment.GetEnvironmentVariable("CHANGE_TARGET"))
                    .SetPullRequestBranch(SonarPRBranch ?? Environment.GetEnvironmentVariable("CHANGE_TITLE"))
                    .SetPullRequestKey(SonarPRNumber ?? Environment.GetEnvironmentVariable("CHANGE_ID"))
                    .SetProcessArgumentConfigurator(args =>
                    {
                        if (!string.IsNullOrEmpty(SonarPRProvider))
                        {
                            args = args.Add($"/d:sonar.pullrequest.provider={SonarPRProvider}");
                        }

                        if (!string.IsNullOrEmpty(SonarGithubRepo))
                        {
                            args = args.Add("/d:sonar.pullrequest.github.repository={value}", SonarGithubRepo);
                        }

                        return args;
                    }))
                .When(!PullRequest, cc => cc
                    .SetBranchName(branchName)
                    .SetProcessArgumentConfigurator(args =>
                    {
                        if (!_sonarLongLiveBranches.Contains(branchName))
                        {
                            args = args.Add($"/d:\"sonar.branch.target={branchNameTarget}\"");
                        }

                        return args;
                    })
                )
            );
        });

    public Target SonarQubeEnd => _ => _
        .After(SonarQubeStart)
        .DependsOn(Compile)
        .Executes(() =>
        {
            const string framework = "net5.0";
            if (OperatingSystem.IsLinux())
            {
                const string sonarScript = "sonar-scanner";
                var sonarScannerShPath = ToolPathResolver.GetPackageExecutable("dotnet-sonarscanner",
                        sonarScript, framework: framework)
                    .Replace("netcoreapp2.0", "net5.0")
                    .Replace("netcoreapp3.0", "net5.0");
                var sonarScannerShRightPath = Directory.GetParent(sonarScannerShPath)?.Parent?.FullName ?? string.Empty;
                var tmpFile = TemporaryDirectory / sonarScript;
                FileSystemTasks.MoveFile(sonarScannerShPath, tmpFile);
                FileSystemTasks.DeleteDirectory(sonarScannerShRightPath);
                var sonarScriptDestinationPath = Path.Combine(sonarScannerShRightPath, sonarScript);
                FileSystemTasks.MoveFile(tmpFile, sonarScriptDestinationPath);
                Log.Information($"{sonarScript} path: {sonarScriptDestinationPath}");
                var chmod = ToolResolver.GetPathTool("chmod");
                chmod.Invoke($"+x {sonarScriptDestinationPath}");
            }

            SonarScannerTasks.SonarScannerEnd(c => c
                .SetFramework(framework)
                .SetLogin(SonarAuthToken));
        });

    public Target StartAnalyzer => _ => _
        .DependsOn(SonarQubeStart, SonarQubeEnd)
        .Executes(() =>
        {
            Log.Information("Sonar validation done.");
        });

    public Target MassPullAndBuild => _ => _
        .Requires(() => ModulesFolderPath)
        .Executes(() =>
        {
            if (ModulesFolderPath.DirectoryExists())
            {
                foreach (var moduleDirectory in Directory.GetDirectories(ModulesFolderPath))
                {
                    var isGitRepository =
                        FileSystemTasks.FindParentDirectory(moduleDirectory, x => x.GetDirectories(".git").Any()) !=
                        null;

                    if (isGitRepository)
                    {
                        GitTasks.Git("pull", moduleDirectory);
                        ProcessTasks.StartProcess("nuke", "Compile", moduleDirectory).AssertWaitForExit();
                        ProcessTasks.StartProcess("nuke", "WebPackBuild", moduleDirectory).AssertWaitForExit();
                    }
                }
            }
        });

    public Target Release => _ => _
        .DependsOn(Clean, Compress)
        .Requires(() => GitHubUser, () => GitHubToken)
        .Executes(async () =>
        {
            var tag = ReleaseVersion;
            var description = File.Exists(ReleaseNotes) ? File.ReadAllText(ReleaseNotes) : "";

            try
            {
                await PublishRelease(GitHubUser, GitRepositoryName, GitHubToken, tag, description, ZipFilePath,
                    PreRelease);
            }
            catch (AggregateException ex)
            {
                foreach (var innerException in ex.Flatten().InnerExceptions)
                {
                    if (innerException is ApiValidationException exception)
                    {
                        var responseString = exception?.HttpResponse?.Body.ToString() ?? string.Empty;
                        var responseDocument = JsonDocument.Parse(responseString);
                        var alreadyExistsError = false;

                        if (responseDocument.RootElement.TryGetProperty("errors", out var errors))
                        {
                            var errorCount = errors.GetArrayLength();

                            if (errorCount > 0)
                            {
                                alreadyExistsError = errors.EnumerateArray().Any(e =>
                                    e.GetProperty("code").GetString() == "already_exists");
                            }
                        }

                        if (alreadyExistsError)
                        {
                            ExitCode = (int)ExitCodes.GithubReleaseAlreadyExists;
                        }

                        Log.Error($"Api Validation Error: {responseString}");
                    }
                    else
                    {
                        Log.Error(innerException, "Error");
                    }
                }

                Assert.Fail("Publish Release Failed");
            }
        });

    public Target ClearTemp => _ => _
        .Executes(() => ClearTempBeforeExit = true);

    public Target ValidateManifest => _ => _
        .Executes(ValidateManifestsDependencies);

    public static int Main(string[] args)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "1");
        if (args.ElementAtOrDefault(0)?.ToLowerInvariant() == "help" || args.Length == 0)
        {
            if (args.Length >= 2)
            {
                var help = HelpProvider.HelpProvider.GetHelpForTarget(args[1]);
                Console.WriteLine(help);
            }
            else if (args.Length <= 1)
            {
                var targets = HelpProvider.HelpProvider.GetTargets();
                var stringBuilder = new StringBuilder("There is a help for targets:" + Environment.NewLine);
                targets.ForEach(target => stringBuilder = stringBuilder.Append("- ").AppendLine(target));
                Console.WriteLine(stringBuilder.ToString());
            }

            Environment.Exit(0);
        }

        var currentDirectory = Directory.GetCurrentDirectory();

        var nukeFiles = Directory.GetFiles(currentDirectory, ".nuke");

        if (!nukeFiles.Any() && !Directory.Exists(Path.Join(currentDirectory, ".nuke")))
        {
            Console.WriteLine("No .nuke file found!");
            var solutions = Directory.GetFiles(currentDirectory, "*.sln");

            if (solutions.Length == 1)
            {
                var solutionFileName = Path.GetFileName(solutions[0]);
                Console.WriteLine($"Solution found: {solutionFileName}");
                CreateDotNuke(currentDirectory, solutionFileName);
            }
            else if (solutions.Length < 1)
            {
                CreateDotNuke(currentDirectory);
            }
        }
        else if (nukeFiles.Any())
        {
            var nukeFile = nukeFiles[0];
            ConvertDotNukeFile(nukeFile);
        }

        if (ClearTempBeforeExit)
        {
            FileSystemTasks.DeleteDirectory(TemporaryDirectory);
        }

        var exitCode = Execute<Build>(x => x.Compile);
        return _exitCode ?? exitCode;
    }

    private static void ConvertDotNukeFile(string path)
    {
        var directory = Path.GetDirectoryName(path);
        var solutionPath = File.ReadLines(path).FirstOrDefault();
        FileSystemTasks.DeleteFile(path);
        CreateDotNuke(directory, solutionPath);
    }

    private static void CreateDotNuke(string path, string solutionPath = "")
    {
        var dotnukeDir = Path.Join(path, ".nuke");
        var paramsFilePath = Path.Join(dotnukeDir, "parameters.json");
        FileSystemTasks.EnsureExistingDirectory(dotnukeDir);
        var parameters = new NukeParameters { Solution = solutionPath };
        SerializationTasks.JsonSerializeToFile(parameters, paramsFilePath);
    }

    public static void CustomDotnetLogger(OutputType type, string text)
    {
        Log.Information(text);

        if (text.Contains("error: Response status code does not indicate success: 409"))
        {
            _exitCode = (int)ExitCodes.HttpRequestConflict;
        }
    }

    public void ChangeProjectVersion(string versionPrefix = null, string versionSuffix = null)
    {
        //theme
        if (IsTheme)
        {
            var jObject = SerializationTasks.JsonDeserializeFromFile<JObject>(PackageJsonPath);
            jObject["version"] = versionPrefix;
            SerializationTasks.JsonSerializeToFile(jObject, Path.GetFullPath(PackageJsonPath));
            return;
        }

        //module.manifest
        if (IsModule)
        {
            UpdateModuleManifest(versionPrefix, versionSuffix);
        }

        //Directory.Build.props
        UpdateDirectoryBuildProps(versionPrefix, versionSuffix);
    }

    private void UpdateDirectoryBuildProps(string versionPrefix, string versionSuffix)
    {
        var xmlDocument = LoadXml(DirectoryBuildPropsPath);

        if (!string.IsNullOrEmpty(versionPrefix))
        {
            var prefixNode = xmlDocument.GetElementsByTagName("VersionPrefix")[0];

            if (prefixNode != null)
            {
                prefixNode.InnerText = versionPrefix;
            }
        }

        if (string.IsNullOrEmpty(VersionSuffix) && !string.IsNullOrEmpty(versionSuffix))
        {
            var suffixNode = xmlDocument.GetElementsByTagName("VersionSuffix")[0];

            if (suffixNode != null)
            {
                suffixNode.InnerText = versionSuffix;
            }
        }

        SaveXml(DirectoryBuildPropsPath, xmlDocument);
    }

    private static void UpdateModuleManifest(string versionPrefix, string versionSuffix)
    {
        var xmlModuleManifestDoc = LoadXml(ModuleManifestFile);

        var moduleRootNode = xmlModuleManifestDoc.SelectSingleNode("module");

        // Update version
        if (!string.IsNullOrEmpty(versionPrefix))
        {
            moduleRootNode.SelectSingleNode("version").InnerText = versionPrefix;
        }

        // Update versionSuffix
        if (!string.IsNullOrEmpty(versionSuffix))
        {
            var versionTagNode = moduleRootNode.SelectSingleNode("version-tag");
            if (versionTagNode == null)
            {
                var versionNode = moduleRootNode.SelectSingleNode("version");
                versionTagNode = xmlModuleManifestDoc.CreateElement("version-tag");
                moduleRootNode.InsertAfter(versionTagNode, versionNode);
            }

            versionTagNode.InnerText = versionSuffix;
        }

        SaveXml(ModuleManifestFile, xmlModuleManifestDoc);
    }

    private static XmlDocument LoadXml(string filePath)
    {
        var xmlDocument = new XmlDocument { PreserveWhitespace = true };
        xmlDocument.Load(filePath);

        return xmlDocument;
    }

    private static void SaveXml(string filePath, XmlDocument xmlDocument)
    {
        var xmlWriterSettings = new XmlWriterSettings { Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false) };
        using var writer = XmlWriter.Create(filePath, xmlWriterSettings);
        xmlDocument.Save(writer);
    }

    private string GetThemeVersion(string packageJsonPath)
    {
        var json = JsonDocument.Parse(File.ReadAllText(packageJsonPath));

        if (json.RootElement.TryGetProperty("version", out var version))
        {
            return version.GetString();
        }

        Assert.Fail("No version found");
        return "";
    }

    public void IncrementVersionMinor()
    {
        var version = new Version(VersionPrefix);
        var newPrefix = $"{version.Major}.{version.Minor + 1}.{version.Build}";
        CustomVersionPrefix = newPrefix;
    }

    public void IncrementVersionPatch()
    {
        var version = new Version(VersionPrefix);
        var newPrefix = $"{version.Major}.{version.Minor}.{version.Build + 1}";
        CustomVersionPrefix = newPrefix;
    }

    private async Task<string> SendSwaggerSchemaToValidator(HttpClient httpClient, string schemaPath,
        string validatorUri)
    {
        var swaggerSchema = await File.ReadAllTextAsync(schemaPath);
        Log.Information($"Swagger schema length: {swaggerSchema.Length}");
        var requestContent = new StringContent(swaggerSchema, Encoding.UTF8, "application/json");
        Log.Information("Request content created");
        var request = new HttpRequestMessage(HttpMethod.Post, validatorUri);
        Log.Information("Request created");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = requestContent;
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        Log.Information($"Response from Validator: {result}");
        return result;
    }

    private static void GitLogger(OutputType type, string text)
    {
        if (text.Contains("github returned 422 Unprocessable Entity") && text.Contains("already_exists"))
        {
            _exitCode = (int)ExitCodes.GithubReleaseAlreadyExists;
        }

        if (text.Contains("nothing to commit, working tree clean"))
        {
            _exitCode = (int)ExitCodes.GitNothingToCommit;
        }

        switch (type)
        {
            case OutputType.Err:
                Log.Error(text);
                break;

            case OutputType.Std:
                Log.Information(text);
                break;
        }
    }

    private async Task PublishRelease(string owner, string repo, string token, string tag, string description,
        string artifactPath, bool prerelease)
    {
        var tokenAuth = new Credentials(token);

        var githubClient = new GitHubClient(new ProductHeaderValue("vc-build")) { Credentials = tokenAuth };

        var newRelease = new NewRelease(tag)
        {
            Name = tag,
            Prerelease = prerelease,
            Draft = false,
            Body = description,
            TargetCommitish = GitTasks.GitCurrentBranch()
        };

        var release = await githubClient.Repository.Release.Create(owner, repo, newRelease);

        using var artifactStream = File.OpenRead(artifactPath);
        var assetUpload = new ReleaseAssetUpload
        {
            FileName = Path.GetFileName(artifactPath),
            ContentType = "application/zip",
            RawData = artifactStream
        };

        await githubClient.Repository.Release.UploadAsset(release, assetUpload);
    }

    protected override void OnBuildCreated()
    {
        HttpTasks.DefaultTimeout = TimeSpan.FromSeconds(HttpTimeout);
        base.OnBuildCreated();
    }

    private void ValidateManifestsDependencies()
    {
        Log.Information("Dependencies of module.manifest:");
        ModuleManifest.Dependencies.ForEach(d => Log.Information($"{d.Id} - {d.Version}"));
        foreach (var project in Solution.AllProjects)
        {
            CompareWithManifest(project, ModuleManifest);
        }
    }

    private static void CompareWithManifest(Project project, ModuleManifest moduleManifest)
    {
        var projectPackageReferences = project.GetItems("PackageReference");
        projectPackageReferences.ForEach(p =>
        {
            var manifestDependency = FindManifestDependency(p, moduleManifest.Dependencies);
            if (manifestDependency != null)
            {
                CheckDependencyVersion(project, manifestDependency, p);
            }
            else
            {
                Log.Information($"No similar dependencies found for: {p}");
            }
        });
    }

    private static ManifestDependency FindManifestDependency(string packageId,
        IEnumerable<ManifestDependency> manifestDependencies)
    {
        return manifestDependencies.FirstOrDefault(d => packageId.StartsWith($"{d.Id}Module"));
    }

    private static void CheckDependencyVersion(Project project, ManifestDependency dependency, string packageId)
    {
        var packageVersion = project.GetPackageReferenceVersion(packageId);
        if (new Version(dependency.Version) != new Version(packageVersion))
        {
            Log.Error(
                $"Versions in module.manifest and {project.Name} of {dependency.Id} and {packageId} are mismatch!");
            Log.Error($"{dependency.Version} - {packageVersion}");
        }
        else
        {
            Log.Information($"{dependency.Id}:{dependency.Version} - {packageId}:{packageVersion}");
        }
    }

    private void CompressExecuteMethod()
    {
        if (IsModule)
        {
            var ignoredFiles = HttpTasks.HttpDownloadString(GlobalModuleIgnoreFileUrl).SplitLineBreaks();

            if (ModuleIgnoreFile.FileExists())
            {
                ignoredFiles = ignoredFiles.Concat(TextTasks.ReadAllLines(ModuleIgnoreFile)).ToArray();
            }

            ignoredFiles = ignoredFiles.Select(x => x.Trim()).Distinct().ToArray();

            var keepFiles = Array.Empty<string>();
            if (ModuleKeepFile.FileExists())
            {
                keepFiles = TextTasks.ReadAllLines(ModuleKeepFile).ToArray();
            }

            ArtifactPacker.CompressModule(options => options.WithSourceDirectory(ModuleOutputDirectory)
                                                            .WithOutputZipPath(ZipFilePath)
                                                            .WithModuleId(ModuleManifest.Id)
                                                            .WithModuleManifestPath(ModuleManifestFile)
                                                            .WithWebProjectDirectory(WebProject.Directory)
                                                            .WithIgnoreList(ignoredFiles)
                                                            .WithKeepList(keepFiles)
                                                            .WithModuleContentFolders(_moduleContentFolders));
        }
        else
        {
            ArtifactPacker.CompressPlatform(ArtifactsDirectory / "publish", ZipFilePath);
        }
    }

    private static void CleanSolution(string[] searchPattern, AbsolutePath[] ignorePaths = null)
    {
        if (SourceDirectory.DirectoryExists())
        {
            if (ignorePaths?.Any() == true)
            {
                SourceDirectory
                    .GlobDirectories(searchPattern)
                    .Where(directory => !ignorePaths.Any(p => p.Contains(directory)))
                    .ForEach(FileSystemTasks.DeleteDirectory);
            }
            else
            {
                SourceDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
            }

            if (TestsDirectory.DirectoryExists())
            {
                TestsDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
            }

            if (SamplesDirectory.DirectoryExists())
            {
                SamplesDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
            }
        }
        else
        {
            RootDirectory.GlobDirectories(searchPattern).ForEach(FileSystemTasks.DeleteDirectory);
        }

        FileSystemTasks.EnsureCleanDirectory(ArtifactsDirectory);
    }
}
