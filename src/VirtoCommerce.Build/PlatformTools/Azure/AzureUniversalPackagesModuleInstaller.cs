using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzureUniversalPackagesModuleInstaller : ModulesInstallerBase
    {
        readonly string token;
        readonly string discoveryPath;

        public AzureUniversalPackagesModuleInstaller(string token, string discoveryPath)
        {
            this.token = token;
            this.discoveryPath = discoveryPath;
        }

        protected override Task InnerInstall(ModuleSource source)
        {
            var artifacts = (AzureUniversalPackages)source;
            foreach (var module in artifacts.Modules)
            {
                Log.Information($"Installing {module.Id}");
                var moduleDestination = Path.Join(discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                FileSystemTasks.EnsureCleanDirectory(moduleDestination);
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
                    Assert.Fail($"Can't download {module.Id} - {module.Version}");
                }

                Log.Information($"Extracting {zipPath}");
                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
                Log.Information($"Successfully installed {module.Id}");
            }

            return Task.CompletedTask;
        }

        public override Task Install(ModuleSource source)
        {
            return InnerInstall((AzureUniversalPackages)source);
        }
    }
}

