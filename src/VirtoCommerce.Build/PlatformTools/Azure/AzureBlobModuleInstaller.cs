using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using VirtoCommerce.Build.PlatformTools;
using AzureBlobs = Azure.Storage.Blobs;

namespace PlatformTools.Azure
{
    internal class AzureBlobModuleInstaller : IModulesInstaller
    {
        readonly string _token;
        readonly string _discoveryPath;

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
                var zipName = $"{module.BlobName}.zip";
                var zipPath = Path.Join(destination, zipName);
                var moduleDestination = Path.Join(destination, module.BlobName);
                var blobClient = containerClient.GetBlobClient(module.BlobName);
                blobClient.DownloadTo(zipPath);
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
            }
            return Task.CompletedTask;
        }
    }
}
