using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzureBlob : ModuleSource
    {
        public override string Name { get; set; } = nameof(AzureBlob);
        public List<AzureBlobModuleItem> Modules { get; set; }
        public string Container { get; set; }
        public string ServiceUri { get; set; }
    }
}
