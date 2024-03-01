using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Nuke.Common;

namespace PlatformTools.Modules.Azure;

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
        Assert.NotEmpty(artifacts,
            $"No builds found for the project id: {project.ToString()}{Environment.NewLine}branch: {branch}{Environment.NewLine}definition: {definitionName}");
        var result = artifacts.FirstOrDefault()?.Resource.DownloadUrl;

        return new Uri(result ?? string.Empty);
    }

    public async Task<Stream> GetArtifactStream(Guid project, string branch, string definitionName)
    {
        var client = await _connection.GetClientAsync<BuildHttpClient>();
        var build = await client.GetLatestBuildAsync(project, definitionName, branch);
        var result = await client.GetArtifactContentZipAsync(project, build.Id, "backend");
        return result;
    }
}
