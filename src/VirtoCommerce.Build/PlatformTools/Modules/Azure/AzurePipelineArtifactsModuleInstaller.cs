using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules.Azure
{
    internal class AzurePipelineArtifactsModuleInstaller : ModuleInstallerBase
    {
        private readonly string _token;
        private readonly string _discoveryPath;

        public AzurePipelineArtifactsModuleInstaller(string token, string discoveryPath)
        {
            _token = token;
            _discoveryPath = discoveryPath;
        }

        protected override async Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var artifacts = (AzurePipelineArtifacts)source;
            var azureClient = new AzureDevClient(artifacts.Organization, _token);
            var clientOptions = ExtModuleCatalog.GetOptions(_token, new List<string>() { "https://virtocommerce.com" });
            var downloadClient = new AzurePipelineArtifactsClient(clientOptions);
            foreach (var module in artifacts.Modules)
            {
                progress.ReportInfo($"Installing {module.Id}");
                var moduleDestination = Path.Join(_discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                var zipName = $"{module.Id}.zip";
                var zipDestination = Path.Join(moduleDestination, zipName);
                var artifactUrl = await azureClient.GetArtifactUrl(Guid.Parse(artifacts.Project), module.Branch, module.Definition);
                progress.ReportInfo($"Downloading {artifactUrl}");
                using (var stream = downloadClient.OpenRead(artifactUrl))
                {
                    using (var output = File.OpenWrite(zipDestination))
                    {
                        await stream.CopyToAsync(output);
                    }
                }
                progress.ReportInfo($"Extracting {zipName}");
                ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
                progress.ReportInfo($"Successfully installed {module.Id}");
            }
        }
    }
}

