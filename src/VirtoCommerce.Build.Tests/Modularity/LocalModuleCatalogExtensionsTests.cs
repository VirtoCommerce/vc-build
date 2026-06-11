using PlatformTools.Extensions;

namespace VirtoCommerce.Build.Tests.Modularity;

public class LocalModuleCatalogExtensionsTests : ModularityTestsBase
{
    [Fact]
    public void IsModuleSymlinked_NonexistentModule_ReturnsFalse()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsModuleSymlinked("NonExistentModule");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsModuleSymlinked_RegularDirectory_ReturnsFalse()
    {
        // Arrange
        WriteManifest("TestModule", "1.0.0");
        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsModuleSymlinked("TestModule");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsModuleSymlinked_SymlinkedDirectory_ReturnsTrue()
    {
        // Arrange
        var realModulePath = Path.Combine(TestRoot, "real-module");
        WriteManifest("TestModule", "1.0.0", directoryPath: realModulePath);

        var symlinkPath = Path.Combine(DiscoveryPath, "SymlinkedModule");

        try
        {
            Directory.CreateSymbolicLink(symlinkPath, realModulePath);
        }
        catch (IOException)
        {
            Assert.Skip("Creating symlinks requires elevated privileges on Windows");
            return;
        }

        var catalog = GetLocalModuleCatalog();

        // Act
        var result = catalog.IsModuleSymlinked("TestModule");

        // Assert
        Assert.True(result);
    }
}
