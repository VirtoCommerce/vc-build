using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    internal class PackageManifest: ManifestBase
    {
        public string PlatformAssetUrl { get; set; }
        public List<string> ModuleSources { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
