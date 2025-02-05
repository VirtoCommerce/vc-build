using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
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
                var arguments = new string[]
                    {
                        "artifacts universal download",
                        $"--organization \"{artifacts.Organization}\"",
                        $"--project \"{artifacts.Project}\"",
                        $"--feed {artifacts.Feed}",
                        $"--name {module.Id}",
                        $"--path \"{moduleDestination}\"",
                        "--scope project",
                        $"--version {module.Version}"
                    };

                var envVars = new Dictionary<string, string>() { { "AZURE_DEVOPS_EXT_PAT", token } };

                ProcessTasks.StartProcess(azPath, arguments: string.Join(' ', arguments), environmentVariables: envVars).AssertZeroExitCode();

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

