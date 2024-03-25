using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Nuke.Common.IO;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules.Gitlab;

internal class GitlabJobArtifactsModuleInstaller : ModuleInstallerBase
{
    private readonly GitLabClient _client;
    private readonly string _discoveryPath;

    public GitlabJobArtifactsModuleInstaller(string server, string token, string discoveryPath)
    {
        _discoveryPath = discoveryPath;
        _client = new GitLabClient(token, server);
    }

    protected override async Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
    {
        var gitlabJobArtifacts = (GitlabJobArtifacts)source;
        foreach (var module in gitlabJobArtifacts.Modules)
        {
            var moduleDestination = AbsolutePath.Create(Path.Join(_discoveryPath, module.Id));
            moduleDestination.CreateOrCleanDirectory();
            progress.ReportInfo($"Downloading {module.Id}");
            var artifactZipPath = await _client.DownloadArtifact(module.Id, module.JobId, module.ArtifactName, moduleDestination);
            progress.ReportInfo($"Extracting {module.Id}");
            ZipFile.ExtractToDirectory(artifactZipPath, moduleDestination);
            progress.ReportInfo($"Successfully installed {module.Id}");
        }
    }
}
