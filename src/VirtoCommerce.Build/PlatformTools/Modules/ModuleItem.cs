namespace PlatformTools.Modules
{
    public class ModuleItem
    {
        public ModuleItem(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; set; }
        public string Version { get; set; }
    }
}
