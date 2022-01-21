using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ThemeNameHelmParameter: V1alpha1HelmParameter
    {
        public ThemeNameHelmParameter(string value) : base(false, $"theme.name", value)
        {

        }
    }
}
