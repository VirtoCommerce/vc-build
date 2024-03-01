using PlatformTools.Modules;

namespace PlatformTools.Modules.Gitlab;

internal class GitlabJobArtifactsModuleItem : ModuleItem
{
    public GitlabJobArtifactsModuleItem(string id, string version) : base(id, version)
    {
    }

    public string JobId { get; set; }
    public string ArtifactName { get; set; }
}
