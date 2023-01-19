using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GitLabApiClient;
using Nuke.Common.IO;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Gitlab;

internal class GitlabJobArtifactsModuleInstaller : IModulesInstaller
{
    private readonly string _server;
    private readonly string _token;
    private readonly string _discoveryPath;
    private readonly GitLabClient _client;

    public GitlabJobArtifactsModuleInstaller(string server, string token, string discoveryPath)
    {
        _server = server;
        _token = token;
        _discoveryPath = discoveryPath;
        _client = new GitLabClient(_server, _token);
    }
    public Task Install(ModuleSource source)
    {
        return InnerInstall(source as GitlabJobArtifacts);
    }

    protected async Task InnerInstall(GitlabJobArtifacts source)
        {
            foreach (var module in source.Modules)
            {
                var jobs = await _client.Projects.GetJobsAsync(module.Id);
                var job = jobs.FirstOrDefault(j => j.Id == int.Parse(module.JobId));
                job.

                //var moduleDestination = Path.Join(_discoveryPath, module.Id);
                //Directory.CreateDirectory(moduleDestination);
                //FileSystemTasks.EnsureCleanDirectory(moduleDestination);
                //var zipName = $"{module.Id}.zip";
                //var zipDestination = Path.Join(moduleDestination, zipName);
                //var release = await _client.Repository.Release.Get(source.Owner, module.Id, module.Version);
                //if (release == null)
                //{
                //    Log.Error($"{module.Id}:{module.Version} is not found");
                //    continue;
                //}
                //var asset = release.Assets.FirstOrDefault();
                //if (asset == null)
                //{
                //    Log.Error($"{module.Id}:{module.Version} has no assets");
                //    continue;
                //}
                //Log.Information($"Downloading {module.Id}");
                //await HttpTasks.HttpDownloadFileAsync(asset.Url, zipDestination, clientConfigurator: c =>
                //{
                //    c.DefaultRequestHeaders.Add("User-Agent", "VirtoCommerce.Build");
                //    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                //    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                //    return c;
                //});
                //Log.Information($"Extracting {module.Id}");
                //ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
                //Log.Information($"Successfully installed {module.Id}");
            }
        }
}
