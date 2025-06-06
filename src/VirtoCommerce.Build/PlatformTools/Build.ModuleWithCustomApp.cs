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
            .Executes(() =>
            {
                Log.Warning("{target} is deprecated. Use {actualTarget} instead.", nameof(CompressWithCustomApp), nameof(Compress));
                CompressExecuteMethod();
            });

        public Target BuildCustomApp => _ => _
            .After(WebPackBuild)
            .OnlyWhenDynamic(() => ThereAreCustomApps)
            .Executes(() =>
            {
                if (WebProject != null && ModuleManifest.Apps.Length > 0)
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
                        var sourceDirectory = WebProject.Directory / "App" / "dist";
                        sourceDirectory.Copy(WebProject.Directory / "Content" / app.Id, ExistsPolicy.MergeAndOverwrite);
                    }
                }
                else
                {
                    Log.Information("Nothing to build.");
                }
            });
    }
}
