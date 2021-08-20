using System.Collections.Generic;

namespace PlatformTools
{
    internal class PackageManifest
    {
        public string PlatformVersion { get; set; }
        public string PlatformAssetUrl { get; set; }
        public List<string> ModuleSources { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
