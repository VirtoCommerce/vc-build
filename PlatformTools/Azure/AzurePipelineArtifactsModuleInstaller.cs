using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using PlatformTools;

namespace VirtoCommerce.Build.PlatformTools.Azure
{
    internal class AzurePipelineArtifactsModuleInstaller: IModulesInstaller
    {
        readonly string token;
        readonly string discoveryPath;

        public AzurePipelineArtifactsModuleInstaller(string token, string discoveryPath)
        {
            this.token = token;
            this.discoveryPath = discoveryPath;
        }
        public Task Install(ModuleSource source)
        {
            return InnerInstall((AzurePipelineArtifacts)source);
        }

        protected async Task InnerInstall(AzurePipelineArtifacts artifacts)
        {
            var azureClient = new AzureDevClient(artifacts.Organization, token);
            var clientOptions = ExtModuleCatalog.GetOptions(token, new List<string>() { "https://virtocommerce.com" });
            var downloadClient = new AzurePipelineArtifactsClient(clientOptions);
            foreach(var module in artifacts.Modules)
            {
                var moduleDestination = Path.Join(discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                var zipName = $"{module.Id}.zip";
                var zipDestination = Path.Join(moduleDestination, zipName);
                var artifactUrl = await azureClient.GetArtifactUrl(Guid.Parse(artifacts.Project), module.Branch, module.Definition);
                using (var stream = downloadClient.OpenRead(artifactUrl))
                {
                    using(var output = File.OpenWrite(zipDestination))
                    {
                        await stream.CopyToAsync(output);
                    }
                }

                ZipFile.ExtractToDirectory(zipDestination, moduleDestination);
            }
        }
    }
}

