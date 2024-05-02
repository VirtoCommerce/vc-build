using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        public static bool ThereAreCustomApps => IsModule && ModuleManifest?.Apps?.Length > 0;

        public Target CompressWithCustomApp => _ => _
            .DependsOn(Clean, WebPackBuild, BuildCustomApp, Test, Publish)
            .Executes(() => {
                Log.Warning("{target} is deprecated. Use {actualTarget} instead.", nameof(CompressWithCustomApp), nameof(Compress));
                CompressExecuteMethod();
            });

        public Target BuildCustomApp => _ => _
            .After(WebPackBuild)
            .OnlyWhenDynamic(() => ThereAreCustomApps)
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
    }
}
