namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class SecretConfig : HelmParameter
    {
        public SecretConfig(string name, string value) : base(false, $"platform.secret_config.{name}", value)
        {
        }
    }
}

