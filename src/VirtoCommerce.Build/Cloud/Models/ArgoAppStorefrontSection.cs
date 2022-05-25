using System.Collections.Generic;

namespace Cloud.Models
{
    public class ArgoAppStorefrontSection
    {
        public string ImageRepository { get; set; }
        public string ImageTag { get; set; }
        public string ThemeUrl { get; set; }
        public string ThemeName { get; set; }
        public string Ingress { get; set; }
        public Dictionary<string, string> Config { get; set; }
        public Dictionary<string, string> SecretConfig { get; set; }
    }
}
