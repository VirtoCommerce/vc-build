using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using PlatformTools.Extensions;
using PlatformTools.Modules;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    public static class PackageManager
    {
        private const string _defaultModuleManifest = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json";

        // Nuke's JSON defaults, reading values exactly as written so that unknown properties are saved back unchanged
        private static readonly JsonSerializerSettings _serializerSettings = new(JsonExtensions.DefaultSerializerSettings)
        {
            DateParseHandling = DateParseHandling.None,
            FloatParseHandling = FloatParseHandling.Decimal,
        };

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

        /// <summary>
        /// Saves the manifest, keeping the existing file's property order.
        /// The file is not rewritten when its content, as read by vc-build, is unchanged.
        /// </summary>
        /// <param name="sortModules">
        /// When the file is written, sort the modules of every source (and the top-level modules of v1 manifests) by Id, case-insensitively.
        /// Entries without Id are sorted by BlobName.
        /// </param>
        public static void ToFile(ManifestBase manifest, string path = "./vc-package.json", bool sortModules = false)
        {
            var absolutePath = path.ToAbsolutePath();
            var updatedManifest = manifest.ToJObject(JsonSerializer.Create(_serializerSettings));

            if (absolutePath.FileExists())
            {
                var existingManifest = absolutePath.ReadJson(_serializerSettings);
                if (JToken.DeepEquals(existingManifest, updatedManifest))
                {
                    return;
                }

                updatedManifest = updatedManifest.WithPropertyOrderOf(existingManifest);
            }

            if (sortModules)
            {
                SortModulesById(updatedManifest);
            }

            absolutePath.WriteJson(updatedManifest, _serializerSettings);
        }

        /// <summary>
        /// Writes the manifest as is, replacing an existing file without reading it.
        /// </summary>
        public static void OverwriteFile(ManifestBase manifest, string path = "./vc-package.json")
        {
            path.ToAbsolutePath().WriteJson(manifest, _serializerSettings);
        }

        private static void SortModulesById(JObject manifest)
        {
            // v1 manifests keep modules at the top level, v2 manifests inside each source
            var moduleLists = manifest.SelectTokens("$.Modules").Concat(manifest.SelectTokens("$.Sources[*].Modules")).OfType<JArray>();

            foreach (var modules in moduleLists)
            {
                var sortedModules = modules
                    .OrderBy(module => module.GetStringValueIgnoreCase("Id") ?? module.GetStringValueIgnoreCase("BlobName"), StringComparer.OrdinalIgnoreCase)
                    .ToList();
                modules.ReplaceAll(sortedModules);
            }
        }

        public static ManifestBase FromFile(string path = "./vc-package.json")
        {
            var absolutePath = path.ToAbsolutePath();
            var baseManifest = absolutePath.ReadJson<ManifestBase>(_serializerSettings);
            ManifestBase result;
            if (string.IsNullOrEmpty(baseManifest.ManifestVersion) || new Version(baseManifest.ManifestVersion) < new Version("2.0"))
            {
                result = absolutePath.ReadJson<PackageManifest>(_serializerSettings);
            }
            else
            {
                result = absolutePath.ReadJson<MixedPackageManifest>(_serializerSettings);
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
                    return mm.Sources ?? [];
                default:
                    return new List<ModuleSource>();
            }
        }

        public static GithubReleases GetGithubModulesSource(ManifestBase manifest)
        {
            var sources = GetModuleSources(manifest);
            var githubSource = (GithubReleases)sources.Find(s => s.Name == nameof(GithubReleases));
            return githubSource ?? new GithubReleases();
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
