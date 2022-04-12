using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Azure
{
    internal class AzureUniversalPackagesModuleInstaller : IModulesInstaller
    {
        readonly string token;
        readonly string discoveryPath;

        public AzureUniversalPackagesModuleInstaller(string token, string discoveryPath)
        {
            this.token = token;
            this.discoveryPath = discoveryPath;
        }

        public Task InnerInstall(AzureUniversalPackages artifacts)
        {
            foreach (var module in artifacts.Modules)
            {
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
                    Assert.Fail($"Can't download {module.Id} - {module.Version}");

                ZipFile.ExtractToDirectory(zipPath, moduleDestination);
            }

            return Task.CompletedTask;
        }

        public Task Install(ModuleSource source)
        {
            return InnerInstall((AzureUniversalPackages)source);
        }
    }
}

