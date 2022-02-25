namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class AdvancedService : HelmParameter
    {
        public AdvancedService(string name, string value) : base(false, $"advanced.service.{name}", value)
        {
        }
    }
}

