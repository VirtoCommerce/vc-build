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
        var manifestJson = CreateExternalManifestJson("TestModule", "1.0.0", "https://example.com/TestModule_1.0.0.zip");

        // Act
        var catalog1 = await GetExternalModuleCatalog("https://example.com/modules.json", manifestJson);
        var catalog2 = await GetExternalModuleCatalog("https://example.com/modules.json", manifestJson);

        // Assert
        Assert.Same(catalog1, catalog2);
    }

    [Fact]
    public async Task GetCatalog_LoadsExternalModules()
    {
        // Arrange
        var manifestJson = CreateExternalManifestJson("ExternalModule", "1.0.0", "https://example.com/ExternalModule_1.0.0.zip");

        // Act
        var catalog = await GetExternalModuleCatalog("https://example.com/modules.json", manifestJson);
        var modules = catalog.Modules;

        // Assert
        Assert.Single(modules);
        Assert.Equal("ExternalModule", modules[0].Id);
    }

    [Fact]
    public async Task GetCatalog_MergesExternalWithLocal()
    {
        // Arrange
        WriteManifest("LocalModule", "1.0.0");
        var manifestJson = CreateExternalManifestJson("ExternalModule", "2.0.0", "https://example.com/ExternalModule_2.0.0.zip");

        // Act
        var catalog = await GetExternalModuleCatalog("https://example.com/modules.json", manifestJson);
        var modules = catalog.Modules;

        // Assert
        Assert.Equal(2, modules.Count);
        Assert.Contains(modules, x => x.Id == "LocalModule" && x.IsInstalled);
        Assert.Contains(modules, x => x.Id == "ExternalModule" && !x.IsInstalled);
    }

    [Fact]
    public async Task GetCatalog_NoManifestUrl_ReturnsOnlyInstalled()
    {
        // Arrange
        WriteManifest("LocalModule", "1.0.0");

        // Act
        var catalog = await GetExternalModuleCatalog();
        var modules = catalog.Modules;

        // Assert
        Assert.Single(modules);
        Assert.Equal("LocalModule", modules[0].Id);
    }


    private static string CreateExternalManifestJson(string id, string version, string packageUrl)
    {
        var manifest = new
        {
            Id = id,
            Title = id,
            Description = "Test",
            Authors = new[] { "Test" },
            Versions = new[]
            {
                new
                {
                    Version = version,
                    VersionTag = "",
                    PlatformVersion = "3.800.0",
                    PackageUrl = packageUrl,
                    ReleaseNotes = "",
                }
            }
        };

        var manifests = new[] { manifest };

        return JsonSerializer.Serialize(manifests);
    }

    private async Task<ExtModuleCatalog> GetExternalModuleCatalog(string? modulesManifestUrl = null, string? manifestJson = null)
    {
        var manifestUrls = modulesManifestUrl is null ? null : new[] { modulesManifestUrl };
        var localModuleCatalog = GetLocalModuleCatalog();
        var httpClient = CreateFakeHttpClient(manifestJson);

        var externalCatalog = await ExtModuleCatalog.GetCatalog(authToken: null, manifestUrls, localModuleCatalog, httpClient);

        return externalCatalog;
    }

    private static HttpClient CreateFakeHttpClient(string? responseJson)
    {
        var handler = new FakeHttpMessageHandler(responseJson);
        return new HttpClient(handler);
    }

    private class FakeHttpMessageHandler(string? responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
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
