using System.Collections.Generic;

namespace PlatformTools.Modules.Github
{
    internal class GithubPrivateRepos : ModuleSource
    {
        public const string SourceName = nameof(GithubPrivateRepos);
        public override string Name { get; set; } = SourceName;
        public string Owner { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
