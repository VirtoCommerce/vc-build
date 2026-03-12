using System.Net.Http;

namespace PlatformTools.Modules
{
    class SimpleHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}
