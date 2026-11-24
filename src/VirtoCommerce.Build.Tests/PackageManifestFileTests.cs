using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlatformTools;
using PlatformTools.Modules;

namespace VirtoCommerce.Build.Tests
{
    public sealed class PackageManifestFileTests : IDisposable
    {
        // Shape of a real deployment manifest: unknown properties at the top level, inside a source and inside a module,
        // unsorted modules (one AzureBlob entry has no Id), uneven whitespace.
        private const string _manifestJson = """
            {
              "ManifestVersion": "2.0",
              "PlatformVersion": "3.1100.0",
              "PlatformImage": "ghcr.io/virtocommerce/platform",
              "PlatformImageTag": "3.1100.0",
              "PlatformAssetUrl": "",
              "Sources": [
                {
                  "Name": "AzureBlob",
                  "Container": "packages",
                  "Comment": "keep me too",
                  "ServiceUri": "https://vc3prerelease.blob.core.windows.net",
                  "Modules": [
                    {
                      "BlobName": "VirtoCommerce.Xapi_3.1000.0.zip"
                    },
                    {
                      "Id": "VirtoCommerce.Assets",
                      "BlobName": "VirtoCommerce.Assets_3.1000.0.zip"
                    }
                  ]
                },
                {
                  "Name": "GithubReleases",
                  "ModuleSources": [
                    "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json"
                  ],
                  "Modules": [
                    {
                      "Id": "VirtoCommerce.SalesRep",
                      "Version": "3.1026.0"
                    },
                    {
                      "Id": "VirtoCommerce.Core",
                      "Comment": "keep me",
                      "Version": "3.1013.0"
                    },
                      {
                       "Id": "VirtoCommerce.Cart",
                       "Version": "3.1015.0"
                    }
                  ]
                }
              ]
            }

            """;

        // Read values exactly as written, so a reformatted date or number fails the comparison
        private static readonly JsonSerializerSettings _verbatimSettings = new()
        {
            DateParseHandling = DateParseHandling.None,
            FloatParseHandling = FloatParseHandling.Decimal,
        };

        private readonly string _path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        public PackageManifestFileTests()
        {
            File.WriteAllText(_path, _manifestJson);
        }

        public void Dispose()
        {
            File.Delete(_path);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ToFile_UnchangedManifest_LeavesFileUntouched(bool sortModules)
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);

            // Act
            PackageManager.ToFile(manifest, _path, sortModules);

            // Assert
            Assert.Equal(_manifestJson, File.ReadAllText(_path));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ToFile_ChangedModuleVersion_PreservesUnknownPropertiesAndOrder(bool sortModules)
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);
            PackageManager.GetGithubModules(manifest).Single(module => module.Id == "VirtoCommerce.Core").Version = "3.1014.0";

            // Act
            PackageManager.ToFile(manifest, _path, sortModules);

            // Assert
            AssertFileEquals(sortModules, expected => expected.SelectToken("$.Sources[1].Modules[?(@.Id == 'VirtoCommerce.Core')].Version")!.Replace("3.1014.0"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ToFile_RemovedModule_RemovesOnlyThatModule(bool sortModules)
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);
            PackageManager.GetGithubModules(manifest).RemoveAll(module => module.Id == "VirtoCommerce.Cart");

            // Act
            PackageManager.ToFile(manifest, _path, sortModules);

            // Assert
            AssertFileEquals(sortModules, expected => expected.SelectToken("$.Sources[1].Modules[?(@.Id == 'VirtoCommerce.Cart')]")!.Remove());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ToFile_AddedModule_AddsOnlyThatModule(bool sortModules)
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);
            PackageManager.GetGithubModules(manifest).Add(new ModuleItem("VirtoCommerce.Search", "3.1015.0"));

            // Act
            PackageManager.ToFile(manifest, _path, sortModules);

            // Assert
            AssertFileEquals(sortModules, expected => ((JArray)expected.SelectToken("$.Sources[1].Modules")!).Add(new JObject
            {
                ["Id"] = "VirtoCommerce.Search",
                ["Version"] = "3.1015.0",
            }));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ToFile_SavedAgainWithoutChanges_LeavesFileUntouched(bool sortModules)
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);
            PackageManager.GetGithubModules(manifest).Add(new ModuleItem("VirtoCommerce.Search", "3.1015.0"));
            PackageManager.ToFile(manifest, _path, sortModules);
            var firstSave = File.ReadAllText(_path);

            // Act
            PackageManager.ToFile(PackageManager.FromFile(_path), _path, sortModules);

            // Assert
            Assert.Equal(firstSave, File.ReadAllText(_path));
        }

        [Fact]
        public void ToFile_ChangedManifestWithSorting_SortsModulesByIdThenBlobNameIgnoringCase()
        {
            // Arrange
            var manifest = PackageManager.FromFile(_path);
            PackageManager.GetGithubModules(manifest).Add(new ModuleItem("virtocommerce.Catalog", "3.1052.0"));

            // Act
            PackageManager.ToFile(manifest, _path, sortModules: true);

            // Assert
            var actual = JObject.Parse(File.ReadAllText(_path));
            Assert.Equal(
                ["VirtoCommerce.Assets", "VirtoCommerce.Xapi_3.1000.0.zip"],
                actual.SelectTokens("$.Sources[0].Modules[*]").Select(module => (string)module["Id"] ?? (string)module["BlobName"]));
            Assert.Equal(
                ["VirtoCommerce.Cart", "virtocommerce.Catalog", "VirtoCommerce.Core", "VirtoCommerce.SalesRep"],
                actual.SelectTokens("$.Sources[1].Modules[*].Id").Select(id => (string)id));
        }

        [Fact]
        public void ToFile_ChangedManifest_KeepsUnknownDatesAndNumbersAsWritten()
        {
            // Arrange
            var manifestJson = _manifestJson
                .Replace("\"PlatformImage\":", "\"BuildDate\": \"2024-01-01T00:00:00+03:00\",\n  \"PlatformImage\":")
                .Replace("\"Comment\": \"keep me too\"", "\"Ratio\": 1.10")
                .Replace("\"Comment\": \"keep me\"", "\"Released\": \"2024-01-01T00:00:00.1200000Z\"");
            File.WriteAllText(_path, manifestJson);
            var manifest = PackageManager.FromFile(_path);
            manifest.PlatformVersion = "3.1101.0";

            // Act
            PackageManager.ToFile(manifest, _path);

            // Assert
            var actual = File.ReadAllText(_path);
            Assert.Contains("\"BuildDate\": \"2024-01-01T00:00:00+03:00\"", actual);
            Assert.Contains("\"Ratio\": 1.10", actual);
            Assert.Contains("\"Released\": \"2024-01-01T00:00:00.1200000Z\"", actual);
        }

        [Fact]
        public void ToFile_ChangedV1Manifest_PreservesUnknownPropertiesAndSortsModules()
        {
            // Arrange
            File.WriteAllText(_path, """
                {
                  "PlatformVersion": "3.1100.0",
                  "Custom": "keep me",
                  "ModuleSources": [
                    "https://raw.githubusercontent.com/VirtoCommerce/vc-modules/master/modules_v3.json"
                  ],
                  "Modules": [
                    {
                      "Id": "VirtoCommerce.Core",
                      "Version": "3.1013.0"
                    },
                    {
                      "Id": "VirtoCommerce.Cart",
                      "Version": "3.1015.0"
                    }
                  ]
                }
                """);
            var manifest = PackageManager.FromFile(_path);
            manifest.PlatformVersion = "3.1101.0";

            // Act
            PackageManager.ToFile(manifest, _path, sortModules: true);

            // Assert
            var actual = JObject.Parse(File.ReadAllText(_path));
            Assert.Equal(["PlatformVersion", "Custom", "ModuleSources", "Modules"], actual.Properties().Select(property => property.Name));
            Assert.Equal("3.1101.0", (string)actual["PlatformVersion"]);
            Assert.Equal(["VirtoCommerce.Cart", "VirtoCommerce.Core"], actual.SelectTokens("$.Modules[*].Id").Select(id => (string)id));
        }

        [Fact]
        public void ToFile_NewFile_WritesVersionsFirst()
        {
            // Arrange
            File.Delete(_path);
            var manifest = PackageManager.CreatePackageManifest("3.1100.0");

            // Act
            PackageManager.ToFile(manifest, _path);

            // Assert
            var propertyNames = JObject.Parse(File.ReadAllText(_path)).Properties().Select(property => property.Name);
            Assert.Equal(["ManifestVersion", "PlatformVersion", "Sources"], propertyNames);
        }

        [Fact]
        public void OverwriteFile_CorruptFile_ReplacesIt()
        {
            // Arrange
            File.WriteAllText(_path, "{ broken");
            var manifest = PackageManager.CreatePackageManifest("3.1100.0");

            // Act
            PackageManager.OverwriteFile(manifest, _path);

            // Assert
            var propertyNames = JObject.Parse(File.ReadAllText(_path)).Properties().Select(property => property.Name);
            Assert.Equal(["ManifestVersion", "PlatformVersion", "Sources"], propertyNames);
        }

        [Fact]
        public void FromFile_LowercaseSourceName_LoadsSource()
        {
            // Arrange
            File.WriteAllText(_path, """
                {
                  "ManifestVersion": "2.0",
                  "PlatformVersion": "3.1100.0",
                  "Sources": [
                    {
                      "name": "GithubReleases",
                      "ModuleSources": [],
                      "Modules": [
                        {
                          "Id": "VirtoCommerce.Core",
                          "Version": "3.1013.0"
                        }
                      ]
                    }
                  ]
                }
                """);

            // Act
            var manifest = PackageManager.FromFile(_path);

            // Assert
            Assert.Equal("VirtoCommerce.Core", Assert.Single(PackageManager.GetGithubModules(manifest)).Id);
        }

        /// <summary>
        /// Compares the saved file with the original manifest after <paramref name="updateExpected"/>, including property order
        /// and the exact text of values; whitespace is normalized. With <paramref name="sortModules"/>, modules are expected sorted.
        /// </summary>
        private void AssertFileEquals(bool sortModules, Action<JObject> updateExpected)
        {
            var expected = ParseVerbatim(_manifestJson);
            updateExpected(expected);
            if (sortModules)
            {
                foreach (var modules in expected.SelectTokens("$.Sources[*].Modules").OfType<JArray>())
                {
                    modules.ReplaceAll(modules.OrderBy(module => (string)module["Id"] ?? (string)module["BlobName"], StringComparer.OrdinalIgnoreCase).ToList());
                }
            }

            Assert.Equal(expected.ToString(), ParseVerbatim(File.ReadAllText(_path)).ToString());
        }

        private static JObject ParseVerbatim(string json)
        {
            return JsonConvert.DeserializeObject<JObject>(json, _verbatimSettings);
        }
    }
}
