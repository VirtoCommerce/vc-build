using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ConfigHelmParameter : V1alpha1HelmParameter
    {
        public ConfigHelmParameter(string name, string value) : base(false, $"platform.config.{name}", value)
        {
        }
    }
}

