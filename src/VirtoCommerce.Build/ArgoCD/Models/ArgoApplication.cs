namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoApplication
    {
        public string Name { get; set; }
        public ArgoAppPlatformSection Platform { get; set; }
        public ArgoAppStorefrontSection Storefront { get; set; }
    }
}
