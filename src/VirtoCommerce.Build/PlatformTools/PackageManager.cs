using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.IO;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    public static class PackageManager
    {
        private static readonly string _defaultModuleManifest = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json";

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
            SerializationTasks.JsonSerializeToFile(manifest, path);
        }

        public static ManifestBase FromFile(string path = "./vc-package.json")
        {
            var baseManifest = SerializationTasks.JsonDeserializeFromFile<ManifestBase>(path);
            ManifestBase result;
            if (string.IsNullOrEmpty(baseManifest.ManifestVersion) || new Version(baseManifest.ManifestVersion) < new Version("2.0"))
            {
                result = SerializationTasks.JsonDeserializeFromFile<PackageManifest>(path);
            }
            else
            {
                result = SerializationTasks.JsonDeserializeFromFile<MixedPackageManifest>(path);
            }

            return result;
        }

        public static List<ModuleSource> GetModuleSources(ManifestBase manifest)
        {

            List<ModuleSource> result;
            switch (manifest)
            {
                case PackageManifest pm:
                    var githubReleases = new GithubReleases
                    {
                        Modules = pm.Modules,
                        ModuleSources = pm.ModuleSources
                    };
                    result = new List<ModuleSource>() { githubReleases };
                    break;
                case MixedPackageManifest mm:
                    result = mm.Sources;
                    break;
                default:
                    result = new List<ModuleSource>();
                    break;
            }
            return result;
        }

        public static GithubReleases GetGithubModulesSource(ManifestBase manifest)
        {
            var sources = GetModuleSources(manifest);
            return (GithubReleases)sources.FirstOrDefault(s => s.Name == nameof(GithubReleases));
        }

        public static List<string> GetGithubModuleManifests(ManifestBase manifest)
        {
            List<string> result;
            switch (manifest)
            {
                case PackageManifest pm:
                    result = pm.ModuleSources;
                    break;
                case MixedPackageManifest mm:
                    var githubReleasesSource = mm.Sources.Where(s => s.Name == nameof(GithubReleases)).OfType<GithubReleases>().FirstOrDefault();
                    result = githubReleasesSource?.ModuleSources ?? new List<string>();
                    break;
                default:
                    result = new List<string>();
                    break;
            }
            return result;
        }

        public static List<ModuleItem> GetGithubModules(ManifestBase manifest)
        {
            List<ModuleItem> result;
            switch (manifest)
            {
                case PackageManifest pm:
                    result = pm.Modules;
                    break;
                case MixedPackageManifest mm:
                    var githubReleasesSource = mm.Sources.Where(s => s.Name == nameof(GithubReleases)).OfType<GithubReleases>().FirstOrDefault();
                    result = githubReleasesSource?.Modules ?? new List<ModuleItem>();
                    break;
                default:
                    result = new List<ModuleItem>();
                    break;
            }
            return result;
        }
    }
}
