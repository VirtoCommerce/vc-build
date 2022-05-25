namespace Cloud.Models.Storefront
{
    public class Config : HelmParameter
    {
        public Config(string name, string value) : base(false, $"storefront.config.{name}", value)
        {
        }
    }
}
