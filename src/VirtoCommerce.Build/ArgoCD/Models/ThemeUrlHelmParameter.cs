using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ThemeUrlHelmParameter: V1alpha1HelmParameter
    {
        public ThemeUrlHelmParameter(string value) : base(false, $"theme.url", value)
        {

        }
    }
}
