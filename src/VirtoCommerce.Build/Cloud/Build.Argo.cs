using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ArgoCD.Client;
using Cloud.Models;
using Cloud.Models.Platform;
using Nuke.Common;
using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Storefront = Cloud.Models.Storefront;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("ArgoCD Server")] public string ArgoServer { get; set; }
        [Parameter("ArgoCD Token")] public string ArgoToken { get; set; }
        [Parameter("Config file for Argo Application Service")] public string ArgoConfigFile { get; set; }
        [Parameter("Array of Helm parameters")] public HelmParameter[] HelmParameters { get; set; }
        [Parameter("Argo Application Name")] public string ArgoAppName { get; set; }
        [Parameter("Health Status that need to be awaited")] public string HealthStatus { get; set; }
        [Parameter("SyncStatus that need to be awaited")] public string SyncStatus { get; set; }
        [Parameter("Delay between requests in seconds")] public int Delay { get; set; } = 1;
        [Parameter("Number of attempts before fail")] public int AttempsNumber { get; set; } = 100;

        [Parameter("SaaS Portal")] public string CloudUrl { get; set; } = "https://cloud.govirto.com";
        [Parameter("SaaS Token")] public string CloudToken { get; set; }
        [Parameter("App Project Name")] public string AppProject { get; set; }

        public Target WaitFor => _ => _
         .Executes(async () =>
         {
             var argoClient = CreateArgoCDClient(ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"), new Uri(ArgoServer));
             for (int i = 0; i < AttempsNumber; i++)
             {
                 Log.Information($"Attemp #{i + 1}");
                 var argoApp = await argoClient.ApplicationService.GetAsync(ArgoAppName);
                 Log.Information($"Actual Health Status is {argoApp.Status.Health.Status} - expected is {HealthStatus ?? "Not expected"}\n Actual Sync Status is {argoApp.Status.Sync.Status} - expected is {SyncStatus ?? "Not expected"}");
                 if (CheckAppServiceStatus(HealthStatus, argoApp.Status.Health.Status) && CheckAppServiceStatus(SyncStatus, argoApp.Status.Sync.Status))
                 {
                     break;
                 }

                 await Task.Delay(TimeSpan.FromSeconds(Delay));
             }
         });

        private static bool CheckAppServiceStatus(string expected, string actual)
        {
            if (expected == actual || string.IsNullOrEmpty(expected))
            {
                return true;
            }

            return false;
        }

        public Target SetHelmParameter => _ => _
             .Executes(async () =>
             {
                 var argoClient = CreateArgoCDClient(ArgoToken ?? Environment.GetEnvironmentVariable("ARGO_TOKEN"), new Uri(ArgoServer));
                 var argoApp = await argoClient.ApplicationService.GetAsync(ArgoAppName);
                 var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                 foreach (var parameter in HelmParameters)
                 {
                     var argoParameter = argoAppParams.FirstOrDefault(p => p.Name == parameter.Name);
                     if (argoParameter != null)
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
        public Target UpdateCloudEnvironment => _ => _
            .Executes(async () =>
            {
                var cloudClient = new Cloud.Client.VirtoCloudClient(CloudUrl, CloudToken);
                var rawYaml = await File.ReadAllTextAsync(ArgoConfigFile);
                await cloudClient.UpdateEnvironmentAsync(rawYaml, AppProject);
            });
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
                "custom.app"
               };
               foreach (var app in apps)
               {
                   var argoApp = await argoClient.ApplicationService.GetAsync(app.Name);
                   var argoAppParams = argoApp.Spec.Source.Helm.Parameters;
                   var protectedParameters = argoAppParams.Where(p => app.ProtectedParameters?.Contains(p.Name) ?? false).ToList();
                   var parametersToDelete = argoAppParams.Where(p => sectionsToClean.Any(s => p.Name.StartsWith(s)));
                   argoAppParams = argoAppParams.Except(parametersToDelete).ToList();
                   var configs = app.Platform.Config.Select(c => new Config(c.Key, c.Value));
                   List<HelmParameter> secretConfigs = app.Platform.SecretConfig.Select(c => new SecretConfig(c.Key, c.Value)).ToList<HelmParameter>();
                   var storefrontSecretConfigs = app.Storefront.SecretConfig.Select(c => new Storefront.SecretConfig(c.Key, c.Value));
                   var storefrontConfigs = app.Storefront.Config.Select(c => new Storefront.Config(c.Key, c.Value));
                   var secrets = secretConfigs.Concat(storefrontSecretConfigs).Select(s => s.Value).Distinct().Select(s => new Secret(s));
                   var helmParameters = new List<HelmParameter>
                   {
                      new ImageTag(app.Platform.ImageTag),
                      new Tier(app.Platform.Tier),
                      new ImageRepository(app.Platform.ImageRepository),
                      new IngressConfig(app.Ingress.Config),
                      new IngressHostname(app.Ingress.Hostname),
                      new Storefront.IngressHostname(app.Ingress.StorefrontHostname),
                      new Storefront.ImageTag(app.Storefront.ImageTag),
                      new Storefront.ImageRepository(app.Storefront.ImageRepository),
                      new Storefront.IngressHostname(app.Storefront.Ingress),
                      new Storefront.ThemeUrl(app.Storefront.ThemeUrl),
                      new Storefront.ThemeName(app.Storefront.ThemeName)
                   };
                   foreach (var (customAppName, customAppParameters) in app.CustomApps)
                   {
                       helmParameters.AddRange(customAppParameters.GetParameters(customAppName));
                   }
                   helmParameters = helmParameters.Where(p => p.Value != null && !app.ProtectedParameters.Contains(p.Name)).ToList();

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
