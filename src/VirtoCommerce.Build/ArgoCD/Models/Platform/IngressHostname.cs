namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class IngressHostname : V1alpha1HelmParameter
    {
        public IngressHostname(string value) : base(false, $"ingress.hostname", value)
        {

        }
    }
}
