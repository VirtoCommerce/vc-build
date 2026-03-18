using System.Collections.Generic;

namespace PlatformTools.Modules.Azure
{
    internal class AzureUniversalPackages : ModuleSource
    {
        public const string SourceName = nameof(AzureUniversalPackages);
        public override string Name { get; set; } = SourceName;
        public string Organization { get; set; }
        public string Project { get; set; }
        public string Feed { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
