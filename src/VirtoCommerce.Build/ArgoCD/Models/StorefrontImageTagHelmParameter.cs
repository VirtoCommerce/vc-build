using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontImageTagHelmParameter : V1alpha1HelmParameter
    {
        public StorefrontImageTagHelmParameter(string value): base(false, "storefront.image.tag", value)
        {
        }
    }
}

