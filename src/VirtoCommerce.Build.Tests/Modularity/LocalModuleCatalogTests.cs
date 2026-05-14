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
        var modules = catalog.Modules;

        // Assert
        Assert.Empty(modules);
    }

    [Fact]
    public void GetCatalog_EmptyDiscoveryDirectory_LoadsNoModules()
    {
        // Arrange: empty discovery directory, no manifests

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules;

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
        var modules = catalog.Modules;

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
        var modules = catalog.Modules;

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
        var modules = catalog.Modules;

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
        var modules = catalog.Modules;

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
        var modules = catalog.Modules;

        // Assert
        var module = modules.First(x => x.Id == "NoAssembly");
        Assert.Equal(ModuleState.Initialized, module.State);
    }

    [Fact]
    public void GetCatalog_ModuleWithAssemblyFile_SetsAssemblyFile()
    {
        // Arrange
        WriteManifest("WithAssembly", "1.0.0", assemblyFile: "WithAssembly.dll");

        // Act
        var catalog = GetLocalModuleCatalog();
        var modules = catalog.Modules;

        // Assert
        var module = modules.First(x => x.Id == "WithAssembly");
        Assert.NotNull(module.AssemblyFile);
        Assert.Contains("WithAssembly.dll", module.AssemblyFile);
    }

    [Fact]
    public void GetCatalog_CreatesProbingPathIfNotExists()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");
        Assert.False(Directory.Exists(ProbingPath));

        // Act
        var catalog = GetLocalModuleCatalog();
        var exists1 = Directory.Exists(ProbingPath);
        catalog.RefreshProbingDirectory();
        var exists2 = Directory.Exists(ProbingPath);

        // Assert
        Assert.False(exists1);
        Assert.True(exists2);
    }

    // --- Reload ---

    [Fact]
    public void Reload_PicksUpNewModules()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        var catalog = GetLocalModuleCatalog();
        var modules1 = catalog.Modules;
        Assert.Single(modules1);

        // Add another module
        WriteManifest("ModuleB", "1.0.0");

        // Act
        catalog.Reload();
        var modules2 = catalog.Modules;

        // Assert
        Assert.Equal(2, modules2.Count);
    }

    // --- Dependency validation ---

    [Fact]
    public void ValidateDependencies_NoDependencies_ReturnsTrue()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0", platformVersion: "3.1000.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDependencies_AllDependenciesSatisfied_ReturnsTrue()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0", platformVersion: "3.1000.0");
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateDependencies_MissingDependency_ReturnsFalse()
    {
        // Arrange
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateDependencies_IncompatiblePlatform_ReturnsFalse()
    {
        // Arrange
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.ValidateDependencies(platformVersion: "3.800.0");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetCatalog_ModuleWithDependencies_ParsesDependencyList()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0");
        WriteManifest("ModuleB", "1.0.0", dependencies: ["ModuleA:1.0.0"]);

        // Act
        var catalog = GetLocalModuleCatalog();
        var moduleB = catalog.Modules.First(x => x.Id == "ModuleB");

        // Assert
        Assert.NotEmpty(moduleB.Dependencies);
        Assert.Contains(moduleB.Dependencies, d => d.Id == "ModuleA");
    }

    [Fact]
    public void GetCatalog_ModuleWithPlatformVersion_ParsesPlatformVersion()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0", platformVersion: "3.999.0");

        // Act
        var catalog = GetLocalModuleCatalog();
        var module = catalog.Modules.First(x => x.Id == "ModuleA");

        // Assert
        Assert.NotNull(module.PlatformVersion);
        Assert.Equal("3.999.0", module.PlatformVersion.ToString());
    }

    [Fact]
    public void ValidateDependencies_CircularDependency_ReturnsFalse()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0", platformVersion: "3.1000.0", dependencies: ["ModuleB:1.0.0"]);
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateDependencies_MissingDependency_ModuleHasErrors()
    {
        // Arrange
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0", dependencies: ["ModuleA:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        var moduleB = catalog.Modules.First(x => x.Id == "ModuleB");
        Assert.NotEmpty(moduleB.Errors);
    }

    [Fact]
    public void ValidateDependencies_ErrorDoesNotBlockOtherModules()
    {
        // Arrange
        WriteManifest("ModuleA", "1.0.0", platformVersion: "3.1000.0");
        WriteManifest("ModuleB", "1.0.0", platformVersion: "3.1000.0", dependencies: ["Missing:1.0.0"]);
        var catalog = GetLocalModuleCatalog();

        // Act
        catalog.ValidateDependencies(platformVersion: "3.1000.0");

        // Assert
        var moduleA = catalog.Modules.First(x => x.Id == "ModuleA");
        Assert.Empty(moduleA.Errors);
    }

    [Fact]
    public void RefreshProbingDirectory_CopiesAssembliesToProbingPath()
    {
        // Arrange
        var moduleDir = Path.Combine(DiscoveryPath, "TestModule");
        WriteManifest("TestModule", "1.0.0", directoryPath: moduleDir, assemblyFile: "TestModule.dll");

        var dllSource = Path.Combine(moduleDir, "TestModule.dll");
        File.WriteAllBytes(dllSource, [0x4D, 0x5A]); // MZ header

        var catalog = GetLocalModuleCatalog();

        // Act
        catalog.RefreshProbingDirectory();

        // Assert
        Assert.True(Directory.Exists(ProbingPath));
    }

    [Fact]
    public void RefreshProbingDirectory_IsIdempotent()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        catalog.RefreshProbingDirectory();
        catalog.RefreshProbingDirectory();

        // Assert
        Assert.True(Directory.Exists(ProbingPath));
    }
}
