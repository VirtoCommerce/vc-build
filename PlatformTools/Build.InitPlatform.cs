using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nuke.Common;
using PlatformTools;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.DistributedLock;
using VirtoCommerce.Platform.Modules;

partial class Build : NukeBuild
{
    [Parameter("Modules discovery path")] public static string DiscoveryPath;
    [Parameter("Probing path")] public static string ProbingPath = RootDirectory / "app_data" / "modules";
    [Parameter("appsettings.json path")] public static string AppsettingsPath = RootDirectory / "appsettings.json";

    Target InitPlatform => _ => _
    .Executes(() =>
    {
        IConfiguration configuration = AppSettings.GetConfiguration(RootDirectory, AppsettingsPath);
        var moduleCatalogOptions = new LocalStorageModuleCatalogOptions()
        {
            DiscoveryPath = string.IsNullOrEmpty(DiscoveryPath) ? configuration.GetModulesDiscoveryPath() : DiscoveryPath,
            ProbingPath = ProbingPath
        };
        var options = Microsoft.Extensions.Options.Options.Create<LocalStorageModuleCatalogOptions>(moduleCatalogOptions);
        var logger = new LoggerFactory().CreateLogger<LocalStorageModuleCatalog>();
        var distributedLock = new NoLockDistributedLockProvider(new LoggerFactory().CreateLogger<NoLockDistributedLockProvider>());
        var moduleCatalog = new LocalStorageModuleCatalog(options, distributedLock, logger);
        moduleCatalog.Load();
    });
}
