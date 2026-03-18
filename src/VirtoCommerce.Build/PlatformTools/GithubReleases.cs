using System.Collections.Generic;
using PlatformTools.Modules;

namespace VirtoCommerce.Build.PlatformTools
{
    public class GithubReleases : ModuleSource
    {
        public const string SourceName = nameof(GithubReleases);
        public override string Name { get; set; } = SourceName;
        public List<string> ModuleSources { get; set; } = [];
        public List<ModuleItem> Modules { get; set; } = [];
    }
}

