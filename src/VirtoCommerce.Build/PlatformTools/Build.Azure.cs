using System;
using System.Collections.Generic;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Nuke.Common;
using System.Linq;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("Azure Blob Connection String")]
        public string AzureBlobConnectionString { get; set; }
        [Parameter("File Path", Name = "Path")]
        public string FilePath { get; set; }
        [Parameter("Number of Blobs to show")]
        public int BlobsNumber { get; set; } = 10;

        Target UploadBlob => _ => _
        .Executes(() =>
        {
            var containerClient = new BlobContainerClient(new Uri(AzureBlobConnectionString));
            var fileName = Path.GetFileName(FilePath);
            var blobClient = containerClient.GetBlobClient(fileName);
            blobClient.Upload(FilePath);
        });

        Target LatestBlobs => _ => _
        .Executes(() =>
        {
            var containerClient = new BlobContainerClient(new Uri(AzureBlobConnectionString));
            var blobs = new List<BlobItem>();
            foreach (BlobItem item in containerClient.GetBlobs())
            {
                blobs.Add(item);
            }
            var latestBlobs = blobs.OrderBy(b => b.Properties.LastModified).TakeLast(BlobsNumber).ToList();
            Serilog.Log.Information("Name\t\tSize\tLastModified");
            foreach (var blob in latestBlobs)
            {
                Serilog.Log.Information("{Name}\t{Size}\t{LastModified}", blob.Name, blob.Properties.ContentLength, blob.Properties.LastModified);
            }
        });
    }
}
