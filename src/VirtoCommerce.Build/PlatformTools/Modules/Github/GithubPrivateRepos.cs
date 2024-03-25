using System.Collections.Generic;
using PlatformTools.Modules;

namespace PlatformTools.Modules.Github
{
    internal class GithubPrivateRepos : ModuleSource
    {
        public override string Name { get; set; } = "GithubPrivateRepos";
        public string Owner { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
