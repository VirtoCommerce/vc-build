namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ThemeName : HelmParameter
    {
        public ThemeName(string value) : base(false, $"theme.name", value)
        {

        }
    }
}
