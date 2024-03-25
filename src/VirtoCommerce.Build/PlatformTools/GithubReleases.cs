using System.Collections.Generic;
using PlatformTools.Modules;

namespace VirtoCommerce.Build.PlatformTools
{
    public class GithubReleases: ModuleSource
    {
        public override string Name { get; set; } = "GithubReleases";
        public List<string> ModuleSources { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}

