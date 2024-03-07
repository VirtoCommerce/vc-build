using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules.External;

namespace PlatformTools.Modules.Azure
{
    public class AzurePipelineArtifactsClient : IExternalModulesClient
    {
        private readonly ExternalModuleCatalogOptions _options;

        public AzurePipelineArtifactsClient(IOptions<ExternalModuleCatalogOptions> options)
        {
            _options = options.Value;
        }

        public Stream OpenRead(Uri address)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, address);
            HttpResponseMessage response;
            try
            {
                if (!string.IsNullOrEmpty(_options.AuthorizationToken))
                {
                    var tokenEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{_options.AuthorizationToken}"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic " + tokenEncoded);
                }
                response = client.Send(request);
                return response.Content.ReadAsStream();
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}

