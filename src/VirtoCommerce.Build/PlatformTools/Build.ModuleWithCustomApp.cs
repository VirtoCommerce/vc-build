using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        private static AbsolutePath AppsDirectory => WebProject?.Directory / "Apps";
        private static AbsolutePath AppDirectory => WebProject?.Directory / "App";

        public static bool ThereAreCustomApps => IsModule && ModuleManifest?.Apps?.Length > 0;

        public Target CompressWithCustomApp => _ => _
            .DependsOn(Clean, WebPackBuild, BuildCustomApp, Test, Publish)
            .Executes(async () =>
            {
                Log.Warning("{target} is deprecated. Use {actualTarget} instead.", nameof(CompressWithCustomApp), nameof(Compress));
                await CompressExecuteMethod();
            });

        public Target BuildCustomApp => _ => _
            .After(WebPackBuild)
            .OnlyWhenDynamic(() => ThereAreCustomApps)
            .Executes(() =>
            {
                var apps = ModuleManifest.Apps;
                var multipleApps = apps.Length > 1;
                var yarn = ToolResolver.GetPathTool("yarn");

                foreach (var appId in apps.Select(a => a.Id))
                {
                    var appFolder = ResolveAppFolder(appId, multipleApps);
                    if (appFolder == null)
                    {
                        Log.Warning("Skipping app {AppId}: no folder with package.json found.", appId);
                        continue;
                    }

                    var distDirectory = appFolder / "dist";
                    distDirectory.DeleteDirectory();
                    Log.Information("Building custom app: {AppId} from {Folder}", appId, appFolder);
                    yarn.Invoke("install", appFolder);
                    yarn.Invoke("build", appFolder);
                    var targetDirectory = WebProject.Directory / "Content" / appId;
                    targetDirectory.DeleteDirectory();
                    distDirectory.Copy(targetDirectory, ExistsPolicy.MergeAndOverwrite);
                }
            });

        private static AbsolutePath ResolveAppFolder(string appId, bool multipleApps)
        {
            // Multiple apps — only Apps/{id} is supported
            if (multipleApps)
            {
                var folder = AppsDirectory / appId;
                return (folder / "package.json").FileExists() ? folder : null;
            }

            // Single app — prefer Apps/{id}, fallback to legacy App/
            var appsSubfolder = AppsDirectory / appId;
            if ((appsSubfolder / "package.json").FileExists())
            {
                return appsSubfolder;
            }

            if ((AppDirectory / "package.json").FileExists())
            {
                return AppDirectory;
            }

            return null;
        }
    }
}
