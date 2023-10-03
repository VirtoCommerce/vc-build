using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using PlatformTools;
using PlatformTools.Azure;
using PlatformTools.Github;
using PlatformTools.Gitlab;
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

        [Parameter("vc-package.json path")]
        public static string PackageManifestPath { get; set; } = "./vc-package.json";

        [Parameter("Install params (install -module VirtoCommerce.Core:1.2.3)")]
        public static string[] Module { get; set; }

        [Parameter("Skip dependency solving")]
        public static bool SkipDependencySolving { get; set; }

        [Parameter("Install the platform", Name = "Platform")]
        public static bool PlatformParameter { get; set; }

        [Parameter("Custom platform asset url")]
        public static string PlatformAssetUrl { get; set; }

        [Parameter("Azure PAT")]
        public static string AzureToken { get; set; }
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

        [Parameter("Backup file path")] public static string BackupFile { get; set; } = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        public Target Init => _ => _
             .Executes(async () =>
             {
                 var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                 var packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName, platformRelease.Assets.First().BrowserDownloadUrl);
                 PackageManager.ToFile(packageManifest, PackageManifestPath);
             });

        public Target Install => _ => _
             .Triggers(InstallPlatform, InstallModules)
             .DependsOn(Backup)
             .Executes(async () =>
             {
                 var packageManifest = await OpenOrCreateManifest(PackageManifestPath, Edge);
                 var githubModuleSources = PackageManager.GetGithubModuleManifests(packageManifest);
                 var modules = PackageManager.GetGithubModules(packageManifest);

                 var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
                 var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleSources);

                 if (Module?.Length > 0 && !PlatformParameter)
                 {
                     UpdateModules(Module, externalModuleCatalog, modules);
                 }
                 else if (!PlatformParameter && !modules.Any() && !FileSystemTasks.FileExists((AbsolutePath)Path.GetFullPath(PackageManifestPath)))
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

        private void UpdateModules(string[] modulesArg, IModuleCatalog externalModuleCatalog, List<ModuleItem> modules)
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

                var existingModule = modules.FirstOrDefault(m => m.Id == module.Id);

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

        private IEnumerable<ModuleItem> ParseModuleParameter(string[] moduleStrings)
        {
            foreach (var moduleString in moduleStrings)
            {
                string moduleId;
                var moduleVersion = string.Empty;
                var parts = moduleString.Split(":");

                if (parts.Length > 1)
                {
                    moduleId = parts.First();
                    moduleVersion = parts.Last();
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

        private static bool IsModulesInstallation()
        {
            return !PlatformParameter && (!Module?.IsEmpty() ?? false);
        }

        private static bool PlatformVersionChanged()
        {
            var manifest = PackageManager.FromFile(PackageManifestPath);
            return IsPlatformInstallationNeeded(manifest.PlatformVersion);
        }

        public Target Backup => _ => _
            .Triggers(Rollback, RemoveBackup)
            .Before(Install, Update, InstallPlatform, InstallModules)
            .OnlyWhenDynamic(() => !IsServerBuild && Directory.EnumerateFileSystemEntries(RootDirectory).Any())
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
                CompressionTasks.CompressTarGZip(RootDirectory, BackupFile, filter: f => !f.FullName.StartsWith(RootDirectory / ".nuke") && !symlinks.Any(s => f.FullName.StartsWith(s)));

            });

        public Target Rollback => _ => _
            .DependsOn(Backup)
            .After(Backup, Install, Update, InstallPlatform, InstallModules)
            .OnlyWhenDynamic(() => FailedTargets.Any() && SucceededTargets.Contains(Backup))
            .AssuredAfterFailure()
            .Executes(() =>
            {
                CompressionTasks.UncompressTarGZip(BackupFile, RootDirectory);
            });

        public Target RemoveBackup => _ => _
            .After(Backup, Rollback)
            .OnlyWhenDynamic(() => FinishedTargets.Contains(Backup) && File.Exists(BackupFile))
            .AssuredAfterFailure()
            .Unlisted()
            .DependsOn(Backup)
            .Executes(() =>
            {
                FileSystemTasks.DeleteFile(BackupFile);
            });

        public Target InstallPlatform => _ => _
             .OnlyWhenDynamic(() => PlatformVersionChanged() && !IsModulesInstallation())
             .Executes(async () =>
             {
                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var mixedManifest = SerializationTasks.JsonDeserializeFromFile<MixedPackageManifest>(PackageManifestPath);
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
                FileSystemTasks.MoveFile(AppsettingsPath, tempFile, FileExistsPolicy.Overwrite);
            }

            CompressionTasks.Uncompress(platformZip, RootDirectory);
            FileSystemTasks.DeleteFile(platformZip);

            // return appsettings.json back
            if (!string.IsNullOrEmpty(tempFile))
            {
                var bakFileName = new StringBuilder("appsettings.")
                    .Append(DateTime.Now.ToString("MMddyyHHmmss"))
                    .Append(".bak");
                var destinationSettingsPath = !Force ? AppsettingsPath : Path.Join(Path.GetDirectoryName(AppsettingsPath), bakFileName.ToString());
                FileSystemTasks.MoveFile(tempFile, destinationSettingsPath, FileExistsPolicy.Overwrite);

                if (Force)
                {
                    Log.Information($"The old appsettings.json was saved as {bakFileName}");
                }
                else
                {
                    Log.Information($"appsettings.json was restored");
                }
            }
        }

        private string GetDiscoveryPath()
        {
            var configuration = AppSettings.GetConfiguration(RootDirectory, AppsettingsPath);
            return DiscoveryPath.EmptyToNull() ?? configuration.GetModulesDiscoveryPath();
        }

        private static bool IsPlatformInstallationNeeded(string version)
        {
            var result = true;
            var platformWebDllPath = RootDirectory / "VirtoCommerce.Platform.Web.dll";
            var newVersion = new Version(version);

            if (File.Exists(platformWebDllPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(platformWebDllPath);

                if (versionInfo.ProductVersion != null && newVersion <= Version.Parse(versionInfo.ProductVersion))
                {
                    result = false;
                }
            }

            return result;
        }

        public Target InstallModules => _ => _
             .After(InstallPlatform)
             .OnlyWhenDynamic(() => !PlatformParameter)
             .ProceedAfterFailure()
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
                 var alreadyInstalledModules = localModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.IsInstalled).ToList();

                 foreach (var module in githubReleases.Modules)
                 {
                     var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id == module.Id);

                     if (externalModule == null)
                     {
                         ExitCode = (int)ExitCodes.GithubNoModuleFound;
                         Assert.Fail($"No module {module.Id} found");
                         return;
                     }

                     if (alreadyInstalledModules.Any(installedModule => installedModule.ModuleName == module.Id && installedModule.Version.ToString() == module.Version))
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

                 var progress = new Progress<ProgressMessage>(m =>
                 {
                     if (m.Level == ProgressMessageLevel.Error)
                     {
                         Log.Error(m.Message);
                     }
                     else
                     {
                         Log.Information(m.Message);
                     }
                 });

                 if (!SkipDependencySolving)
                 {
                     var missingModules = externalModuleCatalog
                         .CompleteListWithDependencies(modulesToInstall)
                         .Except(modulesToInstall)
                         .OfType<ManifestModuleInfo>()
                         .Except(alreadyInstalledModules)
                         .ToList();

                     modulesToInstall.AddRange(missingModules);
                 }
                 modulesToInstall.ForEach(module =>
                 {
                     module.DependsOn.Clear();
                 });
                 try
                 {
                    moduleInstaller.Install(modulesToInstall, progress);
                 } catch (Exception ex)
                 {
                     Assert.Fail(ex.Message);
                 }

                 foreach (var moduleSource in moduleSources)
                 {
                     var installer = GetModuleInstaller(moduleSource);

                     await installer.Install(moduleSource);
                 }
                 var absoluteDiscoveryPath = (AbsolutePath)Path.GetFullPath(discoveryPath);
                 var zipFiles = absoluteDiscoveryPath.GlobFiles("*/*.zip");
                 zipFiles.ForEach(f => FileSystemTasks.DeleteFile(f));
                 localModuleCatalog.Reload();
             });

        private static ManifestModuleInfo LoadModuleInfo(ModuleItem module, ManifestModuleInfo externalModule)
        {
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

        private ModulesInstallerBase GetModuleInstaller(ModuleSource moduleSource) => moduleSource switch
        {
            AzurePipelineArtifacts => new AzurePipelineArtifactsModuleInstaller(AzureToken, GetDiscoveryPath()),
            AzureUniversalPackages => new AzureUniversalPackagesModuleInstaller(AzureToken, GetDiscoveryPath()),
            GithubPrivateRepos => new GithubPrivateModulesInstaller(GitHubToken, GetDiscoveryPath()),
            AzureBlob _ => new AzureBlobModuleInstaller(AzureToken, GetDiscoveryPath()),
            GitlabJobArtifacts _ => new GitlabJobArtifactsModuleInstaller(GitLabServer, GitLabToken, GetDiscoveryPath()),
            _ => throw new NotImplementedException("Unknown module source"),
        };

        public Target Uninstall => _ => _
             .Executes(async () =>
             {
                 var discoveryPath = GetDiscoveryPath();
                 var packageManifest = PackageManager.FromFile(PackageManifestPath);
                 var localModulesCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                 var githubModules = PackageManager.GetGithubModules(packageManifest);
                 FileSystemTasks.DeleteDirectory(ProbingPath);
                 Module.ForEach(m => FileSystemTasks.DeleteDirectory(Path.Combine(discoveryPath, m)));
                 githubModules.RemoveAll(m => Module.Contains(m.Id));
                 PackageManager.ToFile(packageManifest, PackageManifestPath);
                 if (PlatformVersion.CurrentVersion == null)
                 {
                     var platformRelease = await GithubManager.GetPlatformRelease(null);
                     PlatformVersion.CurrentVersion = SemanticVersion.Parse(platformRelease.TagName);
                 }
                 localModulesCatalog.Reload();
             });

        public Target Update => _ => _
             .Triggers(InstallPlatform, InstallModules)
             .DependsOn(Backup)
             .Executes(async () =>
             {
                 SkipDependencySolving = true;
                 var manifest = PackageManager.FromFile(PackageManifestPath);

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

        private async Task<ManifestBase> UpdateEdgeAsync(ManifestBase manifest, bool platformOnly)
        {
            manifest = await UpdateEdgePlatformAsync(manifest);
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
            manifest = await UpdateStablePlatformAsync(manifest, bundle);
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
            var githubModules = (GithubReleases)manifest.Sources.FirstOrDefault(s => s.Name == nameof(GithubReleases));
            if(githubModules == null)
            {
                Assert.Fail("There is no GithubReleases source in the manifest");
                return Task.FromResult((ManifestBase)manifest); // for sonarQube
            }

            var bundleGithubModules = (GithubReleases)bundle.Sources.FirstOrDefault(s => s.Name == nameof(GithubReleases));
            if(bundleGithubModules == null)
            {
                Assert.Fail($"Github releases not found in the bundle {BundleName}");
                return Task.FromResult((ManifestBase)manifest); // for sonarQube
            }

            foreach (var module in githubModules.Modules)
            {
                var bundleModule = bundleGithubModules.Modules.FirstOrDefault(m => m.Id == module.Id);
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

        private async Task<ManifestBase> UpdateEdgeModulesAsync(ManifestBase manifest)
        {
            var githubModules = PackageManager.GetGithubModules(manifest);
            var githubModuleManifests = PackageManager.GetGithubModuleManifests(manifest);

            var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleManifests);

            foreach (var module in githubModules)
            {
                var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.Id == module.Id).FirstOrDefault(m => m.Ref.Contains("github.com"));

                if (externalModule == null)
                {
                    var errorMessage = $"No module {module.Id} found";
                    Assert.Fail(errorMessage);
                    throw new ArgumentNullException(errorMessage); // for sonarQube
                }

                module.Version = externalModule.Version.ToString();
            }

            return manifest;
        }

        private async Task<ManifestBase> OpenOrCreateManifest(string packageManifestPath, bool isEdge)
        {
            ManifestBase packageManifest;
            var platformWebDllPath = Path.Combine(Directory.GetParent(packageManifestPath).FullName, "VirtoCommerce.Platform.Web.dll");
            if (!isEdge)
            {
                SkipDependencySolving = true;
                if(!File.Exists(packageManifestPath))
                {
                    await DownloadBundleManifest(BundleName, packageManifestPath);
                } 
                packageManifest = PackageManager.FromFile(packageManifestPath);
            }
            else if (!File.Exists(packageManifestPath) && File.Exists(platformWebDllPath))
            {
                var discoveryAbsolutePath = Path.GetFullPath(GetDiscoveryPath());
                packageManifest = CreateManifestFromEnvironment(RootDirectory, (AbsolutePath)discoveryAbsolutePath);
            }
            else if (!File.Exists(packageManifestPath))
            {
                Log.Information("vc-package.json does not exist.");
                Log.Information("Looking for the platform release");
                var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName);
            }
            else
            {
                SkipDependencySolving = true;
                packageManifest = PackageManager.FromFile(PackageManifestPath);
            }
            return packageManifest;
        }

        private static async Task DownloadBundleManifest(string bundleName, string outFile)
        {
            var rawBundlesFile = await HttpTasks.HttpDownloadStringAsync(BundlesUrl);
            var bundlesDictionary = SerializationTasks.JsonDeserialize<Dictionary<string, string>>(rawBundlesFile);
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
            await HttpTasks.HttpDownloadFileAsync(manifestUrl, outFile);
        }

        private static ManifestBase CreateManifestFromEnvironment(AbsolutePath platformPath, AbsolutePath discoveryPath)
        {
            var platformWebDllPath = platformPath / "VirtoCommerce.Platform.Web.dll";
            if (!FileSystemTasks.FileExists(platformWebDllPath))
            {
                Assert.Fail($"{platformWebDllPath} can't be found!");
            }
            var platformWebDllFileInfo = FileVersionInfo.GetVersionInfo(platformWebDllPath);
            var platformVersion = platformWebDllFileInfo.ProductVersion;
            var packageManifest = PackageManager.CreatePackageManifest(platformVersion);
            var githubModules = PackageManager.GetGithubModules(packageManifest);

            var manifests = discoveryPath.GlobFiles("*/module.manifest");
            manifests.ForEach(m =>
            {
                var manifest = ManifestReader.Read(m);
                githubModules.Add(new ModuleItem(manifest.Id, manifest.Version));
            });
            return packageManifest;
        }
    }
}
