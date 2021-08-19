using System.ComponentModel;
using Nuke.Common.Tooling;

namespace VirtoCommerce.Build
{
    [TypeConverter(typeof(TypeConverter<Configuration>))]
    public class Configuration : Enumeration
    {
        public readonly static Configuration Debug = new Configuration { Value = nameof(Debug) };
        public readonly static Configuration Release = new Configuration { Value = nameof(Release) };

        public static implicit operator string(Configuration configuration)
        {
            return configuration.Value;
        }
    }
}
