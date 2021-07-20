using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules;
using VirtoCommerce.Platform.Modules.External;

namespace PlatformTools
{
    internal class ExtModuleCatalog
    {
        private static ExternalModuleCatalog _catalog;

        public static ExternalModuleCatalog GetCatalog(string authToken, LocalStorageModuleCatalog localCatalog, IList<string> manifestUrls)
        {
            var options = GetOptions(authToken, manifestUrls);
            return GetCatalog(options, localCatalog);
        }

        public static ExternalModuleCatalog GetCatalog(IOptions<ExternalModuleCatalogOptions> options, LocalStorageModuleCatalog localCatalog)
        {
            if (_catalog == null)
            {
                var platformRelease = GithubManager.GetPlatformRelease(null).GetAwaiter().GetResult();
                PlatformVersion.CurrentVersion = new SemanticVersion(Version.Parse(platformRelease.TagName)); // workaround to see all modules in the external catalog
                var client = new ExternalModulesClient(options);
                var logger = new LoggerFactory().CreateLogger<ExternalModuleCatalog>();
                _catalog = new ExternalModuleCatalog(localCatalog, client, options, logger);
                _catalog.Load();
            }
            else
            {
                _catalog.Reload();
            }

            return _catalog;
        }

        public static IOptions<ExternalModuleCatalogOptions> GetOptions(string authToken, IList<string> manifestUrls)
        {
            var options = new ExternalModuleCatalogOptions
            {
                ModulesManifestUrl = new Uri(manifestUrls.First()),
                AuthorizationToken = authToken,
                IncludePrerelease = false,
                AutoInstallModuleBundles = Array.Empty<string>(),
                ExtraModulesManifestUrls = manifestUrls.Select(m => new Uri(m)).ToArray(),
            };

            return Options.Create(options);
        }
    }
}
