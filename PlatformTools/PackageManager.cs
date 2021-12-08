using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.IO;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    internal static class PackageManager
    {
        private static readonly string _defaultModuleManifest = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json";

        public static ManifestBase CreatePackageManifest(string platformVersion, string platformAssetUrl)
        {
            //var manifest = new PackageManifest
            //{
            //    PlatformVersion = platformVersion,
            //    PlatformAssetUrl = platformAssetUrl,
            //    Modules = new List<ModuleItem>(),
            //    ModuleSources = new List<string>(),
            //};

            //manifest.ModuleSources.Add(_defaultModuleManifest);
            //var source = new GithubReleases { ModuleSources = new List<string>().Add(_defaultModuleManifest) }
            var manifest = new MixedPackageManifest
            {
                ManifestVersion = "2.0",
                PlatformVersion = platformVersion,
                Sources = new List<ModuleSource>
                {
                    new GithubReleases
                    {
                        ModuleSources = new List<string>
                        {
                            _defaultModuleManifest
                        }
                    }
                }
            };
            return manifest;
        }

        public static ManifestBase CreatePackageManifest(string platformVersion)
        {
            return CreatePackageManifest(platformVersion, "");
        }

        public static PackageManifest UpdatePlatform(PackageManifest manifest, string newVersion)
        {
            manifest.PlatformVersion = newVersion;
            return manifest;
        }

        public static PackageManifest AddModule(PackageManifest manifest, ModuleItem module)
        {
            manifest.Modules.Add(module);
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
                result = SerializationTasks.JsonDeserializeFromFile<PackageManifest>(path);
            else
                result = SerializationTasks.JsonDeserializeFromFile<MixedPackageManifest>(path);
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
