using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureBlobs = Azure.Storage.Blobs;
using VirtoCommerce.Build.PlatformTools;
using System.IO;
using System.IO.Compression;

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

        protected async Task InnerInstall(AzureBlob source, string destination)
        {
            var blobClientOptions = new AzureBlobs.BlobClientOptions();
            if(!string.IsNullOrEmpty(_token))
            {
                var key = new AzureBlobs.Models.CustomerProvidedKey(_token);
                blobClientOptions.CustomerProvidedKey = key;
            }

            var blobServiceClient = new AzureBlobs.BlobServiceClient(new Uri(source.ServiceUri), blobClientOptions);
            var containerClient = blobServiceClient.GetBlobContainerClient(source.Container);
            foreach (var module in source.Modules)
            {
                var zipName = $"{module.BlobName}.zip";
                var zipPath = Path.Join(destination, zipName);
                var moduleDestination = Path.Join(destination, module.BlobName);
                containerClient.GetBlobClient(module.BlobName).DownloadTo(zipPath);
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
            }
        }
    }
}
