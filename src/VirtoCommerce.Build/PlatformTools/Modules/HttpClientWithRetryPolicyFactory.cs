using System.Net.Http;

namespace PlatformTools.Modules
{
    public class HttpClientWithRetryPolicyFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new RetryDelegatingHandler());
    }
}
