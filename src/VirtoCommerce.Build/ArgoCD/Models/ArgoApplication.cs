using System.Collections.Generic;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoApplication
    {
        public string Name { get; set; }
        public ArgoAppPlatformSection Platform { get; set; }
        public ArgoAppStorefrontSection Storefront { get; set; }
        public ArgoAppIngressSection Ingress { get; set; }
        public ArgoAppAdvancedServiceSection AdvancedService { get; set; }
        public List<string> ProtectedParameters { get; set; }
    }
}
