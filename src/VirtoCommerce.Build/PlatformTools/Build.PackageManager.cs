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
using VirtoCommerce.Build.PlatformTools;
using VirtoCommerce.Build.PlatformTools.Azure;
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

        [Parameter("Azure PAT")]
        public static string AzureToken { get; set; }

        public Target Init => _ => _
            .Executes(async () =>
            {
                var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                var packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName, platformRelease.Assets.First().BrowserDownloadUrl);
                PackageManager.ToFile(packageManifest, PackageManifestPath);
            });

        public Target Install => _ => _
            .Triggers(InstallPlatform, InstallModules)
            .Executes(async () =>
            {
                ManifestBase packageManifest = await OpenOrCreateManifets(PackageManifestPath);
                List<string> githubModuleSources = PackageManager.GetGithubModuleManifests(packageManifest);
                List<ModuleItem> modules = PackageManager.GetGithubModules(packageManifest);


                var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
                var externalModuleCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleSources);

                if (Module?.Length > 0 && !PlatformParameter)
                {
                    foreach (var module in ParseModuleParameter(Module))
                    {
                        var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id.EqualsInvariant(module.Id));

                        if (externalModule == null)
                        {
                            Logger.Error($"Cannot find a module with ID '{module.Id}'");
                            continue;
                        }

                        if (!string.IsNullOrEmpty(module.Version) && externalModule.Version < new SemanticVersion(new Version(module.Version)))
                        {
                            Logger.Error($"The latest available version of module {module.Id} is {externalModule.Version}, but entered: {module.Version}");
                            continue;
                        }

                        module.Id = externalModule.Id;
                        module.Version = module.Version.EmptyToNull() ?? externalModule.Version.ToString();

                        var existingModule = modules.FirstOrDefault(m => m.Id == module.Id);

                        if (existingModule == null)
                        {
                            Logger.Info($"Add {module.Id}:{module.Version}");
                            modules.Add(module);
                        }
                        else
                        {
                            if (new Version(existingModule.Version) > new Version(module.Version))
                            {
                                Logger.Error($"{module.Id}: Module downgrading isn't supported");
                                continue;
                            }

                            Logger.Info($"Change version: {existingModule.Version} -> {module.Version}");
                            existingModule.Version = module.Version;
                        }
                    }
                }
                else if (!PlatformParameter && !modules.Any())
                {
                    Logger.Info("Add group: commerce");
                    var commerceModules = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.Groups.Contains("commerce")).Select(m => new ModuleItem(m.Id, m.Version.ToString()));
                    modules.AddRange(commerceModules);
                }
                else if (PlatformParameter)
                {
                    var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                    packageManifest.PlatformVersion = platformRelease.TagName;
                }

                PackageManager.ToFile(packageManifest);
            });

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

        public Target InstallPlatform => _ => _
            .Executes(async () =>
            {
                var packageManifest = PackageManager.FromFile(PackageManifestPath);
                await InstallPlatformAsync(packageManifest.PlatformVersion);
            });

        private async Task InstallPlatformAsync(string platformVersion)
        {
            if (NeedToInstallPlatform(platformVersion))
            {
                Logger.Info($"Installing platform {platformVersion}");
                var platformRelease = await GithubManager.GetPlatformRelease(platformVersion);
                var platformAssetUrl = platformRelease.Assets.FirstOrDefault()?.BrowserDownloadUrl;
                var platformZip = TemporaryDirectory / "platform.zip";

                if (string.IsNullOrEmpty(platformAssetUrl))
                {
                    ControlFlow.Fail($"No platform's assets found with tag {platformVersion}");
                }

                await HttpTasks.HttpDownloadFileAsync(platformAssetUrl, platformZip);

                // backup appsettings.json if exists
                var tempFile = string.Empty;
                if (File.Exists(AppsettingsPath))
                {
                    tempFile = Path.GetTempFileName();
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
                }
            }
        }

        private string GetDiscoveryPath()
        {
            var configuration = AppSettings.GetConfiguration(RootDirectory, AppsettingsPath);
            return DiscoveryPath.EmptyToNull() ?? configuration.GetModulesDiscoveryPath();
        }

        private bool NeedToInstallPlatform(string version)
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
            .Executes(async () =>
            {
                var packageManifest = PackageManager.FromFile(PackageManifestPath);
                List<ModuleSource> moduleSources = PackageManager.GetModuleSources(packageManifest).Where(s => s is not GithubReleases).ToList();
                var githubReleases = PackageManager.GetGithubModulesSource(packageManifest);
                var discoveryPath = GetDiscoveryPath();
                var localModuleCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                var externalModuleCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubReleases.ModuleSources);
                var moduleInstaller = ModuleInstallerFacade.GetModuleInstaller(discoveryPath, ProbingPath, GitHubToken, githubReleases.ModuleSources);
                var modulesToInstall = new List<ManifestModuleInfo>();
                var alreadyInstalledModules = localModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.IsInstalled);

                foreach (var module in githubReleases.Modules)
                {
                    var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.Id == module.Id);

                    if (externalModule == null)
                    {
                        ControlFlow.Fail($"No module {module.Id} found");
                        return;
                    }

                    if (alreadyInstalledModules.Any(installedModule => installedModule.ModuleName == module.Id && installedModule.Version.ToString() == module.Version))
                    {
                        continue;
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
                    modulesToInstall.Add(moduleInfo);
                }

                var progress = new Progress<ProgressMessage>(m => Logger.Info(m.Message));

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
                moduleInstaller.Install(modulesToInstall, progress);

                foreach(var moduleSource in moduleSources)
                {
                    var installer = GetModuleInstaller(moduleSource);
                    await installer.Install(moduleSource);
                }
                AbsolutePath absoluteDiscoveryPath = (AbsolutePath)Path.GetFullPath(discoveryPath);
                var zipFiles = absoluteDiscoveryPath.GlobFiles("**/*.zip");
                zipFiles.ForEach(f => FileSystemTasks.DeleteFile(f));
                localModuleCatalog.Reload();
            });

        private IModulesInstaller GetModuleInstaller(ModuleSource moduleSource) => moduleSource switch
        {
            AzurePipelineArtifacts s => new AzurePipelineArtifactsModuleInstaller(AzureToken, GetDiscoveryPath()),
            AzureUniversalPackages s => new AzureUniversalPackagesModuleInstaller(AzureToken, GetDiscoveryPath()),
            _ => throw new NotImplementedException("Unknown module source"),
        };

        public Target Uninstall => _ => _
            .Executes(() =>
            {
                var discoveryPath = GetDiscoveryPath();
                var packageManifest = PackageManager.FromFile(PackageManifestPath);
                var localModulesCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
                var githubModules = PackageManager.GetGithubModules(packageManifest);
                FileSystemTasks.DeleteDirectory(ProbingPath);
                Module.ForEach(m => FileSystemTasks.DeleteDirectory(Path.Combine(discoveryPath, m)));
                githubModules.RemoveAll(m => Module.Contains(m.Id));
                PackageManager.ToFile(packageManifest);
                localModulesCatalog.Reload();
            });

        public Target Update => _ => _
            .Triggers(InstallPlatform, InstallModules)
            .Executes(async () =>
            {
                var packageManifest = await OpenOrCreateManifets(PackageManifestPath);
                var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                var githubModules = PackageManager.GetGithubModules(packageManifest);
                var githubModuleManifests = PackageManager.GetGithubModuleManifests(packageManifest);
                packageManifest.PlatformVersion = platformRelease.TagName;

                if (!PlatformParameter)
                {
                    var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
                    var externalModuleCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, githubModuleManifests);

                    foreach (var module in githubModules)
                    {
                        var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.Id == module.Id).FirstOrDefault(m => m.Ref.Contains("github.com"));

                        if (externalModule == null)
                        {
                            var errorMessage = $"No module {module.Id} found";
                            ControlFlow.Fail(errorMessage);
                            throw new ArgumentNullException(errorMessage); // for sonarQube
                        }

                        module.Version = externalModule.Version.ToString();
                    }
                }

                PackageManager.ToFile(packageManifest);
            });

        private async Task<ManifestBase> OpenOrCreateManifets(string packageManifestPath)
        {
            ManifestBase packageManifest;
            var platformWebDllPath = Path.Combine(Directory.GetParent(packageManifestPath).FullName, "VirtoCommerce.Platform.Web.dll");
            if (!File.Exists(packageManifestPath) && File.Exists(platformWebDllPath))
            {
                var discoveryAbsolutePath = Path.GetFullPath(GetDiscoveryPath());
                packageManifest = CreateManifestFromEnvironment(RootDirectory, (AbsolutePath)discoveryAbsolutePath);
            }
            else if (!File.Exists(packageManifestPath))
            {
                Logger.Info("vc-package.json is not exists.");
                Logger.Info("Looking for the platform release");
                var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName);
            }
            else
            {
                packageManifest = PackageManager.FromFile(PackageManifestPath);
            }
            return packageManifest;
        }

        private static ManifestBase CreateManifestFromEnvironment(AbsolutePath platformPath, AbsolutePath discoveryPath)
        {
            var platformWebDllPath = platformPath / "VirtoCommerce.Platform.Web.dll";
            if (!FileSystemTasks.FileExists(platformWebDllPath))
            {
                ControlFlow.Fail($"{platformWebDllPath} can't be found!");
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
