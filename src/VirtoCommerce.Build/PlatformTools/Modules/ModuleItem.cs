using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PlatformTools.Modules
{
    public class ModuleItem
    {
        public ModuleItem(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; set; }
        public string Version { get; set; }

        /// <summary>
        /// Properties unknown to vc-build, kept so that saving the manifest writes them back.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
