namespace ArgoCD.Models.Platform
{
    public class Tier : HelmParameter
    {
        public Tier(string value) : base(false, "platform.tier", value)
        {
        }
    }
}
