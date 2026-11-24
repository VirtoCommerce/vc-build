using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PlatformTools.Modules
{
    [JsonConverter(typeof(ModuleSourceConverter))]
    public abstract class ModuleSource
    {
        public abstract string Name { get; set; }

        /// <summary>
        /// Properties unknown to vc-build, kept so that saving the manifest writes them back.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalProperties { get; set; }
    }
}
