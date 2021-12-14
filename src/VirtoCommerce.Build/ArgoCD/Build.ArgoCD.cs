using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using ArgoCD.Client;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using VirtoCommerce.Build.ArgoCD.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json.Serialization;
using System.Linq;
using ArgoCD.Client.Models;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
        [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }
        [Parameter("Config file for Argo Application Service")] public string ArgoConfigFile { get; set; }
        Target ArgoUpdateEnvironment => _ => _
        .Executes(async () =>
        {
            if (string.IsNullOrEmpty(ArgoToken))
            {
                ArgoToken = Environment.GetEnvironmentVariable("ARGO_TOKEN");
            }
            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var rawYaml = await File.ReadAllTextAsync(ArgoConfigFile);
            var apps = deserializer.Deserialize<IList<ArgoApplication>>(rawYaml);
            var httpClient = new HttpClient();
            var argoServerUrl = new Uri(ArgoServer);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ArgoToken);
            httpClient.BaseAddress = argoServerUrl;
            var argoClient = new ArgoCDClient(httpClient, true);
            argoClient.BaseUri = argoServerUrl;
            foreach(var app in apps)
            {
                var argoApp = await argoClient.ApplicationService.GetAsync(app.Name);
                var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                var parametersToDelete = argoAppParams.Where(p => p.Name.StartsWith("platform.config.")
                    || p.Name.StartsWith("platform.secret_config.")
                    || p.Name.StartsWith("platform.secrets")
                    || p.Name.StartsWith("platform.image.tag")
                    || p.Name.StartsWith("platform.tier"));
                argoAppParams = argoAppParams.Except(parametersToDelete).ToList();
                var configs = app.Config.Select(c => new ConfigHelmParameter(c.Key, c.Value));
                var secretConfigs = app.SecretConfig.Select(c => new SecretConfigHelmParameter(c.Key, c.Value));
                var secrets = secretConfigs.DistinctBy(s => s.Value).Select(s => new SecretHelmParameter(s.Value));
                argoAppParams = argoAppParams.Concat(configs)
                    .Concat(secretConfigs)
                    .Concat(secrets)
                    .ToList();
                if (!string.IsNullOrEmpty(app.ImageTag))
                    argoAppParams.Add(new ImageTagHelmParameter(app.ImageTag));
                if (!string.IsNullOrEmpty(app.Tier))
                    argoAppParams.Add(new TierHelmParameter(app.Tier));
                argoApp.Spec.Source.Helm.Parameters = argoAppParams;
                await argoClient.ApplicationService.UpdateSpecAsync(app.Name, argoApp.Spec);
            }
        });
    }
}

