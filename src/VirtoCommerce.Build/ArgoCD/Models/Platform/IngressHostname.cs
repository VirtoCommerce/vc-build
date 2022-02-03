namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class IngressHostname : HelmParameter
    {
        public IngressHostname(string value) : base(false, $"ingress.hostname", value)
        {

        }
    }
}
