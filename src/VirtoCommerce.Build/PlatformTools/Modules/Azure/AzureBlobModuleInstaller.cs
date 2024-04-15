using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;
using AzureBlobs = Azure.Storage.Blobs;

namespace PlatformTools.Modules.Azure
{
    internal class AzureBlobModuleInstaller : ModuleInstallerBase
    {
        private readonly string _token;
        private readonly string _destination;

        public AzureBlobModuleInstaller(string token, string destination)
        {
            _token = token;
            _destination = destination;
        }

        protected override Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var azureBlobSource = (AzureBlob)source;
            var blobClientOptions = new AzureBlobs.BlobClientOptions();
            var blobServiceClient = new AzureBlobs.BlobServiceClient(new Uri($"{azureBlobSource.ServiceUri}?{_token}"), blobClientOptions);
            var containerClient = blobServiceClient.GetBlobContainerClient(azureBlobSource.Container);
            Directory.CreateDirectory(_destination);
            foreach (var moduleBlobName in azureBlobSource.Modules.Select(m => m.BlobName))
            {
                progress.ReportInfo($"Installing {moduleBlobName}");
                var zipName = moduleBlobName;
                if (!zipName.EndsWith(".zip"))
                {
                    zipName += ".zip";
                }

                var zipPath = Path.Join(_destination, zipName);
                var moduleDestination = Path.Join(_destination, moduleBlobName);
                if (moduleDestination.EndsWith(".zip"))
                {
                    moduleDestination = moduleDestination.Replace(".zip", "");
                }
                progress.ReportInfo($"Downloading Blob {moduleBlobName}");
                var blobClient = containerClient.GetBlobClient(moduleBlobName);
                blobClient.DownloadTo(zipPath);
                progress.ReportInfo($"Extracting Blob {moduleBlobName}");
                ZipFile.ExtractToDirectory(zipPath, moduleDestination, true);
                progress.ReportInfo($"Successfully installed {moduleBlobName}");
            }
            return Task.CompletedTask;
        }
    }
}
