using System.Threading.Tasks;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabReleasesModuleInstaller : IModulesInstaller
{
    private readonly GitLabClient _client;
    private readonly string _discoveryPath;
    private readonly string _server;
    private readonly string _token;

    public GitlabReleasesModuleInstaller(string server, string token, string discoveryPath)
    {
        _server = server;
        _token = token;
        _discoveryPath = discoveryPath;
        _client = new GitLabClient(_server, _token);
    }

    public Task Install(ModuleSource source)
    {
        //return InnerInstall(source as GitlabReleases);
        return Task.CompletedTask;
    }

    // protected async Task InnerInstall(GitlabReleases source)
    // {
    //     foreach (var module in source.Modules)
    //     {
    //     }
    // }
}
