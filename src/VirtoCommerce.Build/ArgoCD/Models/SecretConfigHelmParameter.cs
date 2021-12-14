using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class SecretConfigHelmParameter: V1alpha1HelmParameter
    {
        public SecretConfigHelmParameter(string name, string value): base(false, $"platform.secret_config.{name}", value)
        {
        }
    }
}

