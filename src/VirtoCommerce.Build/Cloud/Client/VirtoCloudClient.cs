using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cloud.Models;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using VirtoCloud.Client.Model;

namespace Cloud.Client;

public class VirtoCloudClient
{
    private readonly HttpClient _client;

    public VirtoCloudClient(string baseUrl, string token)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(baseUrl);
        _client.DefaultRequestHeaders.Add("api_key", token);
    }

    public async Task<string> UpdateEnvironmentAsync(string manifest, string appProject)
    {
        var content = new Dictionary<string, string>
        {
            { "manifest", manifest },
            { "appProject", appProject }
        };
        var response = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri("api/saas/environments/update", UriKind.Relative),
            Content = new FormUrlEncodedContent(content)
        });

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var error = VirtoCloudError.FromStringResponse(responseContent);
            Assert.Fail(error.GetErrorMessage());
        }

        Serilog.Log.Information(responseContent);
        Serilog.Log.Information("Environment updated successfully");

        return responseContent;
    }

    public async Task UpdateEnvironmentAsync(CloudEnvironment environment)
    {
        var jsonString = JsonExtensions.ToJson(environment);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(new Uri("api/saas/environments", UriKind.Relative), content);
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var error = VirtoCloudError.FromStringResponse(responseContent);
            Assert.Fail(error.GetErrorMessage());
        }
    }

    public async Task<CloudEnvironment> GetEnvironmentAsync(string environmentName, string orgName = null)
    {
        var relativeUri = string.IsNullOrWhiteSpace(orgName) ? $"api/saas/environments/{environmentName}" : $"api/saas/environments/{orgName}/{environmentName}";
        var response = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(relativeUri, UriKind.Relative)
        });
        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"{response.ReasonPhrase}: {await response.Content.ReadAsStringAsync()}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var env = JsonExtensions.GetJson<CloudEnvironment>(responseContent);
        return env;
    }

    public async Task<string> GetManifest(string environmentName, string orgName = null)
    {
        var relativeUri = string.IsNullOrWhiteSpace(orgName) ? $"api/saas/environments/{environmentName}/manifest" : $"api/saas/environments/manifest/{orgName}/{environmentName}";
        var response = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(relativeUri, UriKind.Relative)
        });
        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"{response.ReasonPhrase}: {await response.Content.ReadAsStringAsync()}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}
