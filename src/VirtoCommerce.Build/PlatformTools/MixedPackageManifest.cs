using System.Collections.Generic;

namespace VirtoCommerce.Build.PlatformTools
{
    internal class MixedPackageManifest: ManifestBase
    {
        public string PlatformAssetUrl { get; set; } = null;
        public List<ModuleSource> Sources { get; set; }

        public MixedPackageManifest()
        {
            Sources = new List<ModuleSource>();
        }
    }
}

