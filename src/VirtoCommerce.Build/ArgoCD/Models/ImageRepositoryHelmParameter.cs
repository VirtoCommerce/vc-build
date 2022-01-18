using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ImageRepositoryHelmParameter: V1alpha1HelmParameter
    {
        public ImageRepositoryHelmParameter(string value): base(false, "platform.image.repository", value)
        {
        }
    }
}

