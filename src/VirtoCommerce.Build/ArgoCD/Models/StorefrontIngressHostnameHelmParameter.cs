using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontIngressHostnameHelmParameter: V1alpha1HelmParameter
    {
        public StorefrontIngressHostnameHelmParameter(string value) : base(false, $"ingress.storefront_hostname", value)
        {

        }
    }
}
