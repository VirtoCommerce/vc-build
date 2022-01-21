using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontSecretConfigHelmParameter: V1alpha1HelmParameter
    {
        public StorefrontSecretConfigHelmParameter(string name, string value): base(false, $"storefront.secret_config.{name}", value)
        {
        }
    }
}

