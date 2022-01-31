using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class ImageTag : V1alpha1HelmParameter
    {
        public ImageTag(string value) : base(false, "advanced.service.image.tag", value)
        {
        }
    }
}

