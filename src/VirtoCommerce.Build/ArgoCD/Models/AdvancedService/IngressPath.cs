namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class IngressPath : HelmParameter
    {
        public IngressPath(string value) : base(false, "advanced.service.ingress_path", value)
        {
        }
    }
}

