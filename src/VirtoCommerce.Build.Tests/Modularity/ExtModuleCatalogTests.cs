using System.Net;
using System.Text;
using System.Text.Json;
using PlatformTools.Modules;

namespace VirtoCommerce.Build.Tests.Modularity;

public class ExtModuleCatalogTests : ModularityTestsBase
{
    [Fact]
    public async Task GetCatalog_ReturnsSameInstance_OnSecondCall()
    {
        // Arrange
        var manifestJson = CreateCartManifestJson();

        // Act
        var catalog1 = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson);
        var catalog2 = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson);

        // Assert
        Assert.Same(catalog1, catalog2);
    }

    [Fact]
    public async Task GetCatalog_LoadsExternalModules()
    {
        // Arrange
        var manifestJson = CreateCartManifestJson();

        // Act
        var catalog = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson);
        var modules = catalog.Modules;

        // Assert
        Assert.Single(modules);
        Assert.Equal("VirtoCommerce.Cart", modules[0].Id);
    }

    [Fact]
    public async Task GetCatalog_MergesExternalWithLocal()
    {
        // Arrange
        WriteManifest("VirtoCommerce.Cart", "3.1003.0");
        var manifestJson = CreateInventoryManifestJson();

        // Act
        var catalog = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson);
        var modules = catalog.Modules;

        // Assert
        Assert.Equal(2, modules.Count);
        Assert.Contains(modules, x => x.Id == "VirtoCommerce.Cart" && x.IsInstalled);
        Assert.Contains(modules, x => x.Id == "VirtoCommerce.Inventory" && !x.IsInstalled);
    }

    [Fact]
    public async Task GetCatalog_NoManifestUrl_ReturnsOnlyInstalled()
    {
        // Arrange
        WriteManifest("VirtoCommerce.Cart", "3.1003.0");

        // Act
        var catalog = await GetExternalModuleCatalog();
        var modules = catalog.Modules;

        // Assert
        Assert.Single(modules);
        Assert.Equal("VirtoCommerce.Cart", modules[0].Id);
    }

    [Fact]
    public async Task GetCatalog_NoDuplicateModules_WhenModuleInBothSources()
    {
        // Arrange
        WriteManifest("VirtoCommerce.Cart", "3.1003.0");
        var manifestJson = CreateCartManifestJson();

        // Act
        var catalog = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson);
        var modules = catalog.Modules;

        // Assert
        var cartModules = modules.Where(x => x.Id == "VirtoCommerce.Cart").ToList();
        Assert.Single(cartModules);
        Assert.Equal("3.1003.0", cartModules[0].Version.ToString());
        Assert.True(cartModules[0].IsInstalled);
    }

    [Fact]
    public async Task GetCatalog_HttpError_ReturnsOnlyInstalledModules()
    {
        // Arrange
        WriteManifest("VirtoCommerce.Cart", "3.1003.0");

        // Act
        var catalog = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", manifestJson: null, httpStatusCode: HttpStatusCode.NotFound);
        var modules = catalog.Modules;

        // Assert
        Assert.Single(modules);
        Assert.Equal("VirtoCommerce.Cart", modules[0].Id);
    }

    [Fact]
    public async Task GetCatalog_MultipleManifestUrls_LoadsAllModules()
    {
        // Arrange
        var combinedJson = $"[{JsonSerializer.Deserialize<JsonElement[]>(CreateCartManifestJson())![0]},{JsonSerializer.Deserialize<JsonElement[]>(CreateInventoryManifestJson())![0]}]";

        // Act
        var catalog = await GetExternalModuleCatalog("https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json", combinedJson);
        var modules = catalog.Modules;

        // Assert
        Assert.Contains(modules, x => x.Id == "VirtoCommerce.Cart");
        Assert.Contains(modules, x => x.Id == "VirtoCommerce.Inventory");
    }

    [Fact]
    public async Task GetCatalog_ExternalModule_HasPackageUrl()
    {
        // Arrange
        var manifestJson = CreateCartManifestJson();

        // Act
        var catalog = await GetExternalModuleCatalog(
            "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/refs/heads/master/modules_v3.json",
            manifestJson);
        var module = catalog.Modules.First(x => x.Id == "VirtoCommerce.Cart");

        // Assert
        Assert.NotNull(module.Ref);
        Assert.StartsWith("https://", module.Ref);
        Assert.EndsWith(".zip", module.Ref);
    }


    private static string CreateCartManifestJson() =>
        """
        [
          {
            "Id": "VirtoCommerce.Cart",
            "Title": "Shopping Cart",
            "Description": "Shopping cart / checkout functionality",
            "Authors": ["Virto Commerce"],
            "Owners": ["Virto Commerce"],
            "ProjectUrl": "https://github.com/VirtoCommerce/vc-module-cart",
            "Tags": "shopping cart",
            "Groups": ["commerce"],
            "Versions": [
              {
                "Version": "3.1003.0",
                "VersionTag": "",
                "PlatformVersion": "3.1004.0",
                "PackageUrl": "https://github.com/VirtoCommerce/vc-module-cart/releases/download/3.1003.0/VirtoCommerce.Cart_3.1003.0.zip",
                "Dependencies": [
                  { "Id": "VirtoCommerce.Assets",        "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Core",          "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Customer",      "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Notifications", "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Payment",       "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Search",        "Version": "3.1000.0", "Optional": true  },
                  { "Id": "VirtoCommerce.Shipping",      "Version": "3.1000.0", "Optional": false },
                  { "Id": "VirtoCommerce.Store",         "Version": "3.1000.0", "Optional": false }
                ],
                "ReleaseNotes": "First version."
              }
            ]
          }
        ]
        """;

    private static string CreateInventoryManifestJson() =>
        """
        [
          {
            "Id": "VirtoCommerce.Inventory",
            "Title": "Inventory",
            "Description": "Inventory management",
            "Authors": ["Virto Commerce"],
            "Owners": ["Virto Commerce"],
            "ProjectUrl": "https://github.com/VirtoCommerce/vc-module-inventory",
            "Tags": "inventory",
            "Groups": ["commerce"],
            "Versions": [
              {
                "Version": "3.1003.0",
                "VersionTag": "",
                "PlatformVersion": "3.1004.0",
                "PackageUrl": "https://github.com/VirtoCommerce/vc-module-inventory/releases/download/3.1003.0/VirtoCommerce.Inventory_3.1003.0.zip",
                "Dependencies": [
                  { "Id": "VirtoCommerce.Core", "Version": "3.1000.0", "Optional": false }
                ],
                "ReleaseNotes": "First version."
              }
            ]
          }
        ]
        """;

    private async Task<ExtModuleCatalog> GetExternalModuleCatalog(string? modulesManifestUrl = null, string? manifestJson = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        var manifestUrls = modulesManifestUrl is null ? null : new[] { modulesManifestUrl };
        var localModuleCatalog = GetLocalModuleCatalog();
        var httpClient = CreateFakeHttpClient(manifestJson, httpStatusCode);

        var externalCatalog = await ExtModuleCatalog.GetCatalog(authToken: null, manifestUrls, localModuleCatalog, httpClient);

        return externalCatalog;
    }

    private static HttpClient CreateFakeHttpClient(string? responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(responseJson, statusCode);
        return new HttpClient(handler);
    }

    private class FakeHttpMessageHandler(string? responseJson, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = responseJson is null ? null : new StringContent(responseJson, Encoding.UTF8, "application/json"),
            };
        }
    }

    protected override void Reset()
    {
        base.Reset();
        ExtModuleCatalog.Reset();
    }
}
