using System.Collections.Generic;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoAppPlatformSection
    {
        public string ImageRepository { get; set; }
        public string ImageTag { get; set; }
        public string Tier { get; set; }
        public string Ingress { get; set; }
        public Dictionary<string, string> Config { get; set; }
        public Dictionary<string, string> SecretConfig { get; set; }
    }
}
