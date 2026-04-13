using System.Net.Http;

namespace PlatformTools.Modules
{
    internal class CustomHttpClientFactory(HttpClient httpClient = null) : IHttpClientFactory
    {
        private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
