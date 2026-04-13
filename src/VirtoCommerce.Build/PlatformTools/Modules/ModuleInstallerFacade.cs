using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Modules;
using VirtoCommerce.Platform.Modules.External;

namespace PlatformTools.Modules
{
    internal static class ModuleInstallerFacade
    {
        private static ModuleInstaller _moduleInstaller;

        public static async Task<ModuleInstaller> GetModuleInstaller(string discoveryPath, string probingPath, string authToken, IList<string> manifestUrls)
        {
            if (_moduleInstaller == null)
            {
                var fileSystem = new FileSystem();
                var localCatalogOptions = LocalModuleCatalog.GetOptions(discoveryPath, probingPath);
                var extCatalogOptions = ExtModuleCatalog.GetOptions(authToken, manifestUrls);
                var localModuleCatalog = LocalModuleCatalog.GetCatalog(localCatalogOptions);
                var externalModuleCatalog = await ExtModuleCatalog.GetCatalog(extCatalogOptions, localModuleCatalog);
                var modulesClient = new ExternalModulesClient(extCatalogOptions, new CustomHttpClientFactory());
                _moduleInstaller = new ModuleInstaller(externalModuleCatalog, modulesClient, localCatalogOptions, fileSystem);
            }

            return _moduleInstaller;
        }
    }
}
