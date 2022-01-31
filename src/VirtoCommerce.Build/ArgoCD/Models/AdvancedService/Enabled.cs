using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class Enabled : V1alpha1HelmParameter
    {
        public Enabled(string value) : base(false, "advanced.service.enabled", value)
        {
        }
    }
}

