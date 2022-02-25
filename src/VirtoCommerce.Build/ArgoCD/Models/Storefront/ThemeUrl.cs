namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ThemeUrl : HelmParameter
    {
        public ThemeUrl(string value) : base(false, $"theme.url", value)
        {

        }
    }
}
