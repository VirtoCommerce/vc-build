namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class Secret : V1alpha1HelmParameter
    {
        public Secret(string name) : base(false, $"platform.secrets.{name}", name)
        {
        }
    }
}

