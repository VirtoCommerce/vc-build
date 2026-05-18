using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Extensions.Logging;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules;

namespace PlatformTools.Modules
{
    public class LocalModuleCatalog
    {
        private static LocalModuleCatalog _catalog;

        public ModuleBootstrapper Bootstrapper { get; private init; }

        public IList<ManifestModuleInfo> Modules => Bootstrapper.GetModules();

        public static LocalModuleCatalog GetCatalog(string discoveryPath, string probingPath)
        {
            if (string.IsNullOrEmpty(discoveryPath))
            {
                throw new InvalidOperationException("The DiscoveryPath cannot be null or empty");
            }

            if (string.IsNullOrEmpty(probingPath))
            {
                throw new InvalidOperationException("The ProbingPath cannot be null or empty");
            }

            if (_catalog == null)
            {
                var options = new LocalStorageModuleCatalogOptions
                {
                    DiscoveryPath = discoveryPath,
                    ProbingPath = probingPath,
                    RefreshProbingFolderOnStart = true,
                };

                var loggerFactory = new SerilogLoggerFactory(Log.Logger);
                var bootstrapper = new ModuleBootstrapper(loggerFactory, options);

                _catalog = new LocalModuleCatalog
                {
                    Bootstrapper = bootstrapper,
                };
            }

            _catalog.Reload();

            ModuleBootstrapper.Instance = _catalog.Bootstrapper;

            return _catalog;
        }

        public void Reload()
        {
            Bootstrapper.Discover();
        }

        public bool ValidateDependencies(string platformVersion)
        {
            Bootstrapper.Validate(SemanticVersion.Parse(platformVersion));

            var failedModules = Bootstrapper.GetFailedModules();
            foreach (var module in failedModules)
            {
                foreach (var error in module.Errors)
                {
                    Log.Error("{moduleId}: {error}", module.Id, error);
                }
            }

            if (failedModules.Count > 0)
            {
                Log.Error("Module validation failed. See the logs for more details.");
                return false;
            }

            return true;
        }

        public void RefreshProbingDirectory()
        {
            Bootstrapper.InvalidateProbingFolder();
            Bootstrapper.Copy(RuntimeInformation.ProcessArchitecture);
        }

        internal static void Reset()
        {
            _catalog = null;
        }
    }
}
