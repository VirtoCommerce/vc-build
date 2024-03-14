namespace PlatformTools.Modules.LocalModules;

internal class LocalModuleItem : ModuleItem
{
    public LocalModuleItem(string id, string version) : base(id, version)
    {

    }

    public string Path { get; set; }
}
