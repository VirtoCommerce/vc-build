namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class ThemeName : V1alpha1HelmParameter
    {
        public ThemeName(string value) : base(false, $"theme.name", value)
        {

        }
    }
}
