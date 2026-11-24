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
        private static readonly JsonSerializer _concreteSourceSerializer = JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new ModuleSourceSpecifiedConcreteClassConverter() });

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ModuleSource);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var sourceName = jo.GetValue(nameof(ModuleSource.Name), StringComparison.OrdinalIgnoreCase)?.Value<string>();
            var sourceType = sourceName switch
            {
                GithubReleases.SourceName => typeof(GithubReleases),
                AzurePipelineArtifacts.SourceName => typeof(AzurePipelineArtifacts),
                AzureUniversalPackages.SourceName => typeof(AzureUniversalPackages),
                GithubPrivateRepos.SourceName => typeof(GithubPrivateRepos),
                AzureBlob.SourceName => typeof(AzureBlob),
                GitlabJobArtifacts.SourceName => typeof(GitlabJobArtifacts),
                LocalModuleSource.SourceName => typeof(LocalModuleSource),
                _ => throw new TypeLoadException($"Unknown module source: {sourceName}"),
            };

            // Convert the already read tokens instead of re-parsing their text, so values keep the reader's parse settings
            return jo.ToObject(sourceType, _concreteSourceSerializer);
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
