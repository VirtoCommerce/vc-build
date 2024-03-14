using System;
using System.IO;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common.IO;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules.LocalModules
{
    internal class LocalModuleInstaller : ModuleInstallerBase
    {
        readonly string _modulesDirectory;

        public LocalModuleInstaller(string modulesDirectory)
        {
            _modulesDirectory = modulesDirectory;
        }
        protected override async Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var moduleSource = (Local)source;

            foreach (var module in moduleSource.Modules)
            {
                var moduleSourceName = module.Id ?? Path.GetFileName(module.Path);
                var moduleDestination = Path.Combine(_modulesDirectory, moduleSourceName);
                var attributes = File.GetAttributes(module.Path);
                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    progress.ReportInfo($"Copying the module from the directory {module.Path}");
                    await SetupModuleFromDirectory(module.Path, moduleDestination);
                }
                else
                {
                    progress.ReportInfo($"Extracting an archive {moduleSourceName}");
                    await SetupModuleFromArchive(module.Path, moduleDestination);
                }
                progress.ReportInfo($"Successfully installed {moduleSourceName}");
            }
        }

        private static Task SetupModuleFromArchive(string src, string moduleDestination)
        {
            var absolutePath = src.ToAbsolutePath();
            absolutePath.UnZipTo(moduleDestination.ToAbsolutePath());
            return Task.CompletedTask;
        }

        private static Task SetupModuleFromDirectory(string src, string moduleDestination)
        {
            var absolutePath = src.ToAbsolutePath();
            FileSystemTasks.CopyDirectoryRecursively(absolutePath, moduleDestination.ToAbsolutePath(), DirectoryExistsPolicy.Merge, FileExistsPolicy.OverwriteIfNewer);
            return Task.CompletedTask;
        }
    }
}
