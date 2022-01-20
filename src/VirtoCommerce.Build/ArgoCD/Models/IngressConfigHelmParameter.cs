using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class IngressConfigHelmParameter: V1alpha1HelmParameter
    {
        public IngressConfigHelmParameter(string value) : base(false, $"ingress.config", value)
        {

        }
    }
}
