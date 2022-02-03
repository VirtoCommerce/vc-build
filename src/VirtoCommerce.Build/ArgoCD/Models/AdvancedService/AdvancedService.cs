namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class AdvancedService : V1alpha1HelmParameter
    {
        public AdvancedService(string name, string value) : base(false, $"advanced.service.{name}", value)
        {
        }
    }
}

