namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class SecretConfig : V1alpha1HelmParameter
    {
        public SecretConfig(string name, string value) : base(false, $"platform.secret_config.{name}", value)
        {
        }
    }
}

