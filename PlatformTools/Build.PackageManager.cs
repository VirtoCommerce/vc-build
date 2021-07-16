using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using PlatformTools;
using VirtoCommerce.Platform.Core.Modularity;

internal partial class Build
{
    [Parameter("Platform or Module version to install", Name = "Version")]
    public static string VersionToInstall;

    [Parameter("vc-package.json path")]
    public static string PackageManifestPath = "./vc-package.json";

    [Parameter("Install params (install -module VirtoCommerce.Core:1.2.3)")]
    public static string[] Module;

    [Parameter("Skip dependency solving")]
    public static bool SkipDependencySolving;

    [Parameter("Install the platform", Name = "Platform")]
    public static bool InstallPlatformParam;

    private Target Init => _ => _
        .Executes(async () =>
        {
            var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
            var packageManifest = PackageManager.CreatePackageManifest(platformRelease.TagName, platformRelease.Assets.First().BrowserDownloadUrl);
            PackageManager.ToFile(packageManifest, PackageManifestPath);
        });

    private Target Install => _ => _
        .Triggers(InstallPlatform, InstallModules)
        .Executes(async () =>
        {
            PackageManifest packageManifest;

            if (!File.Exists(PackageManifestPath))
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

            var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, packageManifest.ModuleSources);

            if (Module?.Length > 0 && !InstallPlatformParam)
            {
                foreach (var module in ParseModuleParameter(Module))
                {
                    var externalModule = externalCatalog.Items.OfType<ManifestModuleInfo>().Where(m => string.Compare(m.Id, module.Id, StringComparison.InvariantCultureIgnoreCase) == 0).FirstOrDefault();
                    module.Id = externalModule.Id;

                    if (string.IsNullOrEmpty(module.Version))
                    {
                        module.Version = externalModule.Version.ToString();
                    }

                    var existedModule = packageManifest.Modules.Where(m => m.Id == module.Id).FirstOrDefault();

                    if (existedModule == null)
                    {
                        Logger.Info($"Add {module.Id}:{module.Version}");
                        packageManifest.Modules.Add(module);
                    }
                    else
                    {
                        if (new Version(existedModule.Version) > new Version(module.Version))
                        {
                            Logger.Error($"{module.Id}: Module downgrading isn't supported");
                            continue;
                        }

                        Logger.Info($"Change version: {existedModule.Version} -> {module.Version}");
                        existedModule.Version = module.Version;
                    }
                }
            }
            else if (!InstallPlatformParam && !packageManifest.Modules.Any())
            {
                Logger.Info("Add group: commerce");
                var commerce = externalCatalog.Modules.OfType<ManifestModuleInfo>().Where(m => m.Groups.Contains("commerce")).Select(m => new ModuleItem(m.ModuleName, m.Version.ToString()));
                packageManifest.Modules.AddRange(commerce);
            }
            else if (InstallPlatformParam)
            {
                var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
                packageManifest.PlatformVersion = platformRelease.TagName;
                packageManifest.PlatformAssetUrl = platformRelease.Assets.FirstOrDefault().BrowserDownloadUrl;
            }

            PackageManager.ToFile(packageManifest);
        });

    private IEnumerable<ModuleItem> ParseModuleParameter(string[] Module)
    {
        foreach (var module in Module)
        {
            string moduleId;
            var moduleVersion = string.Empty;

            if (module.Contains(":"))
            {
                var splitedModule = module.Split(":");
                moduleId = splitedModule.First();
                moduleVersion = splitedModule.Last();
            }
            else if (Module.Length == 1 && !string.IsNullOrEmpty(VersionToInstall))
            {
                moduleId = module;
                moduleVersion = VersionToInstall;
            }
            else
            {
                moduleId = module;
            }

            yield return new ModuleItem(moduleId, moduleVersion);
        }
    }

    private Target InstallPlatform => _ => _
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
            var platformAssetUrl = platformRelease.Assets.FirstOrDefault().BrowserDownloadUrl;
            var platformZip = TemporaryDirectory / "platform.zip";

            if (string.IsNullOrEmpty(platformAssetUrl))
            {
                ControlFlow.Fail($"No platform's assets found with tag {platformVersion}");
            }

            await HttpTasks.HttpDownloadFileAsync(platformAssetUrl, platformZip);
            CompressionTasks.Uncompress(platformZip, RootDirectory);
        }
    }

    private string GetDiscoveryPath()
    {
        var configuration = AppSettings.GetConfiguration(RootDirectory, AppsettingsPath);
        return string.IsNullOrEmpty(DiscoveryPath) ? configuration.GetModulesDiscoveryPath() : DiscoveryPath;
    }

    private bool NeedToInstallPlatform(string version)
    {
        var result = true;
        var platformWeb = RootDirectory / "VirtoCommerce.Platform.Web.dll";
        var newVersion = new Version(version);

        if (File.Exists(platformWeb))
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(platformWeb);
            var currentProductVersion = Version.Parse(versionInfo.ProductVersion);

            if (newVersion <= currentProductVersion)
            {
                result = false;
            }
        }

        return result;
    }

    private Target InstallModules => _ => _
        .After(InstallPlatform)
        .Executes(() =>
        {
            var packageManifest = PackageManager.FromFile(PackageManifestPath);
            var discoveryPath = GetDiscoveryPath();
            var localModuleCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
            var externalModuleCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, packageManifest.ModuleSources);
            var moduleInstaller = ModuleInstallerFacade.GetModuleInstaller(discoveryPath, ProbingPath, GitHubToken, packageManifest.ModuleSources);
            var modulesToInstall = new List<ManifestModuleInfo>();

            foreach (var moduleInstall in packageManifest.Modules)
            {
                var externalModule = externalModuleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.ModuleName == moduleInstall.Id);

                if (externalModule == null)
                {
                    ControlFlow.Fail($"No module {moduleInstall.Id} found");
                }

                if (externalModule.IsInstalled && externalModule.Version.ToString() == moduleInstall.Version)
                {
                    continue;
                }

                var currentModule = new ModuleManifest
                {
                    Id = moduleInstall.Id,
                    Version = moduleInstall.Version,
                    Dependencies = SkipDependencySolving
                        ? null
                        : externalModule.Dependencies.Select(d => new ManifestDependency
                        {
                            Id = d.Id,
                            Version = d.Version.ToString(),
                        }).ToArray(),
                    PackageUrl = externalModule.Ref.Replace(externalModule.Version.ToString(), moduleInstall.Version),
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

                var tmp = new ManifestModuleInfo().LoadFromManifest(currentModule);
                modulesToInstall.Add(tmp);
            }

            var progress = new Progress<ProgressMessage>(m => Logger.Info(m.Message));

            if (!SkipDependencySolving)
            {
                var missingModules = externalModuleCatalog.CompleteListWithDependencies(modulesToInstall.Where(m => !m.IsInstalled).OfType<ModuleInfo>()).Except(modulesToInstall).OfType<ManifestModuleInfo>().ToList();
                modulesToInstall.AddRange(missingModules);
            }

            moduleInstaller.Install(modulesToInstall.Where(m => !m.IsInstalled), progress);
            localModuleCatalog.Reload();
        });

    private Target Uninstall => _ => _
        .Executes(() =>
        {
            var discoveryPath = GetDiscoveryPath();
            var packageManifest = PackageManager.FromFile(PackageManifestPath);
            var localModulesCatalog = LocalModuleCatalog.GetCatalog(discoveryPath, ProbingPath);
            var externalModuleCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModulesCatalog, packageManifest.ModuleSources);
            FileSystemTasks.DeleteDirectory(ProbingPath);
            Module.ForEach(m => FileSystemTasks.DeleteDirectory(Path.Combine(discoveryPath, m)));
            packageManifest.Modules.RemoveAll(m => Module.Contains(m.Id));
            PackageManager.ToFile(packageManifest);
            localModulesCatalog.Load();
        });

    private Target Update => _ => _
        .Triggers(InstallPlatform, InstallModules)
        .Executes(async () =>
        {
            var packageManifest = PackageManager.FromFile(PackageManifestPath);
            var platformRelease = await GithubManager.GetPlatformRelease(GitHubToken, VersionToInstall);
            packageManifest.PlatformVersion = platformRelease.TagName;
            packageManifest.PlatformAssetUrl = platformRelease.Assets.First().BrowserDownloadUrl;
            var localModuleCatalog = LocalModuleCatalog.GetCatalog(GetDiscoveryPath(), ProbingPath);
            var externalCatalog = ExtModuleCatalog.GetCatalog(GitHubToken, localModuleCatalog, packageManifest.ModuleSources);

            foreach (var module in packageManifest.Modules)
            {
                var moduleInfo = externalCatalog.Items.OfType<ManifestModuleInfo>().Where(m => m.Id == module.Id).FirstOrDefault(m => m.Ref.Contains("github.com"));

                if (moduleInfo == null)
                {
                    ControlFlow.Fail($"No module {module.Id} found");
                }

                module.Version = moduleInfo.Version.ToString();
            }

            PackageManager.ToFile(packageManifest);
        });
}
