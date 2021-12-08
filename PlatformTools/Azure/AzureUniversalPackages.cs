using System;
using System.Collections.Generic;
using PlatformTools;

namespace VirtoCommerce.Build.PlatformTools.Azure
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

