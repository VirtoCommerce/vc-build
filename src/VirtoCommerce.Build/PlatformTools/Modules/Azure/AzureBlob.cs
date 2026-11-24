using System.Collections.Generic;

namespace PlatformTools.Modules.Azure
{
    internal class AzureBlob : ModuleSource
    {
        public const string SourceName = nameof(AzureBlob);
        public override string Name { get; set; } = SourceName;
        public string Container { get; set; }
        public string ServiceUri { get; set; }
        public List<AzureBlobModuleItem> Modules { get; set; }
    }
}
