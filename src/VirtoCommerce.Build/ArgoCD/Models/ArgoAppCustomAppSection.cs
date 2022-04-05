using System.Collections.Generic;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoAppCustomAppSection
    {
        private string _app;
        public ArgoAppCustomAppSection()
        {
            PlatformPath = new List<string>();
            StorefrontPath = new List<string>();
            ServicePath = new List<string>();
        }

        public void SetApp(string name)
        {
            _app = name;
        }

        private string GetPrefix()
        {
            return $"custom.{_app}.";
        }
        public string Enabled { get; set; }
        public string Type { get; set; }
        public string AttachedTo { get; set; }
        public string ProbePath { get; set; }
        public string Replicas { get; set; }
        public string Hostname { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
        public string ImageRepository { get; set; }
        public string ImageTag { get; set; }
        public string IngressPath { get; set; }
        public string RequestsMemory { get; set; }
        public string RequestsCPU { get; set; }
        public string LimitsMemory { get; set; }
        public string LimitsCPU { get; set; }
        public List<string> PlatformPath { get; set; }
        public List<string> StorefrontPath { get; set; }
        public List<string> ServicePath { get; set; }


        public List<HelmParameter> GetParameters(string customAppName)
        {
            SetApp(customAppName);
            var parameters = new List<HelmParameter>()
            {
                new HelmParameter(name: $"{GetPrefix()}enabled", value: Enabled),
                new HelmParameter(name: $"{GetPrefix()}type", value: Type),
                new HelmParameter(name: $"{GetPrefix()}attached_to", value: AttachedTo),
                new HelmParameter(name: $"{GetPrefix()}probe_path", value: ProbePath),
                new HelmParameter(name: $"{GetPrefix()}replicas", value: Replicas),
                new HelmParameter(name: $"{GetPrefix()}hostname", value: Hostname),
                new HelmParameter(name: $"{GetPrefix()}port", value: Port),
                new HelmParameter(name: $"{GetPrefix()}name", value: Name),
                new HelmParameter(name: $"{GetPrefix()}image.repository", value: ImageRepository),
                new HelmParameter(name: $"{GetPrefix()}image.tag", value: ImageTag),
                new HelmParameter(name: $"{GetPrefix()}ingress_path", value: IngressPath),
                new HelmParameter(name: $"{GetPrefix()}resources.requests.memory", value: RequestsMemory),
                new HelmParameter(name: $"{GetPrefix()}resources.requests.cpu", value: RequestsCPU),
                new HelmParameter(name: $"{GetPrefix()}resources.limits.memory", value: LimitsMemory),
                new HelmParameter(name: $"{GetPrefix()}resources.limits.cpu", value: LimitsCPU)
            };

            parameters.AddRange(ConvertToPathParameters(PlatformPath, $"{GetPrefix()}paths.platform"));
            parameters.AddRange(ConvertToPathParameters(StorefrontPath, $"{GetPrefix()}paths.storefront"));
            parameters.AddRange(ConvertToPathParameters(ServicePath, $"{GetPrefix()}paths.service"));

            return parameters;
        }

        private static List<HelmParameter> ConvertToPathParameters(List<string> parameters, string name)
        {
            var result = new List<HelmParameter>();
            for(int i = 0; i < parameters.Count; i++)
            {
                result.Add(new HelmParameter(name: $"{name}[{i}]", value: parameters[i]));
            }
            return result;
        }
    }
}
