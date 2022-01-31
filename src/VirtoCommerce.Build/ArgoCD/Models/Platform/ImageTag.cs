using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class ImageTag : V1alpha1HelmParameter
    {
        public ImageTag(string value) : base(false, "platform.image.tag", value)
        {
        }
    }
}

