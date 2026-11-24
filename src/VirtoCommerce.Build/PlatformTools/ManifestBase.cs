using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.Build.PlatformTools
{
    public class ManifestBase
    {
        // Negative orders put these before all other properties (default order is -1) in newly created files;
        // existing files keep their own property order on save
        private const int ManifestVersionOrder = -3;
        private const int PlatformVersionOrder = -2;

        [JsonProperty(Order = ManifestVersionOrder)]
        public string ManifestVersion { get; set; }

        [JsonProperty(Order = PlatformVersionOrder)]
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Properties unknown to vc-build (e.g. PlatformImage, PlatformImageTag), kept so that saving the manifest writes them back.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
