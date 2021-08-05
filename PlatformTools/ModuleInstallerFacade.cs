using System.Collections.Generic;
using System.IO.Abstractions;
using VirtoCommerce.Platform.Data.TransactionFileManager;
using VirtoCommerce.Platform.Data.ZipFile;
using VirtoCommerce.Platform.Modules;
using VirtoCommerce.Platform.Modules.External;

namespace PlatformTools
{
    internal class ModuleInstallerFacade
    {
        private static ModuleInstaller _moduleInstaller;

        public static ModuleInstaller GetModuleInstaller(string discoveryPath, string probingPath, string authToken, IList<string> manifestUrls)
        {
            if (_moduleInstaller == null)
            {
                var fileManager = new TransactionFileManager();
                var fileSystem = new FileSystem();
                var zipFileWrapper = new ZipFileWrapper(fileSystem, fileManager);
                var localCatalogOptions = LocalModuleCatalog.GetOptions(discoveryPath, probingPath);
                var extCatalogOptions = ExtModuleCatalog.GetOptions(authToken, manifestUrls);
                var localModuleCatalog = LocalModuleCatalog.GetCatalog(localCatalogOptions);
                var externalModuleCatalog = ExtModuleCatalog.GetCatalog(extCatalogOptions, localModuleCatalog);
                var modulesClient = new ExternalModulesClient(extCatalogOptions);
                _moduleInstaller = new ModuleInstaller(externalModuleCatalog, modulesClient, fileManager, localCatalogOptions, fileSystem, zipFileWrapper);
            }

            return _moduleInstaller;
        }
    }
}
