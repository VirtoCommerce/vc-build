using PlatformTools.Modules;

namespace PlatformTools.Modules.Github
{
    internal class GithubPrivateModuleItem : ModuleItem
    {
        public GithubPrivateModuleItem(string id, string version) : base(id, version)
        {
        }
        public string Repo { get; set; }
        public string Tag { get; set; }
    }
}
