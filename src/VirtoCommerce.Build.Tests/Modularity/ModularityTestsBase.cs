using Microsoft.Extensions.Logging;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Modules;

namespace VirtoCommerce.Build.Tests.Modularity;

[Collection("ModuleCatalogTests")]
public abstract class ModularityTestsBase : IDisposable
{
    protected string TestRoot { get; }
    protected string DiscoveryPath { get; set; }
    protected string ProbingPath { get; set; }

    protected ModularityTestsBase()
    {
        TestRoot = Path.Combine(Path.GetTempPath(), "vc-build-tests", Guid.NewGuid().ToString());
        DiscoveryPath = Path.Combine(TestRoot, "modules");
        ProbingPath = Path.Combine(TestRoot, "probing");

        Directory.CreateDirectory(DiscoveryPath);

        PlatformVersion.CurrentVersion ??= SemanticVersion.Parse("3.1000.0");

        ModuleBootstrapper.Instance ??= new ModuleBootstrapper(
            new LoggerFactory(),
            new LocalStorageModuleCatalogOptions
            {
                DiscoveryPath = DiscoveryPath,
                ProbingPath = ProbingPath,
            });
    }

    public void Dispose()
    {
        Reset();

        if (Directory.Exists(TestRoot))
        {
            Directory.Delete(TestRoot, true);
        }
    }


    protected LocalCatalog GetLocalModuleCatalog()
    {
        return LocalModuleCatalog.GetCatalog(DiscoveryPath, ProbingPath);
    }

    protected void WriteManifest(string id, string version, string? assemblyFile = null, string[]? dependencies = null)
    {
        WriteManifest(id, version, Path.Combine(DiscoveryPath, id), assemblyFile, dependencies);
    }

    protected void WriteManifest(string id, string version, string directoryPath, string? assemblyFile = null, string[]? dependencies = null)
    {
        Directory.CreateDirectory(directoryPath);

        var assemblyElement = "";
        var dependenciesElement = "";

        if (!string.IsNullOrEmpty(assemblyFile))
        {
            assemblyElement = $"  <assemblyFile>{assemblyFile}</assemblyFile>";
        }

        if (dependencies != null)
        {
            var dependencyElements = string.Join("", dependencies.Select(x =>
            {
                var parts = x.Split(':');
                return $"    <dependency id=\"{parts[0]}\" version=\"{parts[1]}\" />\n";
            }));

            dependenciesElement = $"<dependencies>\n{dependencyElements}  </dependencies>";
        }

        var xml = $"""
                   <?xml version="1.0" encoding="utf-8"?>
                   <module xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                     <id>{id}</id>
                     <title>{id}</title>
                     <version>{version}</version>
                     <platformVersion>3.1000.0</platformVersion>
                     {dependenciesElement}
                     {assemblyElement}
                   </module>
                   """;

        var manifestPath = Path.Combine(directoryPath, "module.manifest");
        File.WriteAllText(manifestPath, xml);
    }

    protected virtual void Reset()
    {
        LocalModuleCatalog.Reset();
    }
}
