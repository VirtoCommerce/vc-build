using System.Collections.Generic;

namespace PlatformTools.Modules.Azure
{
    internal class AzurePipelineArtifacts : ModuleSource
    {
        public string Organization { get; set; }
        public string Project { get; set; }
        public List<AzureModuleItem> Modules { get; set; }
        public const string SourceName = nameof(AzurePipelineArtifacts);
        public override string Name { get; set; } = SourceName;
    }
}

