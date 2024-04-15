using PlatformTools.Modules;

namespace PlatformTools.Modules.Azure
{
    internal class AzureBlobModuleItem : ModuleItem
    {
        public AzureBlobModuleItem(string id, string version) : base(id, version)
        {

        }
        public string BlobName { get; set; }
    }
}
