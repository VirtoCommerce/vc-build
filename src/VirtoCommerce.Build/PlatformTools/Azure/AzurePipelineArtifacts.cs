using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzurePipelineArtifacts : ModuleSource
    {
        public string Organization { get; set; }
        public string Project { get; set; }
        public List<AzureModuleItem> Modules { get; set; }
        public override string Name { get; set; } = nameof(AzurePipelineArtifacts);
    }
}

