namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class Config : HelmParameter
    {
        public Config(string name, string value) : base(false, $"platform.config.{name}", value)
        {
        }
    }
}

