using System.Collections.Generic;
using PlatformTools.Modules;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools
{
    public class PackageManifest: ManifestBase
    {
        public string PlatformAssetUrl { get; set; }
        public List<string> ModuleSources { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
