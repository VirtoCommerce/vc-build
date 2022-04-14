namespace ArgoCD.Models.Storefront
{
    public class ImageTag : HelmParameter
    {
        public ImageTag(string value) : base(false, "storefront.image.tag", value)
        {
        }
    }
}

