using Extensions;
using PlatformTools;
using PlatformTools.Modules;
using VirtoCommerce.Build.PlatformTools;

namespace VirtoCommerce.Build.Tests
{
    public class PackageManagerTests
    {
        [Fact]
        public void CreatePackageManifest_WithPlatformVersion_ReturnsValidManifest()
        {
            // Arrange
            var platformVersion = "1.0.0";
            // Act
            var manifest = (MixedPackageManifest)PackageManager.CreatePackageManifest(platformVersion);

            // Assert
            Assert.NotNull(manifest);
            Assert.Equal("2.0", manifest.ManifestVersion);
            Assert.Equal(platformVersion, manifest.PlatformVersion);
        }

        [Fact]
        public void CreatePackageManifest_WithPlatformVersionAndPlatformAssetsUrl_ReturnsValidManifest()
        {
            // Arrange
            var platformVersion = "3.2.1";
            var platformAssetsUrl =
                "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3_alt.json";
            // Act
            var manifest = (MixedPackageManifest)PackageManager.CreatePackageManifest(platformVersion, platformAssetsUrl);

            // Assert
            Assert.NotNull(manifest);
            Assert.Equal("2.0", manifest.ManifestVersion);
            Assert.Equal(platformVersion, manifest.PlatformVersion);
            Assert.Equal(platformAssetsUrl, manifest.PlatformAssetUrl);
        }

        [Fact]
        public void CreatePackageManifest_WithPlatformVersionAndPlatformAssetsUrl_NoAssetsUrlWhenNullOrEmpty()
        {
            // Arrange
            var platformVersion = "3.2.1";
            var platformAssetsUrl =
                string.Empty;
            // Act
            var manifest = (MixedPackageManifest)PackageManager.CreatePackageManifest(platformVersion, platformAssetsUrl);

            // Assert
            Assert.NotNull(manifest);
            Assert.Equal("2.0", manifest.ManifestVersion);
            Assert.Equal(platformVersion, manifest.PlatformVersion);
            Assert.Null(manifest.PlatformAssetUrl);
        }

        [Fact]
        public void UpdatePlatform_UpdatesPlatformVersion()
        {
            // Arrange
            var oldVersion = "1.0.0";
            var newVersion = "1.1.0";
            var manifest = PackageManager.CreatePackageManifest(oldVersion);

            // Act
            PackageManager.UpdatePlatform((MixedPackageManifest)manifest, newVersion);

            // Assert
            Assert.Equal(newVersion, manifest.PlatformVersion);
        }
        
        [Fact]
        public void ToFile_SavesManifestToFile()
        {
            // Arrange
            var manifest = PackageManager.CreatePackageManifest("1.0.0");
            var path = "./test-vc-package.json";

            // Act
            PackageManager.ToFile(manifest, path.ToAbsolutePath());

            // Assert
            Assert.True(File.Exists(path));

            // Cleanup
            File.Delete(path);
        }

        [Fact]
        public void FromFile_LoadsManifestFromFile()
        {
            // Arrange
            var manifest = PackageManager.CreatePackageManifest("1.0.0");
            var path = "./test-vc-package.json";
            PackageManager.ToFile(manifest, path.ToAbsolutePath());

            // Act
            var loadedManifest = PackageManager.FromFile(path);

            // Assert
            Assert.Equal(manifest.PlatformVersion, loadedManifest.PlatformVersion);
            Assert.Equal(manifest.ManifestVersion, loadedManifest.ManifestVersion);

            // Cleanup
            File.Delete(path);
        }

        [Fact]
        public void GetModuleSources_ReturnsModuleSources()
        {
            // Arrange
            var manifest = PackageManager.CreatePackageManifest("1.0.0");

            // Act
            var sources = PackageManager.GetModuleSources(manifest);

            // Assert
            Assert.Single(sources);
            Assert.IsType<GithubReleases>(sources[0]);
        }

        [Fact]
        public void GetGithubModulesSource_ReturnsGithubModulesSource()
        {
            // Arrange
            var manifest = PackageManager.CreatePackageManifest("1.0.0");

            // Act
            var githubModulesSource = PackageManager.GetGithubModulesSource(manifest);

            // Assert
            Assert.NotNull(githubModulesSource);
            Assert.IsType<GithubReleases>(githubModulesSource);
        }

        [Fact]
        public void GetGithubModuleManifests_ReturnsGithubModuleManifests()
        {
            // Arrange
            var manifest = PackageManager.CreatePackageManifest("1.0.0");

            // Act
            var githubModuleManifests = PackageManager.GetGithubModuleManifests(manifest);
            // Assert
            Assert.NotNull(githubModuleManifests);
        }

        [Fact]
        public void GetGithubModules_ReturnsGithubModules()
        {
            // Arrange
            var manifest = (MixedPackageManifest)PackageManager.CreatePackageManifest("1.0.0");
            var moduleId = "TestModule";
            var moduleVersion = "7.0.0";
            var source = manifest.Sources.Where(s => s.Name == nameof(GithubReleases)).OfType<GithubReleases>().FirstOrDefault();
            Assert.NotNull(source);
            source.Modules.Add(new ModuleItem(moduleId, moduleVersion));

            // Act
            var githubModulesSource = PackageManager.GetGithubModules(manifest);

            // Assert
            Assert.NotNull(githubModulesSource);
            Assert.NotEmpty(githubModulesSource);
            var testModule = githubModulesSource.SingleOrDefault(m => m.Id == moduleId);
            Assert.NotNull(testModule);
            Assert.Equal(moduleVersion, testModule.Version);
        }
    }
}
