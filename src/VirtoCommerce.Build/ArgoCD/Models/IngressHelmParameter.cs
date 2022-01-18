using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class IngressHelmParameter: V1alpha1HelmParameter
    {
        public IngressHelmParameter(string value) : base(false, $"ingress.config", value)
        {

        }
    }
}
