using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build.PlatformTools;
using AzureBlobs = Azure.Storage.Blobs;

namespace PlatformTools.Azure
{
    internal class AzureBlobModuleInstaller : ModulesInstallerBase
    {
        private readonly string _token;
        private readonly string _destination;

        public AzureBlobModuleInstaller(string token, string destination)
        {
            _token = token;
            _destination = destination;
        }

        protected override Task InnerInstall(ModuleSource source)
        {
            var azureBlobSource = (AzureBlob)source;
            var blobClientOptions = new AzureBlobs.BlobClientOptions();
            var blobServiceClient = new AzureBlobs.BlobServiceClient(new Uri($"{azureBlobSource.ServiceUri}?{_token}"), blobClientOptions);
            var containerClient = blobServiceClient.GetBlobContainerClient(azureBlobSource.Container);
            foreach (var moduleBlobName in azureBlobSource.Modules.Select(m => m.BlobName))
            {
                Log.Information($"Installing {moduleBlobName}");
                var zipName = $"{moduleBlobName}.zip";
                var zipPath = Path.Join(_destination, zipName);
                var moduleDestination = Path.Join(_destination, moduleBlobName);
                Log.Information($"Downloading Blob {moduleBlobName}");
                var blobClient = containerClient.GetBlobClient(moduleBlobName);
                blobClient.DownloadTo(zipPath);
                Log.Information($"Extracting Blob {moduleBlobName}");
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
                Log.Information($"Successfully installed {moduleBlobName}");
            }
            return Task.CompletedTask;
        }
    }
}
