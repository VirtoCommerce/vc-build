using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using ArgoCD.Client;
using Nuke.Common;
using VirtoCommerce.Build.ArgoCD.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AdvancedServiceSection = VirtoCommerce.Build.ArgoCD.Models.AdvancedService;
using PlatformSection = VirtoCommerce.Build.ArgoCD.Models.Platform;
using StorefrontSection = VirtoCommerce.Build.ArgoCD.Models.Storefront;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
        [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }
        [Parameter("Config file for Argo Application Service")] public string ArgoConfigFile { get; set; }
        [Parameter("Array of Helm parameters")] public HelmParameter[] HelmParameters { get; set; }
        [Parameter("Argo Application Name")] public string ArgoAppName { get; set; }

        public Target SetHelmParameter => _ => _
            .Executes(async () =>
            {
                var argoClient = CreateArgoCDClient(ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"), new Uri(ArgoServer));
                var argoApp = await argoClient.ApplicationService.GetAsync(ArgoAppName);
                var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                foreach(var parameter in HelmParameters)
                {
                    var argoParameter = argoAppParams.FirstOrDefault(p => p.Name == parameter.Name);
                    if(argoParameter != null)
                    {
                        argoParameter.Value = parameter.Value;
                    }
                    else
                    {
                        argoAppParams.Add(parameter);
                    }
                }
                await argoClient.ApplicationService.UpdateSpecAsync(ArgoAppName, argoApp.Spec);
            });

        private static ArgoCDClient CreateArgoCDClient(string token, Uri argoServer)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.BaseAddress = argoServer;
            var argoClient = new ArgoCDClient(httpClient, true);
            argoClient.BaseUri = argoServer;
            return argoClient;
        }

        public Target ArgoUpdateApplication => _ => _
          .Executes(async () =>
          {
              var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
              var rawYaml = await File.ReadAllTextAsync(ArgoConfigFile);
              var apps = deserializer.Deserialize<IList<ArgoApplication>>(rawYaml);
              var argoServerUrl = new Uri(ArgoServer);
              var argoClient = CreateArgoCDClient(ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"), argoServerUrl);
              var sectionsToClean = new string[]
              {
                "platform.config.",
                "platform.secret_config.",
                "platform.secrets",
                "platform.image.repository",
                "platform.image.tag",
                "platform.tier",
                "storefront.config",
                "storefront.secret_config",
                "storefront.image.repository",
                "storefront.image.tag",
                "ingress.config",
                "ingress.hostname",
                "ingress.storefront_hostname",
                "theme.url",
                "theme.name",
                "advanced.service"
              };
              foreach (var app in apps)
              {
                  var argoApp = await argoClient.ApplicationService.GetAsync(app.Name);
                  var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                  var protectedParameters = argoAppParams.Where(p => app.ProtectedParameters?.Contains(p.Name) ?? false).ToList();
                  var parametersToDelete = argoAppParams.Where(p => sectionsToClean.Any(s => p.Name.StartsWith(s)));
                  argoAppParams = argoAppParams.Except(parametersToDelete).ToList();
                  var configs = app.Platform.Config.Select(c => new PlatformSection.Config(c.Key, c.Value));
                  List<HelmParameter> secretConfigs = app.Platform.SecretConfig.Select(c => new PlatformSection.SecretConfig(c.Key, c.Value)).ToList<HelmParameter>();
                  var storefrontSecretConfigs = app.Storefront.SecretConfig.Select(c => new StorefrontSection.SecretConfig(c.Key, c.Value));
                  var storefrontConfigs = app.Storefront.Config.Select(c => new StorefrontSection.Config(c.Key, c.Value));
                  var secrets = secretConfigs.Concat(storefrontSecretConfigs).Select(s => s.Value).Distinct().Select(s => new PlatformSection.Secret(s));
                  var helmParameters = new List<HelmParameter>
                  {
                      new PlatformSection.ImageTag(app.Platform.ImageTag),
                      new PlatformSection.Tier(app.Platform.Tier),
                      new PlatformSection.ImageRepository(app.Platform.ImageRepository),
                      new PlatformSection.IngressConfig(app.Ingress.Config),
                      new PlatformSection.IngressHostname(app.Ingress.Hostname),
                      new StorefrontSection.IngressHostname(app.Ingress.StorefrontHostname),
                      new StorefrontSection.ImageTag(app.Storefront.ImageTag),
                      new StorefrontSection.ImageRepository(app.Storefront.ImageRepository),
                      new StorefrontSection.IngressHostname(app.Storefront.Ingress),
                      new StorefrontSection.ThemeUrl(app.Storefront.ThemeUrl),
                      new StorefrontSection.ThemeName(app.Storefront.ThemeName),
                      new AdvancedServiceSection.Enabled(app.AdvancedService.Enabled),
                      new AdvancedServiceSection.Name(app.AdvancedService.Name),
                      new AdvancedServiceSection.ImageRepository(app.AdvancedService.ImageRepository),
                      new AdvancedServiceSection.ImageTag(app.AdvancedService.ImageTag),
                      new AdvancedServiceSection.IngressPath(app.AdvancedService.IngressPath)
                  }.Where(p => p.Value != null && !app.ProtectedParameters.Contains(p.Name));

                  argoAppParams = argoAppParams.Concat(configs)
                      .Concat(secretConfigs)
                      .Concat(secrets)
                      .Concat(storefrontSecretConfigs)
                      .Concat(storefrontConfigs)
                      .Concat(helmParameters)
                      .ToList();

                  argoAppParams = argoAppParams.Where(a => !protectedParameters.Any(p => p.Name == a.Name)).Concat(protectedParameters).ToList();

                  argoApp.Spec.Source.Helm.Parameters = argoAppParams;
                  await argoClient.ApplicationService.UpdateSpecAsync(app.Name, argoApp.Spec);
              }
          });
    }
}
