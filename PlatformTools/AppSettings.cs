using System.IO;
using Microsoft.Extensions.Configuration;

namespace PlatformTools
{
    internal class AppSettings
    {
        private static IConfiguration _configuration;

        public static IConfiguration GetConfiguration(string basePath, string appSettingsPath)
        {
            if (_configuration == null && File.Exists(appSettingsPath))
            {
                var builder = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile(appSettingsPath);
                _configuration = builder.Build();
            }

            return _configuration;
        }
    }
}
