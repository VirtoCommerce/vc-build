using System;
using System.IO;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common.IO;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules;

namespace PlatformTools.Modules.LocalModules
{
    internal class LocalModuleInstaller : ModuleInstallerBase
    {
        readonly string _modulesDirectory;

        public LocalModuleInstaller(string modulesDirectory)
        {
            _modulesDirectory = modulesDirectory;
        }
        protected override Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var moduleSource = (LocalModuleSource)source;

            foreach (var module in moduleSource.Modules)
            {
                var moduleSourceName = module.Id ?? Path.GetFileName(module.Path);
                var moduleDestination = Path.Combine(_modulesDirectory, moduleSourceName);
                var attributes = File.GetAttributes(module.Path);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    progress.ReportInfo($"Copying the module from the directory {module.Path}");
                    SetupModuleFromDirectory(module.Path, moduleDestination);
                }
                else
                {
                    progress.ReportInfo($"Extracting an archive {moduleSourceName}");
                    ModulePackageInstaller.Install(module.Path, moduleDestination, deleteZip: false);
                }
                progress.ReportInfo($"Successfully installed {moduleSourceName}");
            }

            return Task.CompletedTask;
        }

        private static void SetupModuleFromDirectory(string src, string moduleDestination)
        {
            var absolutePath = src.ToAbsolutePath();
            absolutePath.Copy(moduleDestination.ToAbsolutePath(), ExistsPolicy.MergeAndOverwriteIfNewer);
        }
    }
}
