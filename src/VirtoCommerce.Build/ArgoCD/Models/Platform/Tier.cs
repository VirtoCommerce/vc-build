using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class Tier : V1alpha1HelmParameter
    {
        public Tier(string value) : base(false, "platform.tier", value)
        {
        }
    }
}
