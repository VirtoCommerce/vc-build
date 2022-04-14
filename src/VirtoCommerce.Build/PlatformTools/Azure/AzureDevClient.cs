using System;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace PlatformTools.Azure
{
    public class AzureDevClient
    {
        private readonly VssConnection _connection;

        public AzureDevClient(string orgName, string token)
        {
            _connection = new VssConnection(new Uri(orgName), new VssBasicCredential(string.Empty, token));
        }

        public async Task<Uri> GetArtifactUrl(Guid project, string branch, string definitionName)
        {
            var client = await _connection.GetClientAsync<BuildHttpClient>();
            var build = await client.GetLatestBuildAsync(project, definitionName, branch);
            var artifacts = await client.GetArtifactsAsync(project, build.Id);
            var result = artifacts.FirstOrDefault().Resource.DownloadUrl;

            return new Uri(result);
        }

        public async Task<Stream> GetArtifactStream(Guid project, string branch, string definitionName)
        {
            var client = await _connection.GetClientAsync<BuildHttpClient>();
            var build = await client.GetLatestBuildAsync(project, definitionName, branch);
            var result = await client.GetArtifactContentZipAsync(project, build.Id, "backend");
            return result;
        }
    }
}
