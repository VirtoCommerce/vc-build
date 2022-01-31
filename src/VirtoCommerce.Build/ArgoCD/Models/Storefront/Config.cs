using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class Config : V1alpha1HelmParameter
    {
        public Config(string name, string value) : base(false, $"storefront.config.{name}", value)
        {
        }
    }
}

