namespace VirtoCommerce.Build.ArgoCD.Models.Storefront
{
    public class SecretConfig : HelmParameter
    {
        public SecretConfig(string name, string value) : base(false, $"storefront.secret_config.{name}", value)
        {
        }
    }
}

