using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nuke.Common;

namespace PlatformTools.Modules.Gitlab;

public class GitLabClient
{
    private readonly HttpClient _client;
    private readonly string _apiUrl;

    public GitLabClient(string accessToken, string apiUrl)
    {
        _apiUrl = apiUrl;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", accessToken);
    }

    public async Task<string> DownloadArtifact(string projectId, string jobId, string artifactName, string downloadPath)
    {
        // Get job details
        var jobUrl = $"{_apiUrl}/projects/{projectId}/jobs/{jobId}";
        var jobResponse = await _client.GetAsync(jobUrl);
        var jobJson = await jobResponse.Content.ReadAsStringAsync();
        var jobData = JObject.Parse(jobJson);


        // Get artifact file name and URL
        var artifactFileName = (string)jobData["artifacts_file"]?["filename"];
        var artifactUrl = $"{_apiUrl}/projects/{projectId}/jobs/{jobId}/artifacts/{artifactName}";

        // Download artifact file
        var fileBytes = await _client.GetByteArrayAsync(artifactUrl);
        var artifactPath = Path.Combine(downloadPath, artifactFileName);
        await File.WriteAllBytesAsync(artifactPath, fileBytes);

        return artifactPath;
    }
}
