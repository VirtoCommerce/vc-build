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
                "theme.url"
              };
              httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"));
              httpClient.BaseAddress = argoServerUrl;
              var argoClient = new ArgoCDClient(httpClient, true);
              argoClient.BaseUri = argoServerUrl;
              foreach (var app in apps)
              {
                  var argoApp = await argoClient.ApplicationService.GetAsync(app.Name);
                  var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                  var parametersToDelete = argoAppParams.Where(p => sectionsToClean.Any(s => p.Name.StartsWith(s)));
                  argoAppParams = argoAppParams.Except(parametersToDelete).ToList();
                  var configs = app.Platform.Config.Select(c => new ConfigHelmParameter(c.Key, c.Value));
                  List<V1alpha1HelmParameter> secretConfigs = app.Platform.SecretConfig.Select(c => new SecretConfigHelmParameter(c.Key, c.Value)).ToList<V1alpha1HelmParameter>();
                  var storefrontSecretConfigs = app.Storefront.SecretConfig.Select(c => new StorefrontSecretConfigHelmParameter(c.Key, c.Value));
                  var storefrontConfigs = app.Storefront.Config.Select(c => new StorefrontConfigHelmParameter(c.Key, c.Value));
                  var secrets = secretConfigs.Concat(storefrontSecretConfigs).Select(s => s.Value).Distinct().Select(s => new SecretHelmParameter(s));
                  var helmParameters = new List<V1alpha1HelmParameter>
                  {
                      new ImageTagHelmParameter(app.Platform.ImageTag),
                      new TierHelmParameter(app.Platform.Tier),
                      new ImageRepositoryHelmParameter(app.Platform.ImageRepository),
                      new IngressConfigHelmParameter(app.Ingress.Config),
                      new IngressHostnameHelmParameter(app.Ingress.Hostname),
                      new StorefrontIngressHostnameHelmParameter(app.Ingress.StorefrontHostname),
                      new StorefrontImageTagHelmParameter(app.Storefront.ImageTag),
                      new StorefrontImageRepositoryHelmParameter(app.Storefront.ImageRepository),
                      new StorefrontIngressHostnameHelmParameter(app.Storefront.Ingress),
                      new ThemeUrlHelmParameter(app.Storefront.ThemeUrl)
                  }.Where(p => p.Value != null);

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
