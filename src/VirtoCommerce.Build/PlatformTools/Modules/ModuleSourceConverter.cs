using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PlatformTools.Modules.Azure;
using PlatformTools.Modules.Github;
using PlatformTools.Modules.Gitlab;
using PlatformTools.Modules.LocalModules;
using VirtoCommerce.Build.PlatformTools;

namespace PlatformTools.Modules
{
    public class ModuleSourceConverter : JsonConverter
    {
        private static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new ModuleSourceSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ModuleSource);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
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
                case nameof(AzureBlob):
                    return JsonConvert.DeserializeObject<AzureBlob>(jo.ToString(), SpecifiedSubclassConversion);
                case nameof(GitlabJobArtifacts):
                    return JsonConvert.DeserializeObject<GitlabJobArtifacts>(jo.ToString(),
                        SpecifiedSubclassConversion);
                case nameof(Local):
                    return JsonConvert.DeserializeObject<Local>(jo.ToString(), SpecifiedSubclassConversion);
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
            {
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            }

            return base.ResolveContractConverter(objectType);
        }
    }
}
