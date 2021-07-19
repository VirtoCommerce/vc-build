using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using VirtoCommerce.Platform.Core.Modularity;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Formatting = Newtonsoft.Json.Formatting;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Project = Nuke.Common.ProjectModel.Project;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
internal partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    //public Build()
    //{
    //    ToolPathResolver.ExecutingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    //}

    public static int Main()
    {
        var nukeFile = Directory.GetFiles(Directory.GetCurrentDirectory(), ".nuke");

        if (!nukeFile.Any())
        {
            Logger.Info("No .nuke file found!");
            var solutions = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");

            if (solutions.Length == 1)
            {
                var solutionFileName = Path.GetFileName(solutions.First());
                Logger.Info($"Solution found: {solutionFileName}");
                File.WriteAllText(".nuke", solutionFileName);
            }
            else if (solutions.Length < 1)
            {
                File.CreateText(Path.Combine(Directory.GetCurrentDirectory(), ".nuke")).Close();
            }
        }

        var exitCode = Execute<Build>(x => x.Compile);
        return ExitCode ?? exitCode;
    }

    private new static int? ExitCode;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public static Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;


    private static readonly string[] ModuleContentFolders = { "dist", "Localizations", "Scripts", "Content" };

    [Solution]
    public static Solution Solution { get; set; }

    private GitRepository GitRepository => GitRepository.FromLocalDirectory(RootDirectory / ".git");


    //private readonly Tool Git;

    //private readonly string MasterBranch = "master";
    //private readonly string DevelopBranch = "develop";
    //private readonly string ReleaseBranchPrefix = "release";
    //private readonly string HotfixBranchPrefix = "hotfix";
    private readonly string SonarLongLiveBranches = "master;develop";


    private static readonly HttpClient httpClient = new HttpClient();

    [Parameter("ApiKey for the specified source")]
    public static string ApiKey { get; set; }

    [Parameter]
    public static string Source { get; set; } = @"https://api.nuget.org/v3/index.json";

    [Parameter]
    public static string GlobalModuleIgnoreFileUrl { get; set; } = @"https://raw.githubusercontent.com/VirtoCommerce/vc-platform/dev/module.ignore";

    [Parameter]
    public static string SonarAuthToken { get; set; } = "";

    [Parameter]
    public static string SonarUrl { get; set; } = "https://sonarcloud.io";

    [Parameter]
    public static AbsolutePath CoverageReportPath { get; set; } = RootDirectory / ".tmp" / "coverage.xml";

    [Parameter]
    public static string TestsFilter { get; set; } = "Category!=IntegrationTest";

    [Parameter("Url to Swagger Validation Api")]
    public static string SwaggerValidatorUri { get; set; } = "https://validator.swagger.io/validator/debug";

    [Parameter("GitHub user for release creation")]
    public static string GitHubUser { get; set; }

    [Parameter("GitHub user security token for release creation")]
    public static string GitHubToken { get; set; }

    [Parameter("True - prerelease, False - release")]
    public static bool PreRelease { get; set; }

    [Parameter("True - Pull Request")]
    public static bool PullRequest { get; set; }

    [Parameter("Path to folder with  git clones of modules repositories")]
    public static AbsolutePath ModulesFolderPath { get; set; }

    [Parameter("Repo Organization/User")]
    public static string RepoOrg { get; set; } = "VirtoCommerce";

    [Parameter("Repo Name")]
    public static string RepoName { get; set; }

    [Parameter("Sonar Organization (\"virto-commerce\" by default)")]
    public static string SonarOrg { get; set; } = "virto-commerce";

    [Parameter("Path to nuget config")]
    public static AbsolutePath NugetConfig { get; set; }

    [Parameter("Swagger schema path")]
    public static AbsolutePath SwaggerSchemaPath { get; set; }

    [Parameter("Path to modules.json")]
    public static string ModulesJsonName { get; set; } = "modules_v3.json";

    [Parameter("Full uri to module artifact")]
    public static string CustomModulePackageUri { get; set; }

    [Parameter("Path to packageJson")]
    public static string PackageJsonPath { get; set; } = "package.json";

    [Parameter("Path to Release Notes File")]
    public static AbsolutePath ReleaseNotes { get; set; }

    [Parameter("VersionTag for module.manifest and Directory.Build.props")]
    public static string CustomVersionPrefix { get; set; }

    [Parameter("VersionSuffix for module.manifest and Directory.Build.props")]
    public static string CustomVersionSuffix { get; set; }

    [Parameter("Release branch")]
    public static string ReleaseBranch { get; set; }

    [Parameter("Branch Name for SonarQube")]
    public static string SonarBranchName { get; set; }

    [Parameter("Target Branch Name for SonarQube")]
    public static string SonarBranchNameTarget { get; set; } = "dev";

    [Parameter("PR Base for SonarQube")]
    public static string SonarPRBase { get; set; }

    [Parameter("PR Branch for SonarQube")]
    public static string SonarPRBranch { get; set; }

    [Parameter("PR Number for SonarQube")]
    public static string SonarPRNumber { get; set; }

    [Parameter("Github Repository for SonarQube")]
    public static string SonarGithubRepo { get; set; }

    [Parameter("PR Provider for SonarQube")]
    public static string SonarPRProvider { get; set; }

    [Parameter("Modules.json repo url")]
    public static string ModulesJsonRepoUrl { get; set; } = "https://github.com/VirtoCommerce/vc-modules.git";

    [Parameter("Force parameter for git checkout")]
    public static bool Force { get; set; }

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath TestsDirectory => RootDirectory / "tests";

    [Parameter("Path to Artifacts Directory")]
    public static AbsolutePath ArtifactsDirectory { get; set; } = RootDirectory / "artifacts";

    [Parameter("Directory containing modules.json")]
    public static string ModulesJsonDirectoryName { get; set; } = "vc-modules";

    private AbsolutePath ModulesLocalDirectory => ArtifactsDirectory / ModulesJsonDirectoryName;
    private Project WebProject => Solution?.AllProjects.FirstOrDefault(x => x.Name.EndsWith(".Web") && !x.Path.ToString().Contains("samples") || x.Name.EndsWith("VirtoCommerce.Storefront"));
    private AbsolutePath ModuleManifestFile => WebProject.Directory / "module.manifest";
    private AbsolutePath ModuleIgnoreFile => RootDirectory / "module.ignore";

    private Microsoft.Build.Evaluation.Project MSBuildProject => WebProject?.GetMSBuildProject();
    private string VersionPrefix => IsTheme ? GetThemeVersion(PackageJsonPath) : MSBuildProject.GetProperty("VersionPrefix")?.EvaluatedValue;
    private string VersionSuffix => MSBuildProject?.GetProperty("VersionSuffix")?.EvaluatedValue;
    private string ReleaseVersion => MSBuildProject?.GetProperty("PackageVersion")?.EvaluatedValue ?? WebProject.GetProperty("Version");

    private bool IsTheme => Solution == null;

    private ModuleManifest ModuleManifest => ManifestReader.Read(ModuleManifestFile);

    private AbsolutePath ModuleOutputDirectory => ArtifactsDirectory / ModuleManifest.Id;

    private AbsolutePath DirectoryBuildPropsPath => Solution.Directory / "Directory.Build.props";

    private string ZipFileName => IsModule ? $"{ModuleManifest.Id}_{ReleaseVersion}.zip" : $"{WebProject.Solution.Name}.{ReleaseVersion}.zip";
    private string ZipFilePath => ArtifactsDirectory / ZipFileName;
    private string GitRepositoryName => GitRepository.Identifier.Split('/')[1];

    private string ModulePackageUrl => CustomModulePackageUri.IsNullOrEmpty()
        ? $"https://github.com/VirtoCommerce/{GitRepositoryName}/releases/download/{ReleaseVersion}/{ModuleManifest.Id}_{ReleaseVersion}.zip"
        : CustomModulePackageUri;

    private GitRepository ModulesRepository => GitRepository.FromUrl(ModulesJsonRepoUrl);

    private bool IsModule => FileExists(ModuleManifestFile);

    private void SonarLogger(OutputType type, string text)
    {
        switch (type)
        {
            case OutputType.Err:
                Logger.Error(text);
                break;
            case OutputType.Std:
                Logger.Info(text);
                break;
        }
    }

    private Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

            if (DirectoryExists(TestsDirectory))
            {
                TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            }

            //if (DirectoryExists(TestsDirectory))
            //{
            //    WebProject.Directory.GlobDirectories("**/node_modules").ForEach(DeleteDirectory);
            //}
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    private Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .When(NugetConfig != null, c => c
                    .SetConfigFile(NugetConfig))
            );
        });

    private Target Pack => _ => _
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
                .SetOutputDirectory(ArtifactsDirectory)
                .SetVersion(ReleaseVersion);

            if (IsModule)
            {
                //For module take nuget package description from module manifest
                settings.SetAuthors(ModuleManifest.Authors)
                    .SetPackageLicenseUrl(ModuleManifest.LicenseUrl)
                    .SetPackageProjectUrl(ModuleManifest.ProjectUrl)
                    .SetPackageIconUrl(ModuleManifest.IconUrl)
                    .SetPackageRequireLicenseAcceptance(false)
                    .SetDescription(ModuleManifest.Description)
                    .SetCopyright(ModuleManifest.Copyright);
            }

            DotNetPack(settings);
        });

    private Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var dotnetPath = ToolPathResolver.GetPathExecutable("dotnet");
            var testProjects = Solution.GetProjects("*.Test|*.Tests|*.Testing");
            var OutPath = RootDirectory / ".tmp";

            testProjects.ForEach((testProject, index) =>
            {
                DotNet($"add {testProject.Path} package coverlet.collector");

                var testSetting = new DotNetTestSettings()
                    .SetProjectFile(testProject.Path)
                    .SetConfiguration(Configuration)
                    .SetFilter(TestsFilter)
                    .SetNoBuild(true)
                    .SetProcessLogOutput(true)
                    .SetResultsDirectory(OutPath)
                    .SetDataCollector("XPlat Code Coverage");

                DotNetTest(testSetting);
            });

            var coberturaReports = OutPath.GlobFiles("**/coverage.cobertura.xml");

            if (coberturaReports.Count > 0)
            {
                var reportGenerator = ToolResolver.GetPackageTool("dotnet-reportgenerator-globaltool", "ReportGenerator.dll", "4.8.8", "netcoreapp3.0");
                reportGenerator.Invoke($"-reports:{OutPath / "**/coverage.cobertura.xml"} -targetdir:{OutPath} -reporttypes:SonarQube");
                var sonarCoverageReportPath = OutPath.GlobFiles("SonarQube.xml").FirstOrDefault();

                if (sonarCoverageReportPath == null)
                {
                    ControlFlow.Fail("No Coverage Report found");
                }

                MoveFile(sonarCoverageReportPath, CoverageReportPath, FileExistsPolicy.Overwrite);
            }
            else
            {
                Logger.Warn("No Coverage Reports found");
            }
        });

    public void CustomDotnetLogger(OutputType type, string text)
    {
        Logger.Info(text);

        if (text.Contains("error: Response status code does not indicate success: 409"))
        {
            ExitCode = 409;
        }
    }

    private Target PublishPackages => _ => _
        .DependsOn(Pack)
        .Requires(() => ApiKey)
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg", "*.snupkg").OrderBy(p => p.ToString());

            DotNetLogger = CustomDotnetLogger;

            DotNetNuGetPush(s => s
                    .SetSource(Source)
                    .SetApiKey(ApiKey)
                    .SetSkipDuplicate(true)
                    .CombineWith(
                        packages, (cs, v) => cs
                            .SetTargetPath(v)),
                degreeOfParallelism: 1,
                completeOnFailure: true);
        });

    public class Utf8StringWriter : StringWriter
    {
        // Use UTF8 encoding but write no BOM to the wire
        public override Encoding Encoding => new UTF8Encoding(false);
    }

    public void ChangeProjectVersion(string prefix = null, string suffix = null)
    {
        //theme
        if (IsTheme)
        {
            //var json = JsonDocument.Parse(File.ReadAllText(PackageJsonPath));
            //json.RootElement.GetProperty("version")
            var json = SerializationTasks.JsonDeserializeFromFile<JObject>(PackageJsonPath);
            json["version"] = prefix;
            SerializationTasks.JsonSerializeToFile(json, Path.GetFullPath(PackageJsonPath));
        }
        else
        {
            //module.manifest
            if (IsModule)
            {
                var manifest = ModuleManifest.Clone();

                if (!string.IsNullOrEmpty(prefix))
                {
                    manifest.Version = prefix;
                }

                if (!string.IsNullOrEmpty(suffix))
                {
                    manifest.VersionTag = suffix;
                }

                using (var writer = new Utf8StringWriter())
                {
                    var xml = new XmlSerializer(typeof(ModuleManifest));
                    xml.Serialize(writer, manifest);
                    File.WriteAllText(ModuleManifestFile, writer.ToString(), Encoding.UTF8);
                }
            }

            //Directory.Build.props
            var xmlDoc = new XmlDocument
            {
                PreserveWhitespace = true,
            };

            xmlDoc.LoadXml(File.ReadAllText(DirectoryBuildPropsPath));

            if (!string.IsNullOrEmpty(prefix))
            {
                var prefixNodex = xmlDoc.GetElementsByTagName("VersionPrefix");
                prefixNodex[0].InnerText = prefix;
            }

            if (string.IsNullOrEmpty(VersionSuffix) && !string.IsNullOrEmpty(suffix))
            {
                var suffixNodes = xmlDoc.GetElementsByTagName("VersionSuffix");
                suffixNodes[0].InnerText = suffix;
            }

            using (var writer = new Utf8StringWriter())
            {
                xmlDoc.Save(writer);
                File.WriteAllText(DirectoryBuildPropsPath, writer.ToString());
            }
        }
    }

    private Target ChangeVersion => _ => _
        .Executes(() =>
        {
            if (string.IsNullOrEmpty(VersionSuffix) && !CustomVersionSuffix.IsNullOrEmpty() || !CustomVersionPrefix.IsNullOrEmpty())
            {
                ChangeProjectVersion(CustomVersionPrefix, CustomVersionSuffix);
            }
        });

    private string GetThemeVersion(string packageJsonPath)
    {
        var json = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
        JsonElement version;

        if (json.RootElement.TryGetProperty("version", out version))
        {
            return version.GetString();
        }

        ControlFlow.Fail("No version found");
        return "";
    }

    private Target StartRelease => _ => _
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;
            var disableApprove = Environment.GetEnvironmentVariable("VCBUILD_DISABLE_RELEASE_APPROVAL");

            if (disableApprove.IsNullOrEmpty() && !Force)
            {
                Console.Write($"Are you sure you want to release {GitRepository.Identifier}? (Y/N): ");
                var response = Console.ReadLine();

                if (response.ToLower().CompareTo("y") != 0)
                {
                    ControlFlow.Fail("Aborted");
                }
            }

            var checkoutCommand = new StringBuilder("checkout dev");

            if (Force)
            {
                checkoutCommand.Append(" --force");
            }

            GitTasks.Git(checkoutCommand.ToString());
            GitTasks.Git("pull");
            string version;

            if (IsTheme)
            {
                version = GetThemeVersion(PackageJsonPath);
            }
            else
            {
                version = ReleaseVersion;
            }

            var releaseBranchName = $"release/{version}";
            Logger.Info(Directory.GetCurrentDirectory());
            GitTasks.Git($"checkout -B {releaseBranchName}");
            GitTasks.Git($"push -u origin {releaseBranchName}");
        });

    private Target CompleteRelease => _ => _
        .After(StartRelease)
        .Executes(() =>
        {
            var currentBranch = GitTasks.GitCurrentBranch();
            //Master
            GitTasks.Git("checkout master");
            GitTasks.Git("pull");
            GitTasks.Git($"merge {currentBranch}");
            GitTasks.Git("push origin master");
            //Dev
            GitTasks.Git("checkout dev");
            GitTasks.Git($"merge {currentBranch}");
            IncrementVersionMinor();
            ChangeProjectVersion(CustomVersionPrefix);
            var addFiles = "";

            if (IsTheme)
            {
                addFiles = $"{PackageJsonPath}";
            }
            else
            {
                var manifestArg = IsModule ? RootDirectory.GetRelativePathTo(ModuleManifestFile) : "";
                addFiles = $"Directory.Build.props {manifestArg}";
            }

            GitTasks.Git($"add {addFiles}");
            GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
            GitTasks.Git("push origin dev");
            //remove release branch
            GitTasks.Git($"branch -d {currentBranch}");
            GitTasks.Git($"push origin --delete {currentBranch}");
        });

    private Target QuickRelease => _ => _
        .DependsOn(StartRelease, CompleteRelease);

    private Target StartHotfix => _ => _
        .Executes(() =>
        {
            GitTasks.Git("checkout master");
            GitTasks.Git("pull");
            IncrementVersionPatch();
            var hotfixBranchName = $"hotfix/{CustomVersionPrefix}";
            Logger.Info(Directory.GetCurrentDirectory());
            GitTasks.Git($"checkout -b {hotfixBranchName}");
            ChangeProjectVersion(CustomVersionPrefix);
            var manifestArg = IsModule ? RootDirectory.GetRelativePathTo(ModuleManifestFile) : "";
            GitTasks.Git($"add Directory.Build.props {manifestArg}");
            GitTasks.Git($"commit -m \"{CustomVersionPrefix}\"");
            GitTasks.Git($"push -u origin {hotfixBranchName}");
        });

    private Target CompleteHotfix => _ => _
        .After(StartHotfix)
        .Executes(() =>
        {
            //workaround for run from sources
            var currentBranch = GitTasks.GitCurrentBranch();
            //Master
            GitTasks.Git("checkout master");
            GitTasks.Git($"merge {currentBranch}");
            GitTasks.Git($"tag {VersionPrefix}");
            GitTasks.Git("push origin master");
            //remove hotfix branch
            GitTasks.Git($"branch -d {currentBranch}");
            GitTasks.Git($"push origin --delete {currentBranch}");
        });

    public void IncrementVersionMinor()
    {
        var v = new Version(VersionPrefix);
        var newPrefix = $"{v.Major}.{v.Minor + 1}.{v.Build}";
        CustomVersionPrefix = newPrefix;
    }

    public void IncrementVersionPatch()
    {
        var v = new Version(VersionPrefix);
        var newPrefix = $"{v.Major}.{v.Minor}.{v.Build + 1}";
        CustomVersionPrefix = newPrefix;
    }

    private Target IncrementMinor => _ => _
        .Triggers(ChangeVersion)
        .Executes(() =>
        {
            IncrementVersionMinor();
        });

    private Target IncrementPatch => _ => _
        .Triggers(ChangeVersion)
        .Executes(() =>
        {
            IncrementVersionPatch();
        });

    private Target Publish => _ => _
        .DependsOn(Compile)
        .After(WebPackBuild, Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProcessWorkingDirectory(WebProject.Directory)
                .EnableNoRestore()
                .SetOutput(IsModule ? ModuleOutputDirectory / "bin" : ArtifactsDirectory / "publish")
                .SetConfiguration(Configuration));
        });

    private Target WebPackBuild => _ => _
        .Executes(() =>
        {
            if (FileExists(WebProject.Directory / "package.json"))
            {
                NpmTasks.Npm("ci", WebProject.Directory);
                NpmTasks.NpmRun(s => s.SetProcessWorkingDirectory(WebProject.Directory).SetCommand("webpack:build"));
            }
            else
            {
                Logger.Info("Nothing to build.");
            }
        });

    private Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    private Target Compress => _ => _
        .DependsOn(Clean, WebPackBuild, Test, Publish)
        .Executes(() =>
        {
            if (IsModule)
            {
                //Copy module.manifest and all content directories into a module output folder
                CopyFileToDirectory(ModuleManifestFile, ModuleOutputDirectory, FileExistsPolicy.Overwrite);

                foreach (var moduleFolder in ModuleContentFolders)
                {
                    var srcModuleFolder = WebProject.Directory / moduleFolder;

                    if (DirectoryExists(srcModuleFolder))
                    {
                        CopyDirectoryRecursively(srcModuleFolder, ModuleOutputDirectory / moduleFolder, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
                    }
                }

                var ignoredFiles = HttpTasks.HttpDownloadString(GlobalModuleIgnoreFileUrl).SplitLineBreaks();

                if (FileExists(ModuleIgnoreFile))
                {
                    ignoredFiles = ignoredFiles.Concat(TextTasks.ReadAllLines(ModuleIgnoreFile)).ToArray();
                }

                ignoredFiles = ignoredFiles.Select(x => x.Trim()).Distinct().ToArray();

                DeleteFile(ZipFilePath);
                //TODO: Exclude all ignored files and *module files not related to compressed module
                var ignoreModulesFilesRegex = new Regex(@".+Module\..*", RegexOptions.IgnoreCase);
                var includeModuleFilesRegex = new Regex(@$".*{ModuleManifest.Id}(Module)?\..*", RegexOptions.IgnoreCase);

                CompressionTasks.CompressZip(ModuleOutputDirectory, ZipFilePath, x => !ignoredFiles.Contains(x.Name, StringComparer.OrdinalIgnoreCase) && !ignoreModulesFilesRegex.IsMatch(x.Name)
                                                                                      || includeModuleFilesRegex.IsMatch(x.Name));
            }
            else
            {
                DeleteFile(ZipFilePath);
                CompressionTasks.CompressZip(ArtifactsDirectory / "publish", ZipFilePath);
            }
        });

    private Target GetManifestGit => _ => _
        .Before(UpdateManifest)
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;
            var modulesJsonFile = ModulesLocalDirectory / ModulesJsonName;

            if (!DirectoryExists(ModulesLocalDirectory))
            {
                GitTasks.Git($"clone {ModulesRepository.HttpsUrl} {ModulesLocalDirectory}");
            }
            else
            {
                GitTasks.Git("pull", ModulesLocalDirectory);
            }
        });

    private Target UpdateManifest => _ => _
        .Before(PublishManifestGit)
        .After(GetManifestGit)
        .Executes(() =>
        {
            var modulesJsonFile = ModulesLocalDirectory / ModulesJsonName;
            var manifest = ModuleManifest;

            var modulesExternalManifests = JsonConvert.DeserializeObject<List<ExternalModuleManifest>>(TextTasks.ReadAllText(modulesJsonFile));
            manifest.PackageUrl = ModulePackageUrl;
            var existExternalManifest = modulesExternalManifests.FirstOrDefault(x => x.Id == manifest.Id);

            if (existExternalManifest != null)
            {
                if (!manifest.VersionTag.IsNullOrEmpty() || !CustomVersionSuffix.IsNullOrEmpty())
                {
                    var tag = manifest.VersionTag.IsNullOrEmpty() ? CustomVersionSuffix : manifest.VersionTag;
                    manifest.VersionTag = tag;
                    var existPrereleaseVersions = existExternalManifest.Versions.Where(v => !v.VersionTag.IsNullOrEmpty());

                    if (existPrereleaseVersions.Any())
                    {
                        var prereleaseVersion = existPrereleaseVersions.First();
                        prereleaseVersion.Dependencies = manifest.Dependencies;
                        prereleaseVersion.Incompatibilities = manifest.Incompatibilities;
                        prereleaseVersion.PlatformVersion = manifest.PlatformVersion;
                        prereleaseVersion.ReleaseNotes = manifest.ReleaseNotes;
                        prereleaseVersion.Version = manifest.Version;
                        prereleaseVersion.VersionTag = manifest.VersionTag;
                        prereleaseVersion.PackageUrl = manifest.PackageUrl;
                    }
                    else
                    {
                        existExternalManifest.Versions.Add(ExternalModuleManifestVersion.FromManifest(manifest));
                    }
                }
                else
                {
                    existExternalManifest.PublishNewVersion(manifest);
                }

                existExternalManifest.Title = manifest.Title;
                existExternalManifest.Description = manifest.Description;
                existExternalManifest.Authors = manifest.Authors;
                existExternalManifest.Copyright = manifest.Copyright;
                existExternalManifest.Groups = manifest.Groups;
                existExternalManifest.IconUrl = manifest.IconUrl;
                existExternalManifest.Id = manifest.Id;
                existExternalManifest.LicenseUrl = manifest.LicenseUrl;
                existExternalManifest.Owners = manifest.Owners;
                existExternalManifest.ProjectUrl = manifest.ProjectUrl;
                existExternalManifest.RequireLicenseAcceptance = manifest.RequireLicenseAcceptance;
                existExternalManifest.Tags = manifest.Tags;
            }
            else
            {
                modulesExternalManifests.Add(ExternalModuleManifest.FromManifest(manifest));
            }

            TextTasks.WriteAllText(modulesJsonFile, JsonConvert.SerializeObject(modulesExternalManifests, Formatting.Indented));
        });

    private Target PublishManifestGit => _ => _
        .After(UpdateManifest)
        .Executes(() =>
        {
            GitTasks.GitLogger = GitLogger;
            GitTasks.Git($"commit -am \"{ModuleManifest.Id} {ReleaseVersion}\"", ModulesLocalDirectory);
            GitTasks.Git("push origin HEAD:master -f", ModulesLocalDirectory);
        });

    private Target PublishModuleManifest => _ => _
        .DependsOn(GetManifestGit, UpdateManifest, PublishManifestGit);

    private Target SwaggerValidation => _ => _
        .DependsOn(Publish)
        .Requires(() => !IsModule)
        .Executes(async () =>
        {
            var swashbuckle = ToolResolver.GetPackageTool("Swashbuckle.AspNetCore.Cli", "dotnet-swagger.dll", framework: "netcoreapp3.0");
            var projectPublishPath = ArtifactsDirectory / "publish" / $"{WebProject.Name}.dll";
            var swaggerJson = ArtifactsDirectory / "swagger.json";
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(RootDirectory / "src" / "VirtoCommerce.Platform.Web");
            swashbuckle.Invoke($"tofile --output {swaggerJson} {projectPublishPath} VirtoCommerce.Platform");
            Directory.SetCurrentDirectory(currentDir);

            var responseContent = await SendSwaggerSchemaToValidator(httpClient, swaggerJson, SwaggerValidatorUri);
            var jsonObj = JObject.Parse(responseContent);

            foreach (var msg in jsonObj["schemaValidationMessages"])
            {
                Logger.Normal(msg);
            }

            if (jsonObj["schemaValidationMessages"].Where(t => (string)t["level"] == "error").Any())
            {
                ControlFlow.Fail("Schema Validation Messages contains error");
            }
        });

    private Target ValidateSwaggerSchema => _ => _
        .Requires(() => SwaggerSchemaPath != null)
        .Executes(async () =>
        {
            var responseContent = await SendSwaggerSchemaToValidator(httpClient, SwaggerSchemaPath, SwaggerValidatorUri);
            var jsonObj = JObject.Parse(responseContent);
            JToken validationMessages;

            if (jsonObj.TryGetValue("schemaValidationMessages", out validationMessages))
            {
                foreach (var msg in validationMessages)
                {
                    Logger.Normal(msg);
                }

                if (validationMessages.Where(t => (string)t["level"] == "error").Any())
                {
                    ControlFlow.Fail("Schema Validation Messages contains error");
                }
            }
            else
            {
                Logger.Warn("There are no validation messages from validator");
            }
        });

    private async Task<string> SendSwaggerSchemaToValidator(HttpClient httpClient, string schemaPath, string validatorUri)
    {
        var swaggerSchema = File.ReadAllText(schemaPath);
        Logger.Normal($"Swagger schema length: {swaggerSchema.Length}");
        var requestContent = new StringContent(swaggerSchema, Encoding.UTF8, "application/json");
        Logger.Normal("Request content created");
        var request = new HttpRequestMessage(HttpMethod.Post, validatorUri);
        Logger.Normal("Request created");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = requestContent;
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content?.ReadAsStringAsync();
        Logger.Normal($"Response from Validator: {result}");
        return result;
    }

    private Target SonarQubeStart => _ => _
        .Executes(() =>
        {
            var dotNetPath = ToolPathResolver.TryGetEnvironmentExecutable("DOTNET_EXE") ?? ToolPathResolver.GetPathExecutable("dotnet");
            Logger.Normal($"IsServerBuild = {IsServerBuild}");
            var branchName = string.IsNullOrEmpty(SonarBranchName) ? GitRepository.Branch : SonarBranchName;
            var branchNameTarget = string.IsNullOrEmpty(SonarBranchNameTarget) ? GitRepository.Branch : SonarBranchNameTarget;
            Logger.Info($"BRANCH_NAME = {branchName}");
            var projectName = Solution.Name;
            var prBaseParam = "";
            var prBranchParam = "";
            var prKeyParam = "";
            var ghRepoArg = "";
            var prProviderArg = "";
            var prBase = "";
            var branchParam = "";
            var branchTargetParam = "";

            if (PullRequest)
            {
                prBase = string.IsNullOrEmpty(SonarPRBase) ? Environment.GetEnvironmentVariable("CHANGE_TARGET") : SonarPRBase;
                prBaseParam = $"/d:sonar.pullrequest.base=\"{prBase}\"";

                var changeTitle = string.IsNullOrEmpty(SonarPRBranch) ? Environment.GetEnvironmentVariable("CHANGE_TITLE") : SonarPRBranch;
                prBranchParam = $"/d:sonar.pullrequest.branch=\"{changeTitle}\"";

                var prNumber = string.IsNullOrEmpty(SonarPRNumber) ? Environment.GetEnvironmentVariable("CHANGE_ID") : SonarPRNumber;
                prKeyParam = $"/d:sonar.pullrequest.key={prNumber}";

                ghRepoArg = string.IsNullOrEmpty(SonarGithubRepo) ? "" : $"/d:sonar.pullrequest.github.repository={SonarGithubRepo}";

                prProviderArg = string.IsNullOrEmpty(SonarPRProvider) ? "" : $"/d:sonar.pullrequest.provider={SonarPRProvider}";
            }
            else
            {
                branchParam = $"/d:\"sonar.branch.name={branchName}\"";
                branchTargetParam = SonarLongLiveBranches.Contains(branchName) ? "" : $"/d:\"sonar.branch.target={branchNameTarget}\"";
            }

            var projectNameParam = $"/n:\"{RepoName}\"";
            var projectKeyParam = $"/k:\"{RepoOrg}_{RepoName}\"";
            var projectVersionParam = $"/v:\"{ReleaseVersion}\"";
            var hostParam = $"/d:sonar.host.url={SonarUrl}";
            var tokenParam = $"/d:sonar.login={SonarAuthToken}";
            var sonarReportPathParam = $"/d:sonar.coverageReportPaths={CoverageReportPath}";
            var orgParam = $"/o:{SonarOrg}";

            var startCmd = $"sonarscanner begin {orgParam} {branchParam} {branchTargetParam} {projectKeyParam} {projectNameParam} {projectVersionParam} {hostParam} {tokenParam} {sonarReportPathParam} {prBaseParam} {prBranchParam} {prKeyParam} {ghRepoArg} {prProviderArg}";

            Logger.Normal($"Execute: {startCmd.Replace(SonarAuthToken, "{IS HIDDEN}")}");

            var processStart = ProcessTasks.StartProcess(dotNetPath, startCmd, customLogger: SonarLogger, logInvocation: false)
                .AssertWaitForExit().AssertZeroExitCode();

            processStart.Output.EnsureOnlyStd();
        });

    private Target SonarQubeEnd => _ => _
        .After(SonarQubeStart)
        .DependsOn(Compile)
        .Executes(() =>
        {
            var dotNetPath = ToolPathResolver.TryGetEnvironmentExecutable("DOTNET_EXE") ?? ToolPathResolver.GetPathExecutable("dotnet");
            var tokenParam = $"/d:sonar.login={SonarAuthToken}";
            var endCmd = $"sonarscanner end {tokenParam}";

            Logger.Normal($"Execute: {endCmd.Replace(SonarAuthToken, "{IS HIDDEN}")}");

            var processEnd = ProcessTasks.StartProcess(dotNetPath, endCmd, customLogger: SonarLogger, logInvocation: false)
                .AssertWaitForExit().AssertZeroExitCode();

            var errors = processEnd.Output.Where(o => !o.Text.Contains(@"The 'files' list in config file 'tsconfig.json' is empty") && o.Type == OutputType.Err).ToList();

            if (errors.Any())
            {
                ControlFlow.Fail(errors.Select(e => e.Text).Join(Environment.NewLine));
            }
        });

    private Target StartAnalyzer => _ => _
        .DependsOn(SonarQubeStart, SonarQubeEnd)
        .Executes(() =>
        {
            Logger.Normal("Sonar validation done.");
        });


    private Target MassPullAndBuild => _ => _
        .Requires(() => ModulesFolderPath)
        .Executes(() =>
        {
            if (DirectoryExists(ModulesFolderPath))
            {
                foreach (var moduleDirectory in Directory.GetDirectories(ModulesFolderPath))
                {
                    var isGitRepository = FindParentDirectory(moduleDirectory, x => x.GetDirectories(".git").Any()) != null;

                    if (isGitRepository)
                    {
                        GitTasks.Git("pull", moduleDirectory);
                        ProcessTasks.StartProcess("nuke", "Compile", moduleDirectory).AssertWaitForExit();
                        ProcessTasks.StartProcess("nuke", "WebPackBuild", moduleDirectory).AssertWaitForExit();
                    }
                }
            }
        });

    private void GitLogger(OutputType type, string text)
    {
        if (text.Contains("github returned 422 Unprocessable Entity") && text.Contains("already_exists"))
        {
            ExitCode = 422;
        }

        if (text.Contains("nothing to commit, working tree clean"))
        {
            ExitCode = 423;
        }

        switch (type)
        {
            case OutputType.Err:
                Logger.Error(text);
                break;
            case OutputType.Std:
                Logger.Info(text);
                break;
        }
    }

    private async Task PublishRelease(string owner, string repo, string token, string tag, string description, string artifactPath, bool prerelease)
    {
        var tokenAuth = new Credentials(token);

        var githubClient = new GitHubClient(new ProductHeaderValue("vc-build"))
        {
            Credentials = tokenAuth,
        };

        var newRelease = new NewRelease(tag)
        {
            Name = tag,
            Prerelease = prerelease,
            Draft = false,
            Body = description,
            TargetCommitish = GitTasks.GitCurrentBranch(),
        };

        var release = await githubClient.Repository.Release.Create(owner, repo, newRelease);

        using (var artifactStream = File.OpenRead(artifactPath))
        {
            var assetUpload = new ReleaseAssetUpload
            {
                FileName = Path.GetFileName(artifactPath),
                ContentType = "application/zip",
                RawData = artifactStream,
            };

            var asset = await githubClient.Repository.Release.UploadAsset(release, assetUpload);
        }
    }

    private Target Release => _ => _
        .DependsOn(Clean, Compress)
        .Requires(() => GitHubUser, () => GitHubToken)
        .Executes(() =>
        {
            var tag = ReleaseVersion;
            var targetBranchArg = ReleaseBranch.IsNullOrEmpty() ? "" : $"--target \"{ReleaseBranch}\"";
            var descr = File.Exists(ReleaseNotes) ? File.ReadAllText(ReleaseNotes) : "";

            try
            {
                PublishRelease(GitHubUser, GitRepositoryName, GitHubToken, tag, descr, ZipFilePath, PreRelease).Wait();
            }
            catch (AggregateException ex)
            {
                var responseRaw = ((ApiValidationException)ex.InnerException)?.HttpResponse?.Body.ToString() ?? "";
                var response = JsonDocument.Parse(responseRaw);
                var alreadyExistsError = false;
                JsonElement errors;

                if (response.RootElement.TryGetProperty("errors", out errors))
                {
                    var errorCount = errors.GetArrayLength();

                    if (errorCount > 0)
                    {
                        alreadyExistsError = errors.EnumerateArray().Where(e => e.GetProperty("code").GetString() == "already_exists").Count() > 0;
                    }
                }

                if (alreadyExistsError)
                {
                    ExitCode = 422;
                }

                ControlFlow.Fail(ex.Message);
            }
        });

    //private void FinishReleaseOrHotfix(string tag)
    //{
    //    Git($"checkout {MasterBranch}");
    //    Git($"merge --no-ff --no-edit {GitRepository.Branch}");
    //    Git($"tag {tag}");

    //    Git($"checkout {DevelopBranch}");
    //    Git($"merge --no-ff --no-edit {GitRepository.Branch}");

    //    //Uncomment to switch on armed mode 
    //    //Git($"branch -D {GitRepository.Branch}");
    //    //Git($"push origin {MasterBranch} {DevelopBranch} {tag}");
    //}

    private Target ClearTemp => _ => _
        .Executes(() =>
        {
            DeleteDirectory(TemporaryDirectory);
        });
}
