namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class IngressHostname : HelmParameter
    {
        public IngressHostname(string value) : base(false, $"ingress.storefront_hostname", value)
        {

        }
    }
}
