using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontIngressHelmParameter: V1alpha1HelmParameter
    {
        public StorefrontIngressHelmParameter(string value) : base(false, $"ingress.storefront_config", value)
        {

        }
    }
}
