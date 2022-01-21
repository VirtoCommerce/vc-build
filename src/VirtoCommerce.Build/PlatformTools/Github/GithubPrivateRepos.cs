using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformTools;
using VirtoCommerce.Build.PlatformTools;

namespace VirtoCommerce.Build.PlatformTools.Github
{
    internal class GithubPrivateRepos : ModuleSource
    {
        public override string Name { get; set; } = "GithubPrivateRepos";
        public string Owner { get; set; }
        public List<ModuleItem> Modules { get; set; }
    }
}
