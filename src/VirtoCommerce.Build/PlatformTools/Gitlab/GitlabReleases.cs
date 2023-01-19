using System.Collections.Generic;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabReleases: ModuleSource
{
    public override string Name { get; set; } = "GitlabReleases";
    public string Owner { get; set; }
    public List<GitlabReleasesModuleItem> Modules { get; set; }
}
