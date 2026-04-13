using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.Build.Tests.Modularity;

public class LocalModuleCatalogTests : ModularityTestsBase
{
    // --- Module discovery ---

    [Fact]
    public void GetCatalog_ThrowsOnEmptyProbingPath()
    {
        // Arrange
        ProbingPath = "";

        // Act and Assert
        Assert.ThrowsAny<Exception>(GetLocalModuleCatalog);
    }

    [Fact]
    public void GetCatalog_ThrowsOnEmptyDiscoveryPath()
    {
        // Arrange
        DiscoveryPath = "";

        // Act and Assert
        Assert.ThrowsAny<Exception>(GetLocalModuleCatalog);
    }

    [Fact]
    public void GetCatalog_NonexistentDiscoveryPath_LoadsNoModules()
    {
        // Arrange
        DiscoveryPath = Path.Combine(TestRoot, "nonexistent");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Empty(modules);
    }

    [Fact]
    public void GetCatalog_EmptyDiscoveryDirectory_LoadsNoModules()
    {
        // Arrange: empty discovery directory, no manifests

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Empty(modules);
    }

    [Fact]
    public void GetCatalog_ReturnsSameInstance_OnSecondCall()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");

        // Act
        var catalog1 = GetLocalModuleCatalog();
        var catalog2 = GetLocalModuleCatalog();

        // Assert
        Assert.Same(catalog1, catalog2);
    }

    [Fact]
    public void GetCatalog_SkipsManifestsInArtifactsFolder()
    {
        // Arrange
        var artifactsPath = Path.Combine(DiscoveryPath, "SomeModule", "artifacts", "NestedModule");
        WriteManifest("NestedModule", "1.0.0", directoryPath: artifactsPath);
        WriteManifest("NormalModule", "1.0.0", directoryPath: Path.Combine(DiscoveryPath, "NormalModule"));

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Single(modules);
        Assert.Equal("NormalModule", modules[0].Id);
    }

    [Fact]
    public void GetCatalog_ReadsModulesFromManifests()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Single(modules);
        Assert.Equal("TestModule", modules[0].Id);
    }

    [Fact]
    public void GetCatalog_DiscoversMultipleModules()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        WriteManifest("ModuleB", "2.0.0");
        WriteManifest("ModuleC", "3.0.0");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Equal(3, modules.Count);
        Assert.Contains(modules, x => x.Id == "ModuleA");
        Assert.Contains(modules, x => x.Id == "ModuleB");
        Assert.Contains(modules, x => x.Id == "ModuleC");
    }

    [Fact]
    public void GetCatalog_SetsFullPhysicalPath()
    {
        // Arrange
        var moduleDirectory = Path.Combine(DiscoveryPath, "TestModule");
        WriteManifest("TestModule", "1.0.0", directoryPath: moduleDirectory);

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        var module = modules.First(x => x.Id == "TestModule");
        Assert.Equal(moduleDirectory, module.FullPhysicalPath);
    }

    [Fact]
    public void GetCatalog_ModuleWithoutAssemblyFile_StateIsInitialized()
    {
        // Arrange
        WriteManifest("NoAssembly", "1.0.0");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        var module = modules.First(x => x.Id == "NoAssembly");
        Assert.Equal(ModuleState.Initialized, module.State);
    }

    [Fact]
    public void GetCatalog_ModuleWithAssemblyFile_SetsRef()
    {
        // Arrange
        WriteManifest("WithAssembly", "1.0.0", assemblyFile: "WithAssembly.dll");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        var module = modules.First(x => x.Id == "WithAssembly");
        Assert.NotNull(module.Ref);
        Assert.Contains("WithAssembly.dll", module.Ref);
    }

    [Fact]
    public void GetCatalog_CreatesProbingPathIfNotExists()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");
        Assert.False(Directory.Exists(ProbingPath));

        // Act
        var catalog = GetLocalModuleCatalog();

        // Assert
        Assert.True(Directory.Exists(ProbingPath));
    }

    // --- Reload ---

    [Fact]
    public void Reload_PicksUpNewModules()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        var catalog = GetLocalModuleCatalog();
        var modules1 = catalog.Modules.OfType<ManifestModuleInfo>().ToList();
        Assert.Single(modules1);

        // Add another module
        WriteManifest("ModuleB", "1.0.0");

        // Act
        catalog.Reload();
        var modules2 = catalog.Modules.OfType<ManifestModuleInfo>().ToList();

        // Assert
        Assert.Equal(2, modules2.Count);
    }

    // --- Dependency validation ---

    [Fact]
    public void IsDependenciesValid_NoDependencies_ReturnsTrue()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsDependenciesValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDependenciesValid_AllDependenciesSatisfied_ReturnsTrue()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        WriteManifest("ModuleB", "1.0.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsDependenciesValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDependenciesValid_MissingDependency_ReturnsFalse()
    {
        // Arrange
        WriteManifest("ModuleB", "1.0.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsDependenciesValid();

        // Assert
        Assert.False(result);
    }
}
