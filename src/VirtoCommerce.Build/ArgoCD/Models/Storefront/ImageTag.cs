namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ImageTag : V1alpha1HelmParameter
    {
        public ImageTag(string value) : base(false, "storefront.image.tag", value)
        {
        }
    }
}

