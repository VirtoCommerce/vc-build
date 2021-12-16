using System;
using System.Collections.Generic;
using System.Globalization;
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
using VirtoCommerce.Build.Utils;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Formatting = Newtonsoft.Json.Formatting;
using ProductHeaderValue = Octokit.ProductHeaderValue;
using Project = Nuke.Common.ProjectModel.Project;

namespace VirtoCommerce.Build
{
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    internal partial class Build : NukeBuild
    {
        /// Support plugins are available for:
        ///   - JetBrains ReSharper        https://nuke.build/resharper
        ///   - JetBrains Rider            https://nuke.build/rider
        ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
        ///   - Microsoft VSCode           https://nuke.build/vscode

        private static readonly string[] _moduleContentFolders = { "dist", "Localizations", "Scripts", "Content" };
        private static readonly string[] _sonarLongLiveBranches = { "master", "develop", "dev", "main" };
        private static readonly HttpClient _httpClient = new HttpClient();
        private static int? _exitCode;

        private static bool ClearTempBeforeExit { get; set; } = false;

        public static int Main(string[] args)
        {
            if(!MSBuildLocator.IsRegistered) MSBuildLocator.RegisterDefaults();

            Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "1");
            if (args[0]?.ToLowerInvariant() == "help")
            {
                if (args.Length == 2)
                {
                    var help = HelpProvider.HelpProvider.GetHelpForTarget(args[1]);
                    Logger.Info(help);
                }
                else if (args.Length == 1)
                {
                    var targets = HelpProvider.HelpProvider.GetTargets();
                    var stringBuilder = new StringBuilder("There is a help for targets:" + Environment.NewLine);
                    targets.ForEach(target => stringBuilder = stringBuilder.AppendLine($"- {target}"));
                    Logger.Info(stringBuilder.ToString());
                }
                Environment.Exit(0);
            }

            var nukeFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), ".nuke");

            if (!nukeFiles.Any() && !Directory.Exists(RootDirectory / ".nuke"))
            {
                Logger.Info("No .nuke file found!");
                var solutions = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");

                if (solutions.Length == 1)
                {
                    var solutionFileName = Path.GetFileName(solutions.First());
                    Logger.Info($"Solution found: {solutionFileName}");
                    CreateDotNuke(RootDirectory, solutionFileName);
                }
                else if (solutions.Length < 1)
                {
                    CreateDotNuke(Directory.GetCurrentDirectory());
                }
            }
            else if(nukeFiles.Any())
            {
                var nukeFile = nukeFiles.First();
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
            EnsureExistingDirectory(dotnukeDir);
            var parameters = new NukeParameters { Solution = solutionPath };
            SerializationTasks.JsonSerializeToFile(parameters, paramsFilePath);
        }

        [Solution]
        public static Solution Solution { get; set; }

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public static Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("API key for the specified source")]
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

        [Parameter("URL of Swagger Validation API")]
        public static string SwaggerValidatorUri { get; set; } = "https://validator.swagger.io/validator/debug";

        [Parameter("GitHub user for release creation")]
        public static string GitHubUser { get; set; }

        [Parameter("GitHub user security token for release creation")]
        public static string GitHubToken { get; set; }

        [Parameter("True - prerelease, False - release")]
        public static bool PreRelease { get; set; }

        [Parameter("True - Pull Request")]
        public static bool PullRequest { get; set; }

        [Parameter("Path to folder with git clones of modules repositories")]
        public static AbsolutePath ModulesFolderPath { get; set; }

        [Parameter("Repo Organization/User")]
        public static string RepoOrg { get; set; } = "VirtoCommerce";

        [Parameter("Repo Name")]
        public static string RepoName { get; set; }

        [Parameter("Sonar Organization (\"virto-commerce\" by default)")]
        public static string SonarOrg { get; set; } = "virto-commerce";

        [Parameter("Path to NuGet config")]
        public static AbsolutePath NugetConfig { get; set; }

        [Parameter("Swagger schema path")]
        public static AbsolutePath SwaggerSchemaPath { get; set; }

        [Parameter("Path to modules.json")]
        public static string ModulesJsonName { get; set; } = "modules_v3.json";

        [Parameter("Full URI of module artifact")]
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

        [Parameter("Defauld (start) project name")]
        public static string DefaultProject { get; set; } = ".Web";

        // TODO: Convert to a method because GitRepository.FromLocalDirectory() is a heavy method and it should not be used as a property
        protected GitRepository GitRepository => GitRepository.FromLocalDirectory(RootDirectory / ".git");

        protected static AbsolutePath SourceDirectory => RootDirectory / "src";
        protected static AbsolutePath TestsDirectory => RootDirectory / "tests";
        protected static AbsolutePath SamplesDirectory => RootDirectory / "samples";

        protected AbsolutePath ModulesLocalDirectory => ArtifactsDirectory / ModulesJsonDirectoryName;
        protected static Project WebProject => Solution?.AllProjects.FirstOrDefault(x => x.Name.EndsWith(DefaultProject) || x.Name.EndsWith("VirtoCommerce.Storefront") || x.Name.EndsWith("VirtoCommerce.Build"));
        protected static AbsolutePath ModuleManifestFile => WebProject?.Directory / "module.manifest";
        protected AbsolutePath ModuleIgnoreFile => RootDirectory / "module.ignore";
        protected static AbsolutePath WebDirectory => WebProject?.Directory;

        protected static Microsoft.Build.Evaluation.Project MSBuildProject => WebProject?.GetMSBuildProject();
        protected string VersionPrefix => IsTheme ? GetThemeVersion(PackageJsonPath) : MSBuildProject.GetProperty("VersionPrefix")?.EvaluatedValue;
        protected string VersionSuffix => MSBuildProject?.GetProperty("VersionSuffix")?.EvaluatedValue;
        protected string ReleaseVersion => MSBuildProject?.GetProperty("PackageVersion")?.EvaluatedValue ?? WebProject.GetProperty("Version");

        protected bool IsTheme => Solution == null;

        protected ModuleManifest ModuleManifest => ManifestReader.Read(ModuleManifestFile);

        protected AbsolutePath ModuleOutputDirectory => ArtifactsDirectory / ModuleManifest.Id;

        protected AbsolutePath DirectoryBuildPropsPath => Solution.Directory / "Directory.Build.props";

        protected string ZipFileName => IsModule ? $"{ModuleManifest.Id}_{ReleaseVersion}.zip" : $"{WebProject.Solution.Name}.{ReleaseVersion}.zip";
        protected string ZipFilePath => ArtifactsDirectory / ZipFileName;
        protected string GitRepositoryName => GitRepository.Identifier.Split('/')[1];

        protected string ModulePackageUrl => CustomModulePackageUri.IsNullOrEmpty()
            ? $"https://github.com/VirtoCommerce/{GitRepositoryName}/releases/download/{ReleaseVersion}/{ModuleManifest.Id}_{ReleaseVersion}.zip"
            : CustomModulePackageUri;

        protected GitRepository ModulesRepository => GitRepository.FromUrl(ModulesJsonRepoUrl);

        protected bool IsModule => FileExists(ModuleManifestFile);

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

        public Target Clean => _ => _
            .Before(Restore)
            .Executes(() =>
            {
                var searchPattern = new [] { "**/bin", "**/obj" };
                if (DirectoryExists(SourceDirectory))
                {
                    SourceDirectory.GlobDirectories(searchPattern).ForEach(DeleteDirectory);

                    if (DirectoryExists(TestsDirectory))
                    {
                        TestsDirectory.GlobDirectories(searchPattern).ForEach(DeleteDirectory);
                    }

                    if (DirectoryExists(SamplesDirectory))
                    {
                        SamplesDirectory.GlobDirectories(searchPattern).ForEach(DeleteDirectory);
                    }
                }
                else
                {
                    RootDirectory.GlobDirectories(searchPattern).ForEach(DeleteDirectory);
                }

                EnsureCleanDirectory(ArtifactsDirectory);
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
                            .SetOutputDirectory(ArtifactsDirectory)
                            .SetVersion(ReleaseVersion);
               DotNetPack(settings);
           });

        public Target Test => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                var testProjects = Solution.GetProjects("*.Test|*.Tests|*.Testing");
                var outPath = RootDirectory / ".tmp";

                foreach (var testProject in testProjects)
                {
                    DotNet($"add {testProject.Path} package coverlet.collector");

                    var testSetting = new DotNetTestSettings()
                        .SetProjectFile(testProject.Path)
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
                    var reportGenerator = ToolResolver.GetPackageTool("dotnet-reportgenerator-globaltool", "ReportGenerator.dll", "4.8.8", "netcoreapp3.0");
                    reportGenerator.Invoke($"-reports:{outPath / "**/coverage.cobertura.xml"} -targetdir:{outPath} -reporttypes:SonarQube");
                    var sonarCoverageReportPath = outPath.GlobFiles("SonarQube.xml").FirstOrDefault();

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

        public static void CustomDotnetLogger(OutputType type, string text)
        {
            Logger.Info(text);

            if (text.Contains("error: Response status code does not indicate success: 409"))
            {
                _exitCode = 409;
            }
        }

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
                    degreeOfParallelism: 1,
                    completeOnFailure: true);
            });

        public class Utf8StringWriter : StringWriter
        {
            // Use UTF8 encoding but write no BOM to the wire
            public override Encoding Encoding => new UTF8Encoding(false);
        }

        public void ChangeProjectVersion(string versionPrefix = null, string versionSuffix = null)
        {
            //theme
            if (IsTheme)
            {
                var jObject = SerializationTasks.JsonDeserializeFromFile<JObject>(PackageJsonPath);
                jObject["version"] = versionPrefix;
                SerializationTasks.JsonSerializeToFile(jObject, Path.GetFullPath(PackageJsonPath));
            }
            else
            {
                //module.manifest
                if (IsModule)
                {
                    var manifest = ModuleManifest.Clone();

                    if (!string.IsNullOrEmpty(versionPrefix))
                    {
                        manifest.Version = versionPrefix;
                    }

                    if (!string.IsNullOrEmpty(versionSuffix))
                    {
                        manifest.VersionTag = versionSuffix;
                    }

                    using (var writer = new Utf8StringWriter())
                    {
                        var xmlSerializer = new XmlSerializer(typeof(ModuleManifest));
                        xmlSerializer.Serialize(writer, manifest);
                        File.WriteAllText(ModuleManifestFile, writer.ToString(), Encoding.UTF8);
                    }
                }

                //Directory.Build.props
                var xmlDocument = new XmlDocument
                {
                    PreserveWhitespace = true,
                };

                xmlDocument.LoadXml(File.ReadAllText(DirectoryBuildPropsPath));

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

                using (var writer = new Utf8StringWriter())
                {
                    xmlDocument.Save(writer);
                    File.WriteAllText(DirectoryBuildPropsPath, writer.ToString());
                }
            }
        }

        public Target ChangeVersion => _ => _
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

            if (json.RootElement.TryGetProperty("version", out var version))
            {
                return version.GetString();
            }

            ControlFlow.Fail("No version found");
            return "";
        }

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

                var version = IsTheme
                    ? GetThemeVersion(PackageJsonPath)
                    : ReleaseVersion;

                var releaseBranchName = $"release/{version}";
                Logger.Info(Directory.GetCurrentDirectory());
                GitTasks.Git($"checkout -B {releaseBranchName}");
                GitTasks.Git($"push -u origin {releaseBranchName}");
            });

        public Target CompleteRelease => _ => _
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
                GitTasks.Git("checkout master");
                GitTasks.Git("pull");
                IncrementVersionPatch();
                var hotfixBranchName = $"hotfix/{CustomVersionPrefix}";
                Logger.Info(Directory.GetCurrentDirectory());
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

        public Target IncrementMinor => _ => _
            .Triggers(ChangeVersion)
            .Executes(IncrementVersionMinor);

        public Target IncrementPatch => _ => _
            .Triggers(ChangeVersion)
            .Executes(IncrementVersionPatch);

        public Target Publish => _ => _
            .DependsOn(Compile)
            .After(WebPackBuild, Test)
            .Executes(() =>
            {
                DotNetPublish(settings => settings
                    .SetProcessWorkingDirectory(WebProject.Directory)
                    .EnableNoRestore()
                    .SetOutput(IsModule ? ModuleOutputDirectory / "bin" : ArtifactsDirectory / "publish")
                    .SetConfiguration(Configuration));
            });

        public Target WebPackBuild => _ => _
            .Executes(() =>
            {
                if (FileExists(WebProject.Directory / "package.json"))
                {
                    NpmTasks.Npm("ci", WebProject.Directory);
                    NpmTasks.NpmRun(settings => settings.SetProcessWorkingDirectory(WebProject.Directory).SetCommand("webpack:build"));
                }
                else
                {
                    Logger.Info("Nothing to build.");
                }
            });

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
            .Executes(() =>
            {
                if (IsModule)
                {
                    //Copy module.manifest and all content directories into a module output folder
                    CopyFileToDirectory(ModuleManifestFile, ModuleOutputDirectory, FileExistsPolicy.Overwrite);

                    foreach (var folderName in _moduleContentFolders)
                    {
                        var sourcePath = WebProject.Directory / folderName;

                        if (DirectoryExists(sourcePath))
                        {
                            CopyDirectoryRecursively(sourcePath, ModuleOutputDirectory / folderName, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
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
                    var ignoreModuleFilesRegex = new Regex(@".+Module\..*", RegexOptions.IgnoreCase);
                    var includeModuleFilesRegex = new Regex(@$".*{ModuleManifest.Id}(Module)?\..*", RegexOptions.IgnoreCase);

                    CompressionTasks.CompressZip(ModuleOutputDirectory, ZipFilePath, x => !ignoredFiles.Contains(x.Name, StringComparer.OrdinalIgnoreCase) && !ignoreModuleFilesRegex.IsMatch(x.Name)
                                                                                          || includeModuleFilesRegex.IsMatch(x.Name));
                }
                else
                {
                    DeleteFile(ZipFilePath);
                    CompressionTasks.CompressZip(ArtifactsDirectory / "publish", ZipFilePath);
                }
            });

        public Target GetManifestGit => _ => _
            .Before(UpdateManifest)
            .Executes(() =>
            {
                GitTasks.GitLogger = GitLogger;

                if (!DirectoryExists(ModulesLocalDirectory))
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
                var externalManifests = JsonConvert.DeserializeObject<List<ExternalModuleManifest>>(TextTasks.ReadAllText(modulesJsonFilePath));
                var externalManifest = externalManifests?.FirstOrDefault(x => x.Id == manifest.Id);

                if (externalManifest != null)
                {
                    if (!manifest.VersionTag.IsNullOrEmpty() || !CustomVersionSuffix.IsNullOrEmpty())
                    {
                        manifest.VersionTag = manifest.VersionTag.EmptyToNull() ?? CustomVersionSuffix;

                        var externalPrereleaseVersion = externalManifest.Versions.FirstOrDefault(v => !v.VersionTag.IsNullOrEmpty());

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

                TextTasks.WriteAllText(modulesJsonFilePath, JsonConvert.SerializeObject(externalManifests, Formatting.Indented));
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
                var swashbuckle = ToolResolver.GetPackageTool("Swashbuckle.AspNetCore.Cli", "dotnet-swagger.dll", framework: "netcoreapp3.0");
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
                        Logger.Normal(message);
                    }

                    if (schemaValidationMessages.Any(t => (string)t["level"] == "error"))
                    {
                        ControlFlow.Fail("Schema Validation Messages contains error");
                    }
                }
            });

        public Target ValidateSwaggerSchema => _ => _
            .Requires(() => SwaggerSchemaPath != null)
            .Executes(async () =>
            {
                var responseContent = await SendSwaggerSchemaToValidator(_httpClient, SwaggerSchemaPath, SwaggerValidatorUri);
                var responseObject = JObject.Parse(responseContent);

                if (responseObject.TryGetValue("schemaValidationMessages", out var validationMessages))
                {
                    foreach (var message in validationMessages)
                    {
                        Logger.Normal(message);
                    }

                    if (validationMessages.Any(t => (string)t["level"] == "error"))
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
            var swaggerSchema = await File.ReadAllTextAsync(schemaPath);
            Logger.Normal($"Swagger schema length: {swaggerSchema.Length}");
            var requestContent = new StringContent(swaggerSchema, Encoding.UTF8, "application/json");
            Logger.Normal("Request content created");
            var request = new HttpRequestMessage(HttpMethod.Post, validatorUri);
            Logger.Normal("Request created");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = requestContent;
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Logger.Normal($"Response from Validator: {result}");
            return result;
        }

        public Target SonarQubeStart => _ => _
            .Executes(() =>
            {
                Logger.Normal($"IsServerBuild = {IsServerBuild}");
                var branchName = SonarBranchName ?? GitRepository.Branch;
                var branchNameTarget = SonarBranchNameTarget ?? GitRepository.Branch;
                Logger.Info($"BRANCH_NAME = {branchName}");

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
                            var result = args;
                            if (!string.IsNullOrEmpty(SonarPRProvider))
                                result = result.Add($"/d:sonar.pullrequest.provider={SonarPRProvider}");
                            if (!string.IsNullOrEmpty(SonarGithubRepo))
                                result = result.Add("/d:sonar.pullrequest.github.repository={value}", SonarGithubRepo);
                            return result;
                        }))
                    .When(!PullRequest, cc => cc
                        .SetBranchName(branchName)
                        .When(_sonarLongLiveBranches.Contains(branchName), ccc => ccc
                            .SetProcessArgumentConfigurator(args => args.Add($"/d:\"sonar.branch.target={branchNameTarget}\""))))
                );
            });

        public Target SonarQubeEnd => _ => _
            .After(SonarQubeStart)
            .DependsOn(Compile)
            .Executes(() =>
            {
                var framework = "net5.0";
                string toolPath = string.Empty;
                if (OperatingSystem.IsLinux())
                {
                    toolPath = ToolPathResolver.GetPackageExecutable(packageId: "dotnet-sonarscanner",
                        packageExecutable: "sonar-scanner", framework: framework);
                    var chmodTool = ToolResolver.GetPathTool("chmod");
                    chmodTool.Invoke($"+X {toolPath}").EnsureOnlyStd();
                }
                var output = SonarScannerTasks.SonarScannerEnd(c => c
                    .SetFramework(framework)
                    .When(!string.IsNullOrEmpty(toolPath), _ => c.SetProcessToolPath(toolPath))
                    .SetLogin(SonarAuthToken));

                var errors = output.Where(o => !o.Text.Contains(@"The 'files' list in config file 'tsconfig.json' is empty") && o.Type == OutputType.Err).ToList();

                if (errors.Any())
                {
                    ControlFlow.Fail(errors.Select(e => e.Text).Join(Environment.NewLine));
                }
            });

        public Target StartAnalyzer => _ => _
            .DependsOn(SonarQubeStart, SonarQubeEnd)
            .Executes(() =>
            {
                Logger.Normal("Sonar validation done.");
            });

        public Target MassPullAndBuild => _ => _
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

        private static void GitLogger(OutputType type, string text)
        {
            if (text.Contains("github returned 422 Unprocessable Entity") && text.Contains("already_exists"))
            {
                _exitCode = 422;
            }

            if (text.Contains("nothing to commit, working tree clean"))
            {
                _exitCode = 423;
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

                await githubClient.Repository.Release.UploadAsset(release, assetUpload);
            }
        }

        public Target Release => _ => _
            .DependsOn(Clean, Compress)
            .Requires(() => GitHubUser, () => GitHubToken)
            .Executes(() =>
            {
                var tag = ReleaseVersion;
                var description = File.Exists(ReleaseNotes) ? File.ReadAllText(ReleaseNotes) : "";

                try
                {
                    PublishRelease(GitHubUser, GitRepositoryName, GitHubToken, tag, description, ZipFilePath, PreRelease).Wait();
                }
                catch (AggregateException ex)
                {
                    var responseString = ((ApiValidationException)ex.InnerException)?.HttpResponse?.Body.ToString() ?? "";
                    var responseDocument = JsonDocument.Parse(responseString);
                    var alreadyExistsError = false;

                    if (responseDocument.RootElement.TryGetProperty("errors", out var errors))
                    {
                        var errorCount = errors.GetArrayLength();

                        if (errorCount > 0)
                        {
                            alreadyExistsError = errors.EnumerateArray().Any(e => e.GetProperty("code").GetString() == "already_exists");
                        }
                    }

                    if (alreadyExistsError)
                    {
                        _exitCode = 422;
                    }

                    ControlFlow.Fail(ex.Message);
                }
            });

        public Target ClearTemp => _ => _
            .Executes(() =>
            {
                ClearTempBeforeExit = true;
            });
    }
}
