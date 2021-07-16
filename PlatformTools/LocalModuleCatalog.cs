using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.Platform.Modules;

namespace PlatformTools
{
    internal class LocalModuleCatalog
    {
        private static LocalStorageModuleCatalog _catalog;

        public static LocalStorageModuleCatalog GetCatalog(string discoveryPath, string probingPath)
        {
            var options = GetOptions(discoveryPath, probingPath);
            return GetCatalog(options);
        }

        public static LocalStorageModuleCatalog GetCatalog(IOptions<LocalStorageModuleCatalogOptions> options)
        {
            if (_catalog == null)
            {
                var logger = new LoggerFactory().CreateLogger<LocalStorageModuleCatalog>();
                var distributedLock = new NoLockDistributedLockProvider(new LoggerFactory().CreateLogger<NoLockDistributedLockProvider>());
                _catalog = new LocalStorageModuleCatalog(options, distributedLock, logger);
                _catalog.Load();
            }
            else
            {
                _catalog.Reload();
            }

            return _catalog;
        }

        public static IOptions<LocalStorageModuleCatalogOptions> GetOptions(string discoveryPath, string probingPath)
        {
            var moduleCatalogOptions = new LocalStorageModuleCatalogOptions
            {
                RefreshProbingFolderOnStart = true,
                DiscoveryPath = discoveryPath,
                ProbingPath = probingPath,
            };

            return Options.Create(moduleCatalogOptions);
        }
    }
}
