using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzureUniversalPackages : ModuleSource
    {
        public override string Name { get; set; } = "AzureUniversalPackages";
        public string Organization { get; set; }
        public string Project { get; set; }
        public string Feed { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
