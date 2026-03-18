using System.Collections.Generic;

namespace PlatformTools.Modules.Gitlab;

internal class GitlabJobArtifacts : ModuleSource
{
    public const string SourceName = nameof(GitlabJobArtifacts);
    public override string Name { get; set; } = SourceName;
    public string Owner { get; set; }
    public List<GitlabJobArtifactsModuleItem> Modules { get; set; }
}
