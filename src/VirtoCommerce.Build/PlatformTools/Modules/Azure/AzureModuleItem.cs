using PlatformTools.Modules;

namespace PlatformTools.Modules.Azure
{
    internal class AzureModuleItem : ModuleItem
    {
        public AzureModuleItem(string id, string version) : base(id, version)
        {
        }

        public string BuildId { get; set; }
        public string Branch { get; set; }
        public string Definition { get; set; }
    }
}

