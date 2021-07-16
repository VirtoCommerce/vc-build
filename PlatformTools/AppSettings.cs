using System.IO;
using Microsoft.Extensions.Configuration;

namespace PlatformTools
{
    internal class AppSettings
    {
        private static IConfiguration _configuration;

        public static IConfiguration GetConfiguration(string basePath, string appsettingsPath)
        {
            if (_configuration == null && File.Exists(appsettingsPath))
            {
                var builder = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile(appsettingsPath);
                _configuration = builder.Build();
            }

            return _configuration;
        }
    }
}
