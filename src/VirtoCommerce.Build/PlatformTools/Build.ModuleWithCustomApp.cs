using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        private const string DefaultCustomAppOutputFolder = "dist";

        [Parameter("Package manager executable for custom apps (e.g. yarn, pnpm, npm, bun). When unset, uses ni via npx for auto-detection. When set, both --custom-app-install-command and --custom-app-build-command are required.", Name = "CustomAppPackageManager")]
        public static string CustomAppPackageManager { get; set; }

        [Parameter("Install command (passed to the package manager) for custom apps. Required when --custom-app-package-manager is set.", Name = "CustomAppInstallCommand")]
        public static string CustomAppInstallCommand { get; set; }

        [Parameter("Build command (passed to the package manager) for custom apps. Required when --custom-app-package-manager is set.", Name = "CustomAppBuildCommand")]
        public static string CustomAppBuildCommand { get; set; }

        [Parameter("Custom app build output folder, relative to the app folder. Default: 'dist'.", Name = "CustomAppOutputFolder")]
        public static string CustomAppOutputFolder { get; set; }

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
                var outputFolder = string.IsNullOrWhiteSpace(CustomAppOutputFolder) ? DefaultCustomAppOutputFolder : CustomAppOutputFolder;

                var packageManagerIsSet = !string.IsNullOrWhiteSpace(CustomAppPackageManager);
                if (packageManagerIsSet)
                {
                    Assert.NotNullOrWhiteSpace(CustomAppInstallCommand, $"--custom-app-install-command is required when --custom-app-package-manager is set (got '{CustomAppPackageManager}').");
                    Assert.NotNullOrWhiteSpace(CustomAppBuildCommand, $"--custom-app-build-command is required when --custom-app-package-manager is set (got '{CustomAppPackageManager}').");
                }
                else if (!string.IsNullOrWhiteSpace(CustomAppInstallCommand) || !string.IsNullOrWhiteSpace(CustomAppBuildCommand))
                {
                    Assert.Fail("--custom-app-package-manager is required when --custom-app-install-command or --custom-app-build-command is set (commands use package-manager-specific syntax that cannot be passed to ni).");
                }

                var packageManagerTool = ToolResolver.GetPathTool(packageManagerIsSet ? CustomAppPackageManager : "npx");
                var installArguments = packageManagerIsSet ? CustomAppInstallCommand : "-y @antfu/ni --frozen";
                var buildArguments = packageManagerIsSet ? CustomAppBuildCommand : "-y -p @antfu/ni nr build";
                var env = new Dictionary<string, string>(EnvironmentInfo.Variables, StringComparer.OrdinalIgnoreCase)
                {
                    ["COREPACK_ENABLE_DOWNLOAD_PROMPT"] = "0",
                };

                foreach (var appId in apps.Select(a => a.Id))
                {
                    var appFolder = ResolveAppFolder(appId, multipleApps);
                    if (appFolder == null)
                    {
                        Log.Warning("Skipping app {AppId}: no folder with package.json found.", appId);
                        continue;
                    }

                    var distDirectory = appFolder / outputFolder;
                    distDirectory.DeleteDirectory();
                    Log.Information("Building custom app: {AppId} from {Folder} (output: {OutputFolder})", appId, appFolder, outputFolder);
                    packageManagerTool.Invoke(installArguments, appFolder, env);
                    packageManagerTool.Invoke(buildArguments, appFolder, env);
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
