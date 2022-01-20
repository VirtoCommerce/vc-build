using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class IngressHostnameHelmParameter: V1alpha1HelmParameter
    {
        public IngressHostnameHelmParameter(string value) : base(false, $"ingress.hostname", value)
        {

        }
    }
}
