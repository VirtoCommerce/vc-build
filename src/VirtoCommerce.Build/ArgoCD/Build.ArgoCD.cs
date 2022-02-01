using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using ArgoCD.Client;
using ArgoCD.Client.Models;
using Nuke.Common;
using VirtoCommerce.Build.ArgoCD.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PlatformSection = VirtoCommerce.Build.ArgoCD.Models.Platform;
using StorefrontSection = VirtoCommerce.Build.ArgoCD.Models.Storefront;
using AdvancedServiceSection = VirtoCommerce.Build.ArgoCD.Models.AdvancedService;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
        [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }
        [Parameter("Config file for Argo Application Service")] public string ArgoConfigFile { get; set; }

        public Target ArgoUpdateApplication => _ => _
          .Executes(async () =>
          {
              var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
              var rawYaml = await File.ReadAllTextAsync(ArgoConfigFile);
              var apps = deserializer.Deserialize<IList<ArgoApplication>>(rawYaml);
              var httpClient = new HttpClient();
              var argoServerUrl = new Uri(ArgoServer);
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
              httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"));
              httpClient.BaseAddress = argoServerUrl;
              var argoClient = new ArgoCDClient(httpClient, true);
              argoClient.BaseUri = argoServerUrl;
              foreach (var app in apps)
              {
                  var argoApp = await argoClient.ApplicationService.GetAsync(app.Name);
                  var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                  var parametersToDelete = argoAppParams.Where(p => sectionsToClean.Any(s => p.Name.StartsWith(s) && !app.ProtectedParameters.Contains(p.Name)));
                  argoAppParams = argoAppParams.Except(parametersToDelete).ToList();
                  var configs = app.Platform.Config.Select(c => new PlatformSection.Config(c.Key, c.Value));
                  List<V1alpha1HelmParameter> secretConfigs = app.Platform.SecretConfig.Select(c => new PlatformSection.SecretConfig(c.Key, c.Value)).ToList<V1alpha1HelmParameter>();
                  var storefrontSecretConfigs = app.Storefront.SecretConfig.Select(c => new StorefrontSection.SecretConfig(c.Key, c.Value));
                  var storefrontConfigs = app.Storefront.Config.Select(c => new StorefrontSection.Config(c.Key, c.Value));
                  var secrets = secretConfigs.Concat(storefrontSecretConfigs).Select(s => s.Value).Distinct().Select(s => new PlatformSection.Secret(s));
                  var helmParameters = new List<V1alpha1HelmParameter>
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


                  argoApp.Spec.Source.Helm.Parameters = argoAppParams;
                  await argoClient.ApplicationService.UpdateSpecAsync(app.Name, argoApp.Spec);
              }
          });
    }
}
