using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class IngressHostname : V1alpha1HelmParameter
    {
        public IngressHostname(string value) : base(false, $"ingress.storefront_hostname", value)
        {

        }
    }
}
