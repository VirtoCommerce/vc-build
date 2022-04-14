using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Octokit;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Github
{
    internal class GithubPrivateModulesInstaller : IModulesInstaller
    {

        private readonly string _token;
        private readonly string _discoveryPath;
        private readonly GitHubClient _client;
        public GithubPrivateModulesInstaller(string token, string discoveryPath)
        {
            _token = token;
            _discoveryPath = discoveryPath;
            _client = new GitHubClient(new Octokit.ProductHeaderValue("vc-build"));
            _client.Credentials = new Credentials(token);
        }
        public Task Install(ModuleSource source)
        {
            return InnerInstall((GithubPrivateRepos)source);
        }

        protected async Task InnerInstall(GithubPrivateRepos source)
        {
            foreach (var module in source.Modules)
            {
                var moduleDestination = Path.Join(_discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                FileSystemTasks.EnsureCleanDirectory(moduleDestination);
                var zipName = $"{module.Id}.zip";
                var zipDestination = Path.Join(moduleDestination, zipName);
                var release = await _client.Repository.Release.Get(source.Owner, module.Id, module.Version);
                if (release == null)
                {
                    Log.Error($"{module.Id}:{module.Version} is not found");
                    continue;
                }
                var asset = release.Assets.FirstOrDefault();
                if (asset == null)
                {
                    Log.Error($"{module.Id}:{module.Version} has no assets");
                    continue;
                }
                Log.Information($"Downloading {module.Id}");
                await HttpTasks.HttpDownloadFileAsync(asset.Url, zipDestination, clientConfigurator: c =>
                {
                    c.DefaultRequestHeaders.Add("User-Agent", "VirtoCommerce.Build");
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    return c;
                });
                Log.Information($"Extractiong {module.Id}");
                ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
            }
        }
    }
}
