using System;
using System.ComponentModel;
using System.Globalization;
using ArgoClient = ArgoCD.Client;

namespace VirtoCommerce.Build.ArgoCD.Models
{
    [Serializable]
    [TypeConverter(typeof(TypeConverter))]
    public class V1alpha1HelmParameter: ArgoClient.Models.V1alpha1HelmParameter
    {
        public V1alpha1HelmParameter()
        {
        }

        public V1alpha1HelmParameter(bool? forceString = default(bool?), string name = default(string), string value = default(string)) : base(forceString, name, value)
        {
        }

        public class TypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                var canParse = false;
                if(base.CanConvertFrom(context, sourceType))
                {
                    canParse = (context.Instance as string).Contains('=');
                }
                return sourceType == typeof(string) && canParse;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if(value is string stringValue)
                {
                    var splited = stringValue.Split("=");
                    if (splited.Length == 2)
                        return new V1alpha1HelmParameter(false, splited[0], splited[1]);
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
