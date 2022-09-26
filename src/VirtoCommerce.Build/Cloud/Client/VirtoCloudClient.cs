using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Nuke.Common;

namespace Cloud.Client
{
    public class VirtoCloudClient
    {
        private readonly string _baseUrl;
        private readonly string _token;

        public VirtoCloudClient(string baseUrl, string token)
        {
            _baseUrl = baseUrl;
            _token = token;
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Add("api_key", _token);
            return client;
        }

        public async Task<string> UpdateEnvironmentAsync(string manifest, string appProject)
        {
            var client = GetHttpClient();
            var content = new Dictionary<string, string>();
            content.Add("manifest", manifest);
            content.Add("appProject", appProject);
            var response = await client.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri("api/saas/environments/update", UriKind.Relative),
                Content = new FormUrlEncodedContent(content)
            });

            if (!response.IsSuccessStatusCode)
            {
                Assert.Fail($"{response.ReasonPhrase}: {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
