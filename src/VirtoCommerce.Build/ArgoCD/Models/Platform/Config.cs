namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class Config : V1alpha1HelmParameter
    {
        public Config(string name, string value) : base(false, $"platform.config.{name}", value)
        {
        }
    }
}

