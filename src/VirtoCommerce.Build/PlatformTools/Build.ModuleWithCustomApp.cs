using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        public Target CompressWithCustomApp => _ => _
            .DependsOn(CleanWithCustomApp, BuildCustomApp, Test, Publish)
            .Executes(CompressExecuteMethod);

        public Target BuildCustomApp => _ => _
            .Executes(() =>
            {
                if (WebProject != null && ModuleManifest.Apps.Any())
                {
                    foreach (var app in ModuleManifest.Apps)
                    {
                        if (!(WebProject.Directory / "App" / "package.json").FileExists())
                        {
                            continue;
                        }

                        var chmod = ToolResolver.GetPathTool("yarn");
                        chmod.Invoke("install", WebProject.Directory / "App");
                        chmod.Invoke("build", WebProject.Directory / "App");
                        FileSystemTasks.CopyDirectoryRecursively(WebProject.Directory / "App" / "dist",
                            WebProject.Directory / "Content" / app.Id,
                            DirectoryExistsPolicy.Merge,
                            FileExistsPolicy.Overwrite);
                    }
                }
                else
                {
                    Log.Information("Nothing to build.");
                }
            });

        public Target CleanWithCustomApp => _ => _
            .Before(Restore)
            .Executes(() =>
            {
                var ignorePaths = new[] { WebProject.Directory / "App" };
                var searchPattern = new[] { "**/bin", "**/obj" };
                CleanSolution(searchPattern, ignorePaths);
            });
    }
}
