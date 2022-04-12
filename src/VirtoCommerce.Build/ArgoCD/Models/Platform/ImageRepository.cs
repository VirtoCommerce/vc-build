namespace ArgoCD.Models.Platform
{
    public class ImageRepository : HelmParameter
    {
        public ImageRepository(string value) : base(false, "platform.image.repository", value)
        {
        }
    }
}

