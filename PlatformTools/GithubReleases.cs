using System;
using System.Collections.Generic;
using PlatformTools;

namespace VirtoCommerce.Build.PlatformTools
{
    internal class GithubReleases: ModuleSource
    {
        public override string Name { get; set; } = "GithubReleases";
        public List<string> ModuleSources { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}

