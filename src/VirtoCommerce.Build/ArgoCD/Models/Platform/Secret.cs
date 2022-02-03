namespace VirtoCommerce.Build.ArgoCD.Models.Platform
{
    public class Secret : HelmParameter
    {
        public Secret(string name) : base(false, $"platform.secrets.{name}", name)
        {
        }
    }
}

