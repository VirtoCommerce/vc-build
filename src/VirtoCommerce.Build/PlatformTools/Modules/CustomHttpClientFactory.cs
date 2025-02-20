using System.Net.Http;

namespace PlatformTools.Modules
{
    class CustomHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}
