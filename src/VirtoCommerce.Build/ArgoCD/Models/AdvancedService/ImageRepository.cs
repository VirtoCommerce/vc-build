namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class ImageRepository : V1alpha1HelmParameter
    {
        public ImageRepository(string value) : base(false, "advanced.service.image.repository", value)
        {
        }
    }
}

