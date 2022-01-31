using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ImageRepository : V1alpha1HelmParameter
    {
        public ImageRepository(string value) : base(false, "storefront.image.repository", value)
        {
        }
    }
}

