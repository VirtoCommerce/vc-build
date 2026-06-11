using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace PlatformTools.Modules.Azure
{
    public class AzurePipelineArtifactsClient
    {
        private readonly string _authorizationToken;

        public AzurePipelineArtifactsClient(string authorizationToken)
        {
            _authorizationToken = authorizationToken;
        }

        public Stream OpenRead(Uri address)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, address);

            try
            {
                if (!string.IsNullOrEmpty(_authorizationToken))
                {
                    var tokenEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{_authorizationToken}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", tokenEncoded);
                }

                var response = client.Send(request);

                return response.Content.ReadAsStream();
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}
