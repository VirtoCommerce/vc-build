using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace VirtoCommerce.Build.PlatformTools.Azure
{
    internal class AzureUniversalPackagesModuleInstaller: IModulesInstaller
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
            var azureClient = new AzureDevClient(artifacts.Organization, token);
            foreach (var module in artifacts.Modules)
            {
                var moduleDestination = Path.Join(discoveryPath, module.Id);
                Directory.CreateDirectory(moduleDestination);
                FileSystemTasks.EnsureCleanDirectory(moduleDestination);
                var argsBuilder = new StringBuilder("artifacts universal download");
                argsBuilder = argsBuilder
                    .Append($" --organization {artifacts.Organization}")
                    .Append($" --project {artifacts.Project}")
                    .Append($" --feed {artifacts.Feed}")
                    .Append($" --name {module.Id}")
                    .Append($" --path {moduleDestination}")
                    .Append($" --scope project")
                    .Append($" --version {module.Version}");
                var envVariables = new Dictionary<string, string>()
                {
                    { "AZURE_DEVOPS_EXT_PAT", token }
                };
                var azPath = ToolPathResolver.GetPathExecutable("az");
                var process = ProcessTasks.StartProcess(toolPath: azPath, arguments: argsBuilder.ToString(), environmentVariables: envVariables).AssertZeroExitCode();

                var zipPath = Directory.GetFiles(moduleDestination).Where(p => p.EndsWith(".zip")).FirstOrDefault();
                if (zipPath == null)
                    ControlFlow.Fail($"Can't download {module.Id} - {module.Version}");

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

