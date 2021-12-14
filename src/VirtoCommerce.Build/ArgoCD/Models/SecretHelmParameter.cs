using ArgoCD.Client.Models;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    public class SecretHelmParameter : V1alpha1HelmParameter
    {
        public SecretHelmParameter(string name) : base(false, $"platform.secrets.{name}", name)
        {
        }
    }
}

