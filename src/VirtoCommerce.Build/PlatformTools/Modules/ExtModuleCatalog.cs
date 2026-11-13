using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules;

namespace PlatformTools.Modules
{
    public class ExtModuleCatalog
    {
        private static ExtModuleCatalog _catalog;

        public IList<ManifestModuleInfo> Modules { get; private set; }

        public ExternalModuleCatalogOptions Options { get; private init; }

        public static async Task<ExtModuleCatalog> GetCatalog(string authToken, IList<string> manifestUrls, LocalModuleCatalog localModuleCatalog, HttpClient httpClient = null)
        {
            if (_catalog != null)
            {
                return _catalog;
            }

            // Workaround to see all modules in the external catalog
            var latestPlatformVersion = await GithubReleaseService.GetLatestPlatformVersion();

            httpClient ??= new HttpClient();
            var options = CreateOptions(authToken, manifestUrls);

            List<ManifestModuleInfo> externalModules;
            try
            {
                externalModules = ModulePackageInstaller.LoadExternalModules(options, latestPlatformVersion, httpClient).ToList();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load external modules manifest. Only locally installed modules will be available.");
                externalModules = [];
            }

            var installedModules = localModuleCatalog.Modules;
            var modules = localModuleCatalog.Bootstrapper.MergeWithInstalled(externalModules, installedModules);
            SetLocalModuleRefs(modules, installedModules);


            _catalog = new ExtModuleCatalog
            {
                Options = options,
                Modules = modules,
            };


            return _catalog;
        }

        /// <summary>
        /// The merged catalog contains installed modules that are missing from the external catalog
        /// or are newer than the external version (e.g. a hotfix from a bundle). ModuleBootstrapper.Discover()
        /// doesn't set Ref for them (it's set only when the module assembly is loaded), while the callers rely on
        /// the "file:///" prefix to detect local modules. Set it the way LocalStorageModuleCatalog used to.
        /// </summary>
        private static void SetLocalModuleRefs(IList<ManifestModuleInfo> modules, IList<ManifestModuleInfo> installedModules)
        {
            foreach (var module in modules.Where(m => installedModules.Any(i => ReferenceEquals(i, m))))
            {
                if (!string.IsNullOrEmpty(module.FullPhysicalPath))
                {
                    module.Ref = new Uri(module.FullPhysicalPath).AbsoluteUri;
                }
            }
        }

        private static ExternalModuleCatalogOptions CreateOptions(string authToken, IList<string> manifestUrls)
        {
            manifestUrls ??= new List<string>();

            var options = new ExternalModuleCatalogOptions
            {
                AuthorizationToken = authToken,
                IncludePrerelease = false,
                AutoInstallModuleBundles = [],
                ExtraModulesManifestUrls = manifestUrls.Skip(1).Select(x => new Uri(x)).ToArray(),
            };

            if (manifestUrls.Count > 0)
            {
                options.ModulesManifestUrl = new Uri(manifestUrls[0]);
            }

            return options;
        }

        internal static void Reset()
        {
            _catalog = null;
        }
    }
}
