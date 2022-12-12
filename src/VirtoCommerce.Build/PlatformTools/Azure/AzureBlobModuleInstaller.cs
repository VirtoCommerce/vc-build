using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build.PlatformTools;
using AzureBlobs = Azure.Storage.Blobs;

namespace PlatformTools.Azure
{
    internal class AzureBlobModuleInstaller : IModulesInstaller
    {
        private readonly string _token;
        private readonly string _discoveryPath;

        public AzureBlobModuleInstaller(string token, string discoveryPath)
        {
            _token = token;
            _discoveryPath = discoveryPath;
        }
        public Task Install(ModuleSource source)
        {
            return InnerInstall((AzureBlob)source, _discoveryPath);
        }

        protected Task InnerInstall(AzureBlob source, string destination)
        {
            var blobClientOptions = new AzureBlobs.BlobClientOptions();
            var blobServiceClient = new AzureBlobs.BlobServiceClient(new Uri($"{source.ServiceUri}?{_token}"), blobClientOptions);
            var containerClient = blobServiceClient.GetBlobContainerClient(source.Container);
            foreach (var module in source.Modules)
            {
                Log.Information($"Installing {module.BlobName}");
                var zipName = $"{module.BlobName}.zip";
                var zipPath = Path.Join(destination, zipName);
                var moduleDestination = Path.Join(destination, module.BlobName);
                Log.Information($"Downloading Blob {module.BlobName}");
                var blobClient = containerClient.GetBlobClient(module.BlobName);
                blobClient.DownloadTo(zipPath);
                Log.Information($"Extracting Blob {module.BlobName}");
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
                Log.Information($"Successfully installed {module.BlobName}");
            }
            return Task.CompletedTask;
        }
    }
}
