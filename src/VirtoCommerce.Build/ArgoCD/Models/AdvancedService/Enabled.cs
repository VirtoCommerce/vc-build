namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class Enabled : HelmParameter
    {
        public Enabled(string value) : base(false, "advanced.service.enabled", value)
        {
        }
    }
}

