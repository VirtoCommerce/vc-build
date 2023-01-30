using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabJobArtifactsModuleInstaller : IModulesInstaller
{
    private readonly GitLabClient _client;
    private readonly string _discoveryPath;
    private readonly string _server;
    private readonly string _token;

    public GitlabJobArtifactsModuleInstaller(string server, string token, string discoveryPath)
    {
        _server = server;
        _token = token;
        _discoveryPath = discoveryPath;
        _client = new GitLabClient(_token, server);
    }

    public Task Install(ModuleSource source)
    {
        return InnerInstall(source as GitlabJobArtifacts);
    }

    protected virtual async Task InnerInstall(GitlabJobArtifacts source)
    {
        foreach (var module in source.Modules)
        {
            var moduleDestination = Path.Join(_discoveryPath, module.Id);
            Directory.CreateDirectory(moduleDestination);
            FileSystemTasks.EnsureCleanDirectory(moduleDestination);
            Log.Information($"Downloading {module.Id}");
            var artifactZipPath = await _client.DownloadArtifact(module.Id, module.JobId, moduleDestination);
            Log.Information($"Extracting {module.Id}");
            ZipFile.ExtractToDirectory(artifactZipPath, moduleDestination);
            Log.Information($"Successfully installed {module.Id}");
        }
    }
}
