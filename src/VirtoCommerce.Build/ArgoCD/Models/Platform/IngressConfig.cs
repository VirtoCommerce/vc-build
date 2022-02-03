namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class IngressConfig : V1alpha1HelmParameter
    {
        public IngressConfig(string value) : base(false, $"ingress.config", value)
        {

        }
    }
}
