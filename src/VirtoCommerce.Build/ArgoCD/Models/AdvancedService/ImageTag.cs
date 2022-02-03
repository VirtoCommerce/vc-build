namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class ImageTag : HelmParameter
    {
        public ImageTag(string value) : base(false, "advanced.service.image.tag", value)
        {
        }
    }
}

