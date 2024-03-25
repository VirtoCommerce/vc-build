using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules.Azure
{
    internal class AzureUniversalPackagesModuleInstaller : ModuleInstallerBase
    {
        readonly string token;
        readonly string discoveryPath;

        public AzureUniversalPackagesModuleInstaller(string token, string discoveryPath)
        {
            this.token = token;
            this.discoveryPath = discoveryPath;
        }

        protected override Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            var artifacts = (AzureUniversalPackages)source;
            foreach (var module in artifacts.Modules)
            {
                progress.ReportInfo($"Installing {module.Id}");
                var moduleDestination = Path.Join(discoveryPath, module.Id).ToAbsolutePath();
                Directory.CreateDirectory(moduleDestination);
                moduleDestination.CreateOrCleanDirectory();
                var azPath = ToolPathResolver.GetPathExecutable("az");
                var azToolSettings = new AzureCliToolSettings()
                    .AddProcessEnvironmentVariable("AZURE_DEVOPS_EXT_PAT", token)
                    .SetProcessToolPath(azPath)
                    .SetProcessArgumentConfigurator(c => c
                        .Add("artifacts universal download")
                        .Add("--organization \"{0}\"", artifacts.Organization)
                        .Add("--project \"{0}\"", artifacts.Project)
                        .Add("--feed {0}", artifacts.Feed)
                        .Add("--name {0}", module.Id)
                        .Add("--path \"{0}\"", moduleDestination)
                        .Add("--scope {0}", "project")
                        .Add("--version {0}", module.Version)
                    );
                ProcessTasks.StartProcess(azToolSettings).AssertZeroExitCode();

                var zipPath = Directory.GetFiles(moduleDestination).FirstOrDefault(p => p.EndsWith(".zip"));
                if (zipPath == null)
                {
                    progress.ReportError($"Can't download {module.Id} - {module.Version}");
                }

                progress.ReportInfo($"Extracting {zipPath}");
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
                progress.ReportInfo($"Successfully installed {module.Id}");
            }

            return Task.CompletedTask;
        }
    }
}

