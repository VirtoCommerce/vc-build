using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Nuke.Common.Utilities;
using PlatformTools.Modules;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    public static class PackageManager
    {
        private const string _defaultModuleManifest = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json";

        public static ManifestBase CreatePackageManifest(string platformVersion, string platformAssetUrl)
        {
            var manifest = new MixedPackageManifest
            {
                ManifestVersion = "2.0",
                PlatformVersion = platformVersion,
                PlatformAssetUrl = string.IsNullOrWhiteSpace(platformAssetUrl) ? null : platformAssetUrl,
                Sources = new List<ModuleSource>
                {
                    new GithubReleases
                    {
                        ModuleSources = new List<string>
                        {
                            _defaultModuleManifest
                        },
                        Modules = new List<ModuleItem>()
                    }
                }
            };
            return manifest;
        }

        public static ManifestBase CreatePackageManifest(string platformVersion)
        {
            return CreatePackageManifest(platformVersion, "");
        }

        public static MixedPackageManifest UpdatePlatform(MixedPackageManifest manifest, string newVersion)
        {
            manifest.PlatformVersion = newVersion;
            return manifest;
        }

        public static void ToFile(ManifestBase manifest, string path = "./vc-package.json")
        {
            path.ToAbsolutePath().WriteJson(manifest);
        }

        public static ManifestBase FromFile(string path = "./vc-package.json")
        {
            var absolutePath = path.ToAbsolutePath();
            var baseManifest = absolutePath.ReadJson<ManifestBase>();
            ManifestBase result;
            if (string.IsNullOrEmpty(baseManifest.ManifestVersion) || new Version(baseManifest.ManifestVersion) < new Version("2.0"))
            {
                result = absolutePath.ReadJson<PackageManifest>();
            }
            else
            {
                result = absolutePath.ReadJson<MixedPackageManifest>();
            }

            return result;
        }

        public static List<ModuleSource> GetModuleSources(ManifestBase manifest)
        {
            switch (manifest)
            {
                case PackageManifest pm:
                    var githubReleases = new GithubReleases
                    {
                        Modules = pm.Modules,
                        ModuleSources = pm.ModuleSources
                    };
                    return new List<ModuleSource>() { githubReleases };
                case MixedPackageManifest mm:
                    return mm.Sources;
                default:
                    return new List<ModuleSource>();
            }
        }

        public static GithubReleases GetGithubModulesSource(ManifestBase manifest)
        {
            var sources = GetModuleSources(manifest);
            return (GithubReleases)sources.Find(s => s.Name == nameof(GithubReleases));
        }

        public static List<string> GetGithubModuleManifests(ManifestBase manifest)
        {
            switch (manifest)
            {
                case PackageManifest pm:
                    return pm.ModuleSources;
                case MixedPackageManifest mm:
                    var githubReleasesSource = mm.Sources.Where(s => s.Name == nameof(GithubReleases)).OfType<GithubReleases>().FirstOrDefault();
                    return githubReleasesSource?.ModuleSources ?? new List<string>();
                default:
                    return new List<string>();
            }
        }

        public static List<ModuleItem> GetGithubModules(ManifestBase manifest)
        {
            switch (manifest)
            {
                case PackageManifest pm:
                    return pm.Modules;
                case MixedPackageManifest mm:
                    var githubReleasesSource = mm.Sources.Where(s => s.Name == nameof(GithubReleases)).OfType<GithubReleases>().FirstOrDefault();
                    return githubReleasesSource?.Modules ?? new List<ModuleItem>();
                default:
                    return new List<ModuleItem>();
            }
        }
    }
}
