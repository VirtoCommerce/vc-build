using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class StorefrontImageRepositoryHelmParameter : V1alpha1HelmParameter
    {
        public StorefrontImageRepositoryHelmParameter(string value): base(false, "storefront.image.repository", value)
        {
        }
    }
}

