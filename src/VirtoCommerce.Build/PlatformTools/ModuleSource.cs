using Newtonsoft.Json;

namespace VirtoCommerce.Build.PlatformTools
{
    [JsonConverter(typeof(ModuleSourceConverter))]
    public abstract class ModuleSource
    {
        public abstract string Name { get; set; }
    }
}

