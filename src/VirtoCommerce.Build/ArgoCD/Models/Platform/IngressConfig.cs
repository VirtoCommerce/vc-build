namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class IngressConfig : HelmParameter
    {
        public IngressConfig(string value) : base(false, $"ingress.config", value)
        {

        }
    }
}
