namespace Cloud.Models.Storefront
{
    public class ImageRepository : HelmParameter
    {
        public ImageRepository(string value) : base(false, "storefront.image.repository", value)
        {
        }
    }
}
