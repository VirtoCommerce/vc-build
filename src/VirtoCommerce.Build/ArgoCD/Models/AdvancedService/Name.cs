namespace VirtoCommerce.Build.ArgoCD.Models.AdvancedService
{
    public class Name : HelmParameter
    {
        public Name(string value) : base(false, "advanced.service.name", value)
        {
        }
    }
}

