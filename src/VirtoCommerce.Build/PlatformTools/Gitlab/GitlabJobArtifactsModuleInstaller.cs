using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabJobArtifactsModuleInstaller : ModulesInstallerBase
{
    private readonly GitLabClient _client;
    private readonly string _discoveryPath;

    public GitlabJobArtifactsModuleInstaller(string server, string token, string discoveryPath)
    {
        _discoveryPath = discoveryPath;
        _client = new GitLabClient(token, server);
    }

    protected override async Task InnerInstall(ModuleSource source)
    {
        var gitlabJobArtifacts = (GitlabJobArtifacts) source;
        foreach (var module in gitlabJobArtifacts.Modules)
        {
            var moduleDestination = Path.Join(_discoveryPath, module.Id);
            Directory.CreateDirectory(moduleDestination);
            FileSystemTasks.EnsureCleanDirectory(moduleDestination);
            Log.Information($"Downloading {module.Id}");
            var artifactZipPath = await _client.DownloadArtifact(module.Id, module.JobId, module.ArtifactName, moduleDestination);
            Log.Information($"Extracting {module.Id}");
            ZipFile.ExtractToDirectory(artifactZipPath, moduleDestination);
            Log.Information($"Successfully installed {module.Id}");
        }
    }
}
