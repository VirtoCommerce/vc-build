using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class SecretConfig : V1alpha1HelmParameter
    {
        public SecretConfig(string name, string value) : base(false, $"storefront.secret_config.{name}", value)
        {
        }
    }
}

