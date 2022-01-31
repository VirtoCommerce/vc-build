namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class ArgoAppAdvancedServiceSection
    {
        public string Enabled { get; set; }
        public string Name { get; set; }
        public string ImageRepository { get; set; }
        public string ImageTag { get; set; }
        public string IngressPath { get; set; }
    }
}
