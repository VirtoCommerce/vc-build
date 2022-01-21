using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.Build.PlatformTools.Azure;
using VirtoCommerce.Build.PlatformTools.Github;

namespace VirtoCommerce.Build.PlatformTools
{
    public class ModuleSourceConverter: JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new ModuleSourceSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ModuleSource));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var sourceName = jo[nameof(ModuleSource.Name)].Value<string>();
            switch (sourceName)
            {
                case nameof(GithubReleases):
                    return JsonConvert.DeserializeObject<GithubReleases>(jo.ToString(), SpecifiedSubclassConversion);
                case nameof(AzurePipelineArtifacts):
                    return JsonConvert.DeserializeObject<AzurePipelineArtifacts>(jo.ToString(), SpecifiedSubclassConversion);
                case nameof(AzureUniversalPackages):
                    return JsonConvert.DeserializeObject<AzureUniversalPackages>(jo.ToString(), SpecifiedSubclassConversion);
                case nameof(GithubPrivateRepos):
                    return JsonConvert.DeserializeObject<GithubPrivateRepos>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new TypeLoadException($"Unknown module source: {sourceName}");
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }

    public class ModuleSourceSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(ModuleSource).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
}

