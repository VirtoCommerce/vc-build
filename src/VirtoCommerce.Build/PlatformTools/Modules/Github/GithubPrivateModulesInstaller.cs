using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common.IO;
using Octokit;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules.Github
{
    internal class GithubPrivateModulesInstaller : ModuleInstallerBase
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

        protected override async Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var githubPrivateRepos = (GithubPrivateRepos)source;
            foreach (var module in githubPrivateRepos.Modules)
            {
                var moduleDestination = AbsolutePath.Create(Path.Join(_discoveryPath, module.Id));
                progress.ReportInfo($"Installing {module.Id}");
                moduleDestination.CreateOrCleanDirectory();
                var zipName = $"{module.Id}.zip";
                var zipDestination = Path.Join(moduleDestination, zipName).ToAbsolutePath();
                var release = await _client.Repository.Release.Get(githubPrivateRepos.Owner, module.Id, module.Version);
                if (release == null)
                {
                    progress.ReportError($"{module.Id}:{module.Version} is not found");
                    continue;
                }

                if (release.Assets.Count == 0)
                {
                    progress.ReportError($"{module.Id}:{module.Version} has no assets");
                    continue;
                }
                var asset = release.Assets[0];

                progress.ReportInfo($"Downloading {module.Id}");
                await HttpTasks.HttpDownloadFileAsync(asset.Url, zipDestination, clientConfigurator: c =>
                {
                    c.DefaultRequestHeaders.Add("User-Agent", "VirtoCommerce.Build");
                    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    return c;
                });
                progress.ReportInfo($"Extracting {module.Id}");
                ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
                progress.ReportInfo($"Successfully installed {module.Id}");
            }
        }
    }
}
