using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzurePipelineArtifactsModuleInstaller : IModulesInstaller
    {
        private readonly string _token;
        private readonly string _discoveryPath;

        public AzurePipelineArtifactsModuleInstaller(string token, string discoveryPath)
        {
            this._token = token;
            this._discoveryPath = discoveryPath;
        }
        public Task Install(ModuleSource source)
        {
            return InnerInstall((AzurePipelineArtifacts)source);
        }

        protected async Task InnerInstall(AzurePipelineArtifacts artifacts)
        {
            var azureClient = new AzureDevClient(artifacts.Organization, _token);
            var clientOptions = ExtModuleCatalog.GetOptions(_token, new List<string>() { "https://virtocommerce.com" });
            var downloadClient = new AzurePipelineArtifactsClient(clientOptions);
            foreach (var module in artifacts.Modules)
            {
                Log.Information($"Installing {module.Id}");
                var moduleDestination = Path.Join(_discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                var zipName = $"{module.Id}.zip";
                var zipDestination = Path.Join(moduleDestination, zipName);
                var artifactUrl = await azureClient.GetArtifactUrl(Guid.Parse(artifacts.Project), module.Branch, module.Definition);
                Log.Information($"Downloading {artifactUrl}");
                using (var stream = downloadClient.OpenRead(artifactUrl))
                {
                    using (var output = File.OpenWrite(zipDestination))
                    {
                        await stream.CopyToAsync(output);
                    }
                }
                Log.Information($"Extracting {zipName}");
                ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
                Log.Information($"Successfully installed {module.Id}");
            }
        }
    }
}

