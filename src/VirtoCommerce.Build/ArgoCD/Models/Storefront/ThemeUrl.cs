using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ThemeUrl : V1alpha1HelmParameter
    {
        public ThemeUrl(string value) : base(false, $"theme.url", value)
        {

        }
    }
}
