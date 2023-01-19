namespace PlatformTools.Gitlab;

internal class GitlabReleasesModuleItem: ModuleItem
{
    public GitlabReleasesModuleItem(string id, string version) : base(id, version)
    {
    }

    public string AssetName { get; set; }
}
