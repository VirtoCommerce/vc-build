using Nuke.Common;
using PlatformTools;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("Modules discovery path")]
        public static string DiscoveryPath { get; set; }

        [Parameter("Probing path")]
        public static string ProbingPath { get; set; } = RootDirectory / "app_data" / "modules";

        [Parameter("appsettings.json path")]
        public static string AppsettingsPath { get; set; } = RootDirectory / "appsettings.json";

        public Target InitPlatform => _ => _
            .Executes(() =>
            {
                var configuration = AppSettings.GetConfiguration(RootDirectory, AppsettingsPath);
                LocalModuleCatalog.GetCatalog(DiscoveryPath.EmptyToNull() ?? configuration.GetModulesDiscoveryPath(), ProbingPath);
            });
    }
}
