using System.Collections.Generic;
using Nuke.Common.IO;

namespace PlatformTools
{
    internal class PackageManager
    {
        private static readonly string _defaultModuleManifest = "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json";

        public static PackageManifest CreatePackageManifest(string platformVersion, string PlatformAssetUrl)
        {
            var manifest = new PackageManifest
            {
                PlatformVersion = platformVersion,
                PlatformAssetUrl = PlatformAssetUrl,
                Modules = new List<ModuleItem>(),
                ModuleSources = new List<string>(),
            };

            manifest.ModuleSources.Add(_defaultModuleManifest);
            return manifest;
        }

        public static PackageManifest CreatePackageManifest(string platformVersion)
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

        public static void ToFile(PackageManifest manifest, string path = "./vc-package.json")
        {
            SerializationTasks.JsonSerializeToFile(manifest, path);
        }

        public static PackageManifest FromFile(string path = "./vc-package.json")
        {
            var result = SerializationTasks.JsonDeserializeFromFile<PackageManifest>(path);
            return result;
        }
    }
}
