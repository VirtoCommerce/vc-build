using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontConfigHelmParameter : V1alpha1HelmParameter
    {
        public StorefrontConfigHelmParameter(string name, string value) : base(false, $"storefront.config.{name}", value)
        {
        }
    }
}

