using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuke.Common;
using PlatformTools;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.Platform.Modules;

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

            var moduleCatalogOptions = new LocalStorageModuleCatalogOptions
            {
                DiscoveryPath = string.IsNullOrEmpty(DiscoveryPath) ? configuration.GetModulesDiscoveryPath() : DiscoveryPath,
                ProbingPath = ProbingPath,
            };

            var options = Options.Create(moduleCatalogOptions);
            var logger = new LoggerFactory().CreateLogger<LocalStorageModuleCatalog>();
            var distributedLock = new NoLockDistributedLockProvider(new LoggerFactory().CreateLogger<NoLockDistributedLockProvider>());
            var moduleCatalog = new LocalStorageModuleCatalog(options, distributedLock, logger);
            moduleCatalog.Load();
        });
}
