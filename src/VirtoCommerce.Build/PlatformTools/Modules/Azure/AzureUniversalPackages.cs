using System.Collections.Generic;
using PlatformTools.Modules;

namespace PlatformTools.Modules.Azure
{
    internal class AzureUniversalPackages : ModuleSource
    {
        public override string Name { get; set; } = nameof(AzureUniversalPackages);
        public string Organization { get; set; }
        public string Project { get; set; }
        public string Feed { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
