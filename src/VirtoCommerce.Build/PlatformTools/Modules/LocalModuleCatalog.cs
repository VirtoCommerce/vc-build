using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.Platform.Modules;
using VirtoCommerce.Platform.Modules.Local;

namespace PlatformTools.Modules
{
    internal static class LocalModuleCatalog
    {
        private static LocalCatalog _catalog;

        public static ILocalModuleCatalog GetCatalog(string discoveryPath, string probingPath)
        {
            var options = GetOptions(discoveryPath, probingPath);
            return GetCatalog(options);
        }

        public static ILocalModuleCatalog GetCatalog(IOptions<LocalStorageModuleCatalogOptions> options)
        {
            if (_catalog == null)
            {
                var logger = new LoggerFactory().CreateLogger<LocalStorageModuleCatalog>();
                var distributedLock = new InternalNoLockService(new LoggerFactory().CreateLogger<InternalNoLockService>());
                var fileMetadataProvider = new FileMetadataProvider(options);
                var fileCopyPolicy = new FileCopyPolicy(fileMetadataProvider);
                _catalog = new LocalCatalog(options, distributedLock, fileCopyPolicy, logger, Options.Create(new ModuleSequenceBoostOptions()));
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
