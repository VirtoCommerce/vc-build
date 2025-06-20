using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using PlatformTools;
using PlatformTools.Extensions;
using PlatformTools.Modules;
using PlatformTools.Modules.Azure;
using PlatformTools.Modules.Github;
using PlatformTools.Modules.Gitlab;
using PlatformTools.Modules.LocalModules;
using Serilog;
using VirtoCommerce.Build.PlatformTools;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("Platform or Module version to install", Name = "Version")]
        public static string VersionToInstall { get; set; }

        private const string _packageManifestPathDefaultValue = "./vc-package.json";
        private static string _packageManifestPath = _packageManifestPathDefaultValue;

        [Parameter("vc-package.json path")]
        public static string PackageManifestPath
        {
            get {
                if (_packageManifestPath != _packageManifestPathDefaultValue)
                {
                    Assert.FileExists(_packageManifestPath.ToAbsolutePath());
                }
                return _packageManifestPath;
            }

            set => _packageManifestPath = value;
        }

        [Parameter("Install params (install -module VirtoCommerce.Core:1.2.3)")]
        public static string[] Module { get; set; }

        [Parameter("Skip dependency solving")]
        public static bool SkipDependencySolving { get; set; } = true;

        [Parameter("Install the platform", Name = "Platform")]
        public static bool PlatformParameter { get; set; }

        [Parameter("Custom platform asset url")]
        public static string PlatformAssetUrl { get; set; }

        [Parameter("Azure PAT")]
        public static string AzureToken { get; set; }

        [Parameter("Azure PAT for the Universal Packages")]
        public static string AzureUniversalPackagesPat { get; set; }

        [Parameter("Azure Blob SAS Token")]
        public static string AzureSasToken { get; set; }

        [Parameter("GitLab Token")]
        public static string GitLabToken { get; set; }

        [Parameter("Gitlab Server (default: https://gitlab.com/api/v4)")]
        public static string GitLabServer { get; set; } = "https://gitlab.com/api/v4";

        [Parameter("Bundle name (default: latest)", Name = "v")]
        public static string BundleName { get; set; } = "latest";

        [Parameter("Update to the edge versions")]
        public static bool Edge { get; set; }

        [Parameter("Url to Bundles file")]
        public static string BundlesUrl { get; set; } = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/bundles/stable.json";

        [Parameter("Backup file path")] public static AbsolutePath BackupFile { get; set; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        [Parameter("Modules discovery path")]
        public static string DiscoveryPath { get; set; }

        [Parameter("Probing path")]
        public static AbsolutePath ProbingPath { get; set; } = PlatformRootDirectory / "app_data" / "modules";

        [Parameter("appsettings.json path")]
        public static AbsolutePath AppsettingsPath { get; set; } = PlatformRootDirectory / "appsettings.json";
        public static AbsolutePath PlatformRootDirectory => IsPlatformSource ? WebDirectory : RootDirectory;

        [Parameter("Current Directory")]
        public static AbsolutePath CurrentDirectory { get; set; } = RootDirectory;

        public Target InitPlatform => _ => _
             .Executes(() =>
             {
                 var configuration = AppSettings.GetConfiguration(CurrentDirectory, AppsettingsPath);
                 LocalModuleCatalog.GetCatalog(DiscoveryPath.EmptyToNull() ?? configuration.GetModulesDiscoveryPath(), ProbingPath);
             });

        public Target Init => _ => _
             .Executes(async () =>
             {
                 var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                 var packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName, platformRelease.Assets[0].BrowserDownloadUrl);
                 PackageManager.ToFile(packageManifest, PackageManifestPath);
             });

        public Target Install => _ => _
             .Triggers(InstallPlatform, InstallModules, ValidateDependencies)
             .DependsOn(Backup)
             .Executes(async () =>
             {
                 var packageManifest = await OpenOrCreateManifest(PackageManifestPath.ToAbsolutePath(), Edge);
                 var githubModuleSources = PackageManager.GetGithubModuleManifests(packageManifest);
                 var modules = PackageManager.GetGithubModules(packageManifest);

                 var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
                 var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleSources);

                 if (Module?.Length > 0 && !PlatformParameter)
                 {
                     UpdateModules(Module, externalModuleCatalog, modules);
                 }
                 else if (!PlatformParameter && modules.IsEmpty() && !File.Exists(Path.GetFullPath(PackageManifestPath)))
                 {
                     AddCommerceModules(externalModuleCatalog, modules);
                 }
                 else if (PlatformParameter)
                 {
                     var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                     packageManifest.PlatformVersion = platformRelease.TagName;
                 }

                 PackageManager.ToFile(packageManifest, PackageManifestPath);
             });

        private static void UpdateModules(string[] modulesArg, IModuleCatalog externalModuleCatalog, List<ModuleItem> modules)
        {
            foreach (var module in ParseModuleParameter(modulesArg))
            {
                var externalModule = externalModuleCatalog.Modules
                    .OfType<ManifestModuleInfo>()
                    .FirstOrDefault(m => m.Id.EqualsInvariant(module.Id));

                if (externalModule == null)
                {
                    Log.Error($"Cannot find a module with ID '{module.Id}'");
                    continue;
                }

                if (!string.IsNullOrEmpty(module.Version) && externalModule.Version < SemanticVersion.Parse(module.Version))
                {
                    Log.Error($"The latest available version of module {module.Id} is {externalModule.Version}, but entered: {module.Version}");
                    continue;
                }

                module.Id = externalModule.Id;
                module.Version = module.Version.EmptyToNull() ?? externalModule.Version.ToString();

                var existingModule = modules.Find(m => m.Id == module.Id);

                if (existingModule == null)
                {
                    Log.Information($"Add {module.Id}:{module.Version}");
                    modules.Add(module);
                }
                else
                {
                    if (new Version(existingModule.Version) > new Version(module.Version))
                    {
                        Log.Error($"{module.Id}: Module downgrading isn't supported");
                        continue;
                    }

                    Log.Information($"Change version: {existingModule.Version} -> {module.Version}");
                    existingModule.Version = module.Version;
                }
            }
        }

        private static void AddCommerceModules(IModuleCatalog externalModuleCatalog, List<ModuleItem> modules)
        {
            Log.Information("Add group: commerce");
            var commerceModules = externalModuleCatalog.Modules
                .OfType<ManifestModuleInfo>()
                .Where(m => m.Groups.Contains("commerce"))
                .Select(m => new ModuleItem(m.Id, m.Version.ToString()));
            modules.AddRange(commerceModules);
        }

        private static IEnumerable<ModuleItem> ParseModuleParameter(string[] moduleStrings)
        {
            foreach (var moduleString in moduleStrings)
            {
                string moduleId;
                var moduleVersion = string.Empty;
                var parts = moduleString.Split(":");

                if (parts.Length > 1)
                {
                    moduleId = parts[0];
                    moduleVersion = parts[parts.Length - 1];
                }
                else if (moduleStrings.Length == 1 && !string.IsNullOrEmpty(VersionToInstall))
                {
                    moduleId = moduleString;
                    moduleVersion = VersionToInstall;
                }
                else
                {
                    moduleId = moduleString;
                }

                yield return new ModuleItem(moduleId, moduleVersion);
            }
        }

        private static bool IsModulesInstallation => !PlatformParameter && (!Module?.IsEmpty() ?? false);

        private static bool PlatformVersionChanged
        {
            get
            {
                var manifest = PackageManager.FromFile(PackageManifestPath);
                return IsPlatformInstallationNeeded(manifest.PlatformVersion);
            }
        }
        private static bool ThereAreFilesToBackup => Directory.EnumerateFileSystemEntries(CurrentDirectory).Any(p => !p.Contains(".nuke"));

        public Target Backup => _ => _
            .Triggers(Rollback, RemoveBackup)
            .Before(Install, Update, InstallPlatform, InstallModules)
            .OnlyWhenDynamic(() => IsLocalBuild && ThereAreFilesToBackup)
            .ProceedAfterFailure()
            .Executes(() =>
            {
                var discoveryPath = Path.GetFullPath(GetDiscoveryPath());
                var modulesDirs = new List<string>();
                if(Directory.Exists(discoveryPath))
                {
                    modulesDirs = Directory.EnumerateDirectories(discoveryPath).ToList();
                }
                var symlinks = modulesDirs.Where(m => new DirectoryInfo(m).LinkTarget != null).ToList();
                CompressionExtensions.TarGZipTo(CurrentDirectory, BackupFile, filter: f => !SkipFile(f.ToFileInfo()) && !symlinks.Exists(s => f.ToFileInfo().FullName.StartsWith(s)));
            });

        private static bool SkipFile(FileInfo fileInfo)
        {
            const string nodeModules = "node_modules";
            return fileInfo.FullName.StartsWith(CurrentDirectory / ".nuke") || fileInfo.FullName.Contains(nodeModules);
        }

        public bool ThereAreFailedTargets => FailedTargets.Count > 0 && SucceededTargets.Contains(Backup);
        public Target Rollback => _ => _
            .DependsOn(Backup)
            .After(Backup, Install, Update, InstallPlatform, InstallModules)
            .OnlyWhenDynamic(() => ThereAreFailedTargets)
            .AssuredAfterFailure()
            .Executes(() => CompressionExtensions.UnTarGZipTo(BackupFile, CurrentDirectory));

        public bool WorkIsDone => FinishedTargets.Contains(Backup) && File.Exists(BackupFile);
        public Target RemoveBackup => _ => _
            .After(Backup, Rollback)
            .OnlyWhenDynamic(() => WorkIsDone)
            .AssuredAfterFailure()
            .Unlisted()
            .DependsOn(Backup)
            .Executes(() => BackupFile.DeleteFile());

        public Target InstallPlatform => _ => _
             .OnlyWhenDynamic(() => PlatformVersionChanged && !IsModulesInstallation && !IsPlatformSource)
             .Executes(async () =>
             {
                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var mixedManifest = PackageManifestPath.ToAbsolutePath().ReadJson<MixedPackageManifest>();
                 var platformAssetUrlFromManifest = mixedManifest.PlatformAssetUrl;
                 var platformAssetUrl = string.IsNullOrWhiteSpace(PlatformAssetUrl)
                     ? platformAssetUrlFromManifest
                     : PlatformAssetUrl;
                 await InstallPlatformAsync(packageManifest?.PlatformVersion, platformAssetUrl);
             });

        private static async Task InstallPlatformAsync(string platformVersion, string platformAssetUrl)
        {
            if (string.IsNullOrWhiteSpace(platformAssetUrl))
            {
                Log.Information($"Installing platform {platformVersion}");
                var platformRelease = await GithubManager.GetPlatformRelease(platformVersion);
                platformAssetUrl = platformRelease.Assets[0].BrowserDownloadUrl;
            }
            else
            {
                Log.Information($"Installing platform {platformAssetUrl}");
            }

            var platformZip = TemporaryDirectory / "platform.zip";

            if (string.IsNullOrEmpty(platformAssetUrl))
            {
                Assert.Fail($"No platform's assets found with tag {platformVersion}");
            }

            await HttpTasks.HttpDownloadFileAsync(platformAssetUrl, platformZip);

            // backup appsettings.json if exists
            var tempFile = string.Empty;
            if (File.Exists(AppsettingsPath))
            {
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                AppsettingsPath.Move(tempFile, ExistsPolicy.FileOverwrite);
            }

            platformZip.UncompressTo(CurrentDirectory);
            platformZip.DeleteFile();

            // return appsettings.json back
            if (!string.IsNullOrEmpty(tempFile))
            {
                var bakFileName = new StringBuilder("appsettings.")
                    .Append(DateTime.Now.ToString("MMddyyHHmmss"))
                    .Append(".bak");
                AbsolutePath destinationSettingsPath = !Force ? AppsettingsPath : Path.Join(Path.GetDirectoryName(AppsettingsPath), bakFileName.ToString());
                tempFile.ToAbsolutePath().Move(destinationSettingsPath, ExistsPolicy.FileOverwrite);

                if (Force)
                {
                    Log.Warning("The old appsettings.json was saved as {0}", bakFileName);
                }
                else
                {
                    Log.Information("appsettings.json was restored");
                }
            }
        }

        private static string GetDiscoveryPath()
        {
            if (DiscoveryPath.IsNullOrEmpty())
            {
                var configuration = AppSettings.GetConfiguration(PlatformRootDirectory, AppsettingsPath);
                var path = configuration.GetModulesDiscoveryPath();
                if (!Path.IsPathRooted(path))
                {
                    path = Path.GetFullPath(path, PlatformRootDirectory);
                }
                return path;
            }

            return DiscoveryPath;
        }

        private static bool IsPlatformInstallationNeeded(string version)
        {
            var result = true;
            var platformWebDllPath = CurrentDirectory / "VirtoCommerce.Platform.Web.dll";
            var newVersion = new Version(version);

            if (File.Exists(platformWebDllPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(platformWebDllPath);

                if (versionInfo.FileVersion != null && newVersion <= Version.Parse(versionInfo.FileVersion))
                {
                    result = false;
                }
            }

            return result;
        }

        public Target InstallModules => _ => _
             .After(InstallPlatform)
             .OnlyWhenDynamic(() => !PlatformParameter)
             .Executes(async () =>
             {
                 if (!RunningTargets.Contains(Install))
                 {
                     SkipDependencySolving = true;
                 }

                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var moduleSources = PackageManager.GetModuleSources(packageManifest).Where(s => s is not GithubReleases).ToList();
                 var githubReleases = PackageManager.GetGithubModulesSource(packageManifest);
                 var discoveryPath = GetDiscoveryPath();
                 var localModuleCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                 var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubReleases.ModuleSources);
                 var moduleInstaller = await ModuleInstallerFacade.GetModuleInstaller(discoveryPath, ProbingPath, GitHubToken, githubReleases.ModuleSources);
                 var modulesToInstall = new List<ManifestModuleInfo>();
                 var alreadyInstalledModules = localModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.IsInstalled || IsNonVirtoModuleInstalled(m, (MixedPackageManifest)packageManifest)).ToList();

                 // Remove modules that are no longer in the manifest
                 var modulesToRemove = alreadyInstalledModules
                     .Where(m => !githubReleases.Modules.Exists(module =>
                         module.Id.EqualsInvariant(m.ModuleName) ||
                         module.Id.EqualsInvariant(m.Id)) &&
                         !localModuleCatalog.IsModuleSymlinked(m.ModuleName) &&
                         !IsNonVirtoModuleInstalled(m, (MixedPackageManifest)packageManifest))
                     .ToList();

                 RemoveModules(localModuleCatalog, modulesToRemove);

                 AddVirtoModules(githubReleases, localModuleCatalog, externalModuleCatalog, modulesToInstall, alreadyInstalledModules);

                 var progress = PlatformProgressHandler();

                 SolveDependenciesIfRequested(externalModuleCatalog, modulesToInstall, alreadyInstalledModules);

                 modulesToInstall.ForEach(module => module.DependsOn.Clear());
                 moduleInstaller.Install(modulesToInstall, progress);

                 Assert.False(ExitCode > 0, "Errors occurred while installing modules.");

                 foreach (var moduleSource in moduleSources)
                 {
                     var installer = GetModuleInstaller(moduleSource);

                     await installer.Install(moduleSource, progress);
                 }
                 CleanZipArtifacts(discoveryPath);
                 localModuleCatalog.Reload();
             });

        public Target ValidateDependencies => _ => _
             .OnlyWhenDynamic(() => !PlatformParameter && IsLocalBuild)
             .After(Backup, Update, Install, InstallPlatform, InstallModules)
             .Before(Rollback)
             .Executes(async () =>
             {
                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var githubReleases = PackageManager.GetGithubModulesSource(packageManifest);
                 var discoveryPath = GetDiscoveryPath();
                 var localModuleCatalog = (LocalCatalog)LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                 await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubReleases.ModuleSources);
                 if (!localModuleCatalog.IsDependenciesValid() && SucceededTargets.Contains(Backup))
                 {
                     ProceedWithErrorsOrFail();
                 }
             });

        private static void ProceedWithErrorsOrFail()
        {
            Log.Information("Do you want to proceed with the errors or perform a rollback? ((p)roceed/(r)ollback): ");

            var response = Console.ReadLine()?.ToLower();
            if (response == "r")
            {
                Assert.Fail("Dependencies are not valid. Rollback is requested.");
            }
            else if (response == "p")
            {
                Log.Warning("Proceeding with errors. This may lead to unexpected behavior.");
            }
        }

        private static void SolveDependenciesIfRequested(Platform.Modules.ExternalModuleCatalog externalModuleCatalog, List<ManifestModuleInfo> modulesToInstall, List<ManifestModuleInfo> alreadyInstalledModules)
        {
            if (!SkipDependencySolving && Edge)
            {
                var missingModules = externalModuleCatalog
                    .CompleteListWithDependencies(modulesToInstall)
                    .Except(modulesToInstall)
                    .OfType<ManifestModuleInfo>()
                    .Except(alreadyInstalledModules)
                    .ToList();

                modulesToInstall.AddRange(missingModules);
            }
        }

        private void AddVirtoModules(GithubReleases githubReleases, ILocalModuleCatalog localModuleCatalog, Platform.Modules.ExternalModuleCatalog externalModuleCatalog, List<ManifestModuleInfo> modulesToInstall, List<ManifestModuleInfo> alreadyInstalledModules)
        {
            foreach (var (module, externalModule) in from module in githubReleases.Modules
                                                     let externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id == module.Id)
                                                     select (module, externalModule))
            {
                if (externalModule == null)
                {
                    ExitCode = (int)ExitCodes.GithubNoModuleFound;
                    Assert.Fail($"No module {module.Id} found");
                    return;
                }

                if (alreadyInstalledModules.Exists(installedModule => installedModule.ModuleName == module.Id && installedModule.Version.ToString() == module.Version) || localModuleCatalog.IsModuleSymlinked(module.Id))
                {
                    continue;
                }

                try
                {
                    var moduleInfo = LoadModuleInfo(module, externalModule);
                    modulesToInstall.Add(moduleInfo);
                }
                catch (Exception ex)
                {
                    ExitCode = (int)ExitCodes.ModuleCouldNotBeLoaded;
                    Assert.Fail($"Could not load module '{module.Id}'", ex);
                }
            }
        }

        private static void RemoveModules(ILocalModuleCatalog localModuleCatalog, List<ManifestModuleInfo> modulesToRemove)
        {
            foreach (var moduleToRemove in modulesToRemove)
            {
                Log.Information($"Removing module {moduleToRemove.ModuleName} as it's no longer in the manifest");
                var moduleToDelete = ((LocalCatalog)localModuleCatalog).Items.OfType<ManifestModuleInfo>().FirstOrDefault(m =>
                    m.ModuleName.EqualsInvariant(moduleToRemove.ModuleName) ||
                    m.Id.EqualsInvariant(moduleToRemove.ModuleName));
                var modulePath = moduleToDelete?.FullPhysicalPath;
                if (Directory.Exists(modulePath))
                {
                    Directory.Delete(modulePath, true);
                }
            }
        }

        private static bool IsNonVirtoModuleInstalled(ManifestModuleInfo module, MixedPackageManifest manifest)
        {
            var moduleDirectoryName = Path.GetFileName(module.FullPhysicalPath);

            // Skip GithubReleases sources
            var nonGithubSources = manifest.Sources.Where(s => !(s is GithubReleases));

            return nonGithubSources.Any(source => IsModuleInSource(source, moduleDirectoryName));
        }

        private static bool IsModuleInSource(ModuleSource source, string moduleDirectoryName)
        {
            return source switch
            {
                AzureBlob azureBlob => CheckModules(azureBlob.Modules, m => m.BlobName.Replace(".zip", ""), moduleDirectoryName),
                AzurePipelineArtifacts azurePipeline => CheckModules(azurePipeline.Modules, m => m.Id, moduleDirectoryName),
                AzureUniversalPackages azureUniversal => CheckModules(azureUniversal.Modules, m => m.Id, moduleDirectoryName),
                GithubPrivateRepos githubPrivate => CheckModules(githubPrivate.Modules, m => m.Id, moduleDirectoryName),
                GitlabJobArtifacts gitlabJob => CheckModules(gitlabJob.Modules, m => m.Id, moduleDirectoryName),
                Local local => CheckModules(local.Modules, m => string.IsNullOrWhiteSpace(m.Id) ? Path.GetFileName(m.Path) : m.Id, moduleDirectoryName),
                _ => false
            };
        }

        private static bool CheckModules<T>(IEnumerable<T> modules, Func<T, string> nameSelector, string moduleDirectoryName)
        {
            if (modules == null || !modules.Any())
            {
                return false;
            }

            return modules.Any(m => nameSelector(m) == moduleDirectoryName);
        }

        private Progress<ProgressMessage> PlatformProgressHandler()
        {
            return new Progress<ProgressMessage>(m =>
            {
                if (m.Level == ProgressMessageLevel.Error)
                {
                    ExitCode = 1;
                    Log.Error(m.Message);

                }
                else
                {
                    Log.Information(m.Message);
                }
            });
        }

        private static void CleanZipArtifacts(string discoveryPath)
        {
            AbsolutePath absoluteDiscoveryPath = Path.GetFullPath(discoveryPath);
            var zipFiles = absoluteDiscoveryPath.GlobFiles("*/*.zip");
            zipFiles.ForEach(f => f.DeleteFile());
        }

        private static ManifestModuleInfo LoadModuleInfo(ModuleItem module, ManifestModuleInfo externalModule)
        {
            if (!externalModule.Ref.Contains(externalModule.Version.ToString()))
            {
                Log.Error("Error in file modules_v3.json for module {0}: Version {1} not found in Reference {2}", externalModule.Id, externalModule.Version.ToString(), externalModule.Ref);
            }

            var currentModule = new ModuleManifest
            {
                Id = module.Id,
                Version = module.Version,
                Dependencies = SkipDependencySolving
                    ? null
                    : externalModule.Dependencies.Select(d => new ManifestDependency
                    {
                        Id = d.Id,
                        Version = d.Version.ToString(),
                    }).ToArray(),
                PackageUrl = externalModule.Ref.Replace(externalModule.Version.ToString(), module.Version),
                Authors = externalModule.Authors.ToArray(),
                PlatformVersion = externalModule.PlatformVersion.ToString(),
                Incompatibilities = externalModule.Incompatibilities.Select(d => new ManifestDependency
                {
                    Id = d.Id,
                    Version = d.Version.ToString(),
                }).ToArray(),
                Groups = externalModule.Groups.Select(g => g).ToArray(),
                Copyright = externalModule.Copyright,
                Description = externalModule.Description,
                IconUrl = externalModule.IconUrl,
                Owners = externalModule.Owners.ToArray(),
                ProjectUrl = externalModule.ProjectUrl,
                ReleaseNotes = externalModule.ReleaseNotes,
                Tags = externalModule.Tags,
                Title = externalModule.Title,
                VersionTag = externalModule.VersionTag,
                LicenseUrl = externalModule.LicenseUrl,
                ModuleType = externalModule.ModuleType,
                RequireLicenseAcceptance = externalModule.RequireLicenseAcceptance,
                UseFullTypeNameInSwagger = externalModule.UseFullTypeNameInSwagger,
            };

            var moduleInfo = new ManifestModuleInfo().LoadFromManifest(currentModule);
            return moduleInfo;
        }

        private static ModuleInstallerBase GetModuleInstaller(ModuleSource moduleSource) => moduleSource switch
        {
            AzurePipelineArtifacts => new AzurePipelineArtifactsModuleInstaller(AzureToken, GetDiscoveryPath()),
            AzureUniversalPackages => new AzureUniversalPackagesModuleInstaller(AzureUniversalPackagesPat ?? AzureToken, GetDiscoveryPath()),
            GithubPrivateRepos => new GithubPrivateModulesInstaller(GitHubToken, GetDiscoveryPath()),
            AzureBlob _ => new AzureBlobModuleInstaller(AzureSasToken ?? AzureToken, GetDiscoveryPath()),
            GitlabJobArtifacts _ => new GitlabJobArtifactsModuleInstaller(GitLabServer, GitLabToken, GetDiscoveryPath()),
            Local _ => new LocalModuleInstaller(GetDiscoveryPath()),
            _ => throw new NotImplementedException("Unknown module source"),
        };

        public Target Uninstall => _ => _
             .Executes(async () =>
             {
                 var discoveryPath = GetDiscoveryPath();
                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var localModulesCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                 var githubModules = PackageManager.GetGithubModules(packageManifest);
                 ProbingPath.DeleteDirectory();
                 Module.ForEach(m => AbsolutePath.Create(Path.Combine(discoveryPath, m)).DeleteDirectory());
                 githubModules.RemoveAll(m => Module.Contains(m.Id));
                 PackageManager.ToFile(packageManifest, PackageManifestPath);
                 if (PlatformVersion.CurrentVersion == null)
                 {
                     var platformRelease = await GithubManager.GetPlatformRelease();
                     PlatformVersion.CurrentVersion = SemanticVersion.Parse(platformRelease.TagName);
                 }
                 localModulesCatalog.Reload();
             });

        public Target Update => _ => _
             .Triggers(InstallPlatform, InstallModules, ValidateDependencies)
             .DependsOn(Backup, ShowDiff)
             .Executes(async () =>
             {
                 SkipDependencySolving = true;
                 var manifest = await OpenOrCreateManifest(PackageManifestPath.ToAbsolutePath(), Edge);

                 if (Edge)
                 {
                     manifest = await UpdateEdgeAsync(manifest, PlatformParameter);
                 }
                 else
                 {
                     manifest =  await UpdateStableAsync(manifest, PlatformParameter, BundleName);
                 }

                 PackageManager.ToFile(manifest, PackageManifestPath);
             });

        private static async Task<ManifestBase> UpdateEdgeAsync(ManifestBase manifest, bool platformOnly)
        {
            if (!IsPlatformSource)
            {
                manifest = await UpdateEdgePlatformAsync(manifest);
            }

            if(!platformOnly)
            {
                manifest = await UpdateEdgeModulesAsync(manifest);
            }
            return manifest;
        }

        private async Task<ManifestBase> UpdateStableAsync(ManifestBase manifest, bool platformOnly, string bundleName)
        {
            var bundleTmpFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            await DownloadBundleManifest(bundleName, bundleTmpFilePath);

            var bundle = PackageManager.FromFile(bundleTmpFilePath);

            if (!IsPlatformSource)
            {
                manifest = await UpdateStablePlatformAsync(manifest, bundle);
            }

            if (!platformOnly)
            {
                manifest = await UpdateStableModulesAsync((MixedPackageManifest)manifest, (MixedPackageManifest)bundle);
            }
            return manifest;
        }

        private static Task<ManifestBase> UpdateStablePlatformAsync(ManifestBase manifest, ManifestBase bundle)
        {
            manifest.PlatformVersion = bundle.PlatformVersion;
            return Task.FromResult(manifest);
        }

        private Task<ManifestBase> UpdateStableModulesAsync(MixedPackageManifest manifest, MixedPackageManifest bundle)
        {
            var githubModules = (GithubReleases)manifest.Sources.Find(s => s.Name == nameof(GithubReleases));
            if(githubModules == null)
            {
                Assert.Fail("There is no GithubReleases source in the manifest");
                return Task.FromResult((ManifestBase)manifest); // for sonarQube
            }

            var bundleGithubModules = (GithubReleases)bundle.Sources.Find(s => s.Name == nameof(GithubReleases));
            if(bundleGithubModules == null)
            {
                Assert.Fail($"Github releases not found in the bundle {BundleName}");
                return Task.FromResult((ManifestBase)manifest); // for sonarQube
            }

            foreach (var module in githubModules.Modules)
            {
                var bundleModule = bundleGithubModules.Modules.Find(m => m.Id == module.Id);
                if(bundleModule != null)
                {
                    module.Version = bundleModule.Version;
                }
            }
            return Task.FromResult((ManifestBase)manifest);
        }
        private static async Task<ManifestBase> UpdateEdgePlatformAsync(ManifestBase manifest)
        {
            var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
            manifest.PlatformVersion = platformRelease.TagName;
            return manifest;
        }

        private static async Task<ManifestBase> UpdateEdgeModulesAsync(ManifestBase manifest)
        {
            var githubModules = PackageManager.GetGithubModules(manifest);
            var githubModuleManifests = PackageManager.GetGithubModuleManifests(manifest);

            var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleManifests);

            foreach (var module in githubModules)
            {
                var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id == module.Id);

                if (externalModule == null)
                {
                    var errorMessage = $"No module {module.Id} found";
                    Assert.Fail(errorMessage);
                    throw new ArgumentNullException(errorMessage); // for sonarQube
                }
                else if (externalModule.Ref.StartsWith("file:///"))
                {
                    Log.Information($"{module.Id} already installed.");
                    continue;
                }

                module.Version = externalModule.Version.ToString();
            }

            return manifest;
        }

        private static async Task<ManifestBase> OpenOrCreateManifest(string packageManifestPath, bool isEdge)
        {
            ManifestBase result;
            var platformWebDllPath = Path.Combine(Directory.GetParent(packageManifestPath).FullName, "VirtoCommerce.Platform.Web.dll");
            if(!File.Exists(packageManifestPath))
            {
                if (!isEdge) //Stable
                {
                    await DownloadBundleManifest(BundleName, packageManifestPath);
                    result = PackageManager.FromFile(packageManifestPath);
                }
                else if (File.Exists(platformWebDllPath)) // There is platform
                {
                    var discoveryAbsolutePath = Path.GetFullPath(GetDiscoveryPath());
                    result = await CreateManifestFromEnvironment(PlatformRootDirectory, discoveryAbsolutePath.ToAbsolutePath());
                }
                else // Create new
                {
                    Log.Information("vc-package.json does not exist.");
                    Log.Information("Looking for the platform release");
                    var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                    result = PackageManager.CreatePackageManifest(platformRelease.TagName);
                }
            }
            else
            {
                result = PackageManager.FromFile(packageManifestPath);
            }

            return result;
        }

        private static async Task DownloadBundleManifest(string bundleName, string outFile)
        {
            var rawBundlesFile = await HttpTasks.HttpDownloadStringAsync(BundlesUrl);
            var bundlesDictionary = JsonExtensions.GetJson<Dictionary<string, string>>(rawBundlesFile);
            KeyValuePair<string, string> bundle;
            if (string.IsNullOrEmpty(bundleName))
            {
                bundle = bundlesDictionary.LastOrDefault();
            }
            else
            {
                bundle = bundlesDictionary.FirstOrDefault(kv => kv.Key == bundleName);
            }

            var manifestUrl = bundle.Value;
            await HttpTasks.HttpDownloadFileAsync(manifestUrl, outFile.ToAbsolutePath());
        }

        private async static Task<ManifestBase> CreateManifestFromEnvironment(AbsolutePath platformPath, AbsolutePath discoveryPath)
        {
            var platformWebDllPath = platformPath / "VirtoCommerce.Platform.Web.dll";
            if (!File.Exists(platformWebDllPath))
            {
                Assert.Fail($"{platformWebDllPath} can't be found!");
            }
            var platformWebDllFileInfo = FileVersionInfo.GetVersionInfo(platformWebDllPath);
            var platformVersion = platformWebDllFileInfo.ProductVersion;
            var packageManifest = PackageManager.CreatePackageManifest(platformVersion);
            var githubModules = PackageManager.GetGithubModules(packageManifest);
            var githubModulesSource = PackageManager.GetGithubModulesSource(packageManifest);
            var localModuleCatalog = (LocalCatalog)LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModulesSource.ModuleSources);
            var modulesInCatalog = externalModuleCatalog.Modules.Where(m => !m.Ref.StartsWith("file://")).Select(m => m.ModuleName).ToList();
            var manifests = discoveryPath.GlobFiles("*/module.manifest");
            manifests.ForEach(m =>
            {
                var manifest = ManifestReader.Read(m);
                if (!modulesInCatalog.Contains(manifest.Id))
                {
                    Log.Warning("There is no module {0}:{1} in external catalog. You should add it in manifest manually.", manifest.Id, manifest.Version);
                }
                else
                {
                    githubModules.Add(new ModuleItem(manifest.Id, manifest.Version));
                }
            });

            return packageManifest;
        }

        public Target ShowDiff => _ => _
             .Before(Update)
             .OnlyWhenDynamic(() => IsLocalBuild)
             .Executes(async () =>
             {
                 var manifest = PackageManager.FromFile(PackageManifestPath);
                 var bundle = await GetBundleManifest(manifest);
                 ShowPlatformDiff(manifest, bundle);
                 ShowModulesDiff(manifest, bundle);
                 await ConfirmUpdate();
             });

        private static async Task<MixedPackageManifest> GetBundleManifest(ManifestBase manifest)
        {
            if (Edge)
            {
                return await GetEdgeBundleManifest(manifest);
            }
            return await GetStableBundleManifest();
        }

        private static async Task<MixedPackageManifest> GetEdgeBundleManifest(ManifestBase manifest)
        {
            var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
            var bundle = (MixedPackageManifest)PackageManager.CreatePackageManifest(platformRelease.TagName);
            var githubModules = PackageManager.GetGithubModules(manifest);
            var githubModuleManifests = PackageManager.GetGithubModuleManifests(manifest);
            var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleManifests);
            var modules = await GetEdgeModuleVersions(githubModules, externalModuleCatalog);
            var bundleGithubReleases = new GithubReleases
            {
                ModuleSources = githubModuleManifests,
                Modules = modules
            };
            bundle.Sources.Clear();
            bundle.Sources.Add(bundleGithubReleases);
            return bundle;
        }

        private static Task<List<ModuleItem>> GetEdgeModuleVersions(List<ModuleItem> githubModules, IModuleCatalog externalModuleCatalog)
        {
            var modules = new List<ModuleItem>();
            foreach (var module in githubModules)
            {
                var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id == module.Id);
                if (externalModule == null)
                {
                    Log.Warning($"No module {module.Id} found in external catalog");
                    continue;
                }
                if (externalModule.Ref.StartsWith("file:///"))
                {
                    Log.Information($"{module.Id} is a local module, skipping version check");
                    continue;
                }
                modules.Add(new ModuleItem(module.Id, externalModule.Version.ToString()));
            }
            return Task.FromResult(modules);
        }

        private static async Task<MixedPackageManifest> GetStableBundleManifest()
        {
            var bundleTmpFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            await DownloadBundleManifest(BundleName, bundleTmpFilePath);
            var bundle = (MixedPackageManifest)PackageManager.FromFile(bundleTmpFilePath);
            File.Delete(bundleTmpFilePath);
            return bundle;
        }

        private static void ShowPlatformDiff(ManifestBase manifest, MixedPackageManifest bundle)
        {
            Log.Information("=== Platform Version Differences ===");
            Log.Information($"Current: {manifest.PlatformVersion}");
            Log.Information($"Target: {bundle.PlatformVersion}");
        }

        private static void ShowModulesDiff(ManifestBase manifest, MixedPackageManifest bundle)
        {
            var manifestGithubModules = PackageManager.GetGithubModules(manifest);
            var bundleGithubModules = PackageManager.GetGithubModules(bundle);

            Log.Information("\n=== Module Version Differences ===");
            ShowExistingModuleDiffs(manifestGithubModules, bundleGithubModules);
        }

        private static void ShowExistingModuleDiffs(List<ModuleItem> manifestModules, List<ModuleItem> bundleModules)
        {
            foreach (var manifestModule in manifestModules)
            {
                var bundleModule = bundleModules.Find(m => m.Id == manifestModule.Id);
                if (bundleModule != null)
                {
                    if (manifestModule.Version != bundleModule.Version)
                    {
                        Log.Information($"{manifestModule.Id}: {manifestModule.Version} -> {bundleModule.Version}");
                    }
                }
                else
                {
                    Log.Information($"{manifestModule.Id}: {manifestModule.Version} (not in target)");
                }
            }
        }

        private static Task ConfirmUpdate()
        {
            Log.Information("\nDo you want to proceed with the update? (y/n)");
            var response = Console.ReadLine()?.ToLower();
            if (response != "y")
            {
                Log.Information("Update cancelled by user");
                Environment.Exit(0);
            }

            return Task.CompletedTask;
        }
    }
}
