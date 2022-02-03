namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class ImageRepository : HelmParameter
    {
        public ImageRepository(string value) : base(false, "advanced.service.image.repository", value)
        {
        }
    }
}

