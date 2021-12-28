using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ImageTagHelmParameter: V1alpha1HelmParameter
    {
        public ImageTagHelmParameter(string value): base(false, "platform.image.tag", value)
        {
        }
    }
}

