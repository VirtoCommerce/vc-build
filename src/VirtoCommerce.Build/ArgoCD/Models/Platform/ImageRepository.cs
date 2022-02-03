namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class ImageRepository : V1alpha1HelmParameter
    {
        public ImageRepository(string value) : base(false, "platform.image.repository", value)
        {
        }
    }
}

