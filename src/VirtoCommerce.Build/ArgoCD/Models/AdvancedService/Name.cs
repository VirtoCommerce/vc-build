using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class Name : V1alpha1HelmParameter
    {
        public Name(string value) : base(false, "advanced.service.name", value)
        {
        }
    }
}

