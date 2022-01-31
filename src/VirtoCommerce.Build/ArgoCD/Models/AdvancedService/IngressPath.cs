using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class IngressPath : V1alpha1HelmParameter
    {
        public IngressPath(string value) : base(false, "advanced.service.ingress_path", value)
        {
        }
    }
}

