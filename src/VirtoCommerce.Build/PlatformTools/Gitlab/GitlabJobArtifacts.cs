using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabJobArtifacts : ModuleSource
{
    public override string Name { get; set; } = "GitlabJobArtifacts";
    public string Owner { get; set; }
    public List<GitlabJobArtifactsModuleItem> Modules { get; set; }
}
