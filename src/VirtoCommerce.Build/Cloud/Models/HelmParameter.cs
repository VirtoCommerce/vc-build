using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using Nuke.Common.Utilities;

namespace Cloud.Models
{
    [TypeConverter(typeof(TypeConverter))]
    [JsonConverter(typeof(HelmJsonConverter))]
    public class HelmParameter : V1alpha1HelmParameter
    {
        public HelmParameter(bool? forceString = default, string name = default, string value = default) : base(forceString, name, value)
        {
        }

        public override string ToString()
        {
            var parameter = new V1alpha1HelmParameter(null, Name, Value);
            return parameter.ToJson();
        }

        public class HelmJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var helmParameter = (HelmParameter)value;
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(helmParameter.Name);
                writer.WritePropertyName("value");
                writer.WriteValue(helmParameter.Value);
                writer.WriteEndObject();
            }
        }

        public class TypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                var canParse = false;
                if (base.CanConvertFrom(context, sourceType))
                {
                    canParse = (context.Instance as string).Contains('=');
                }
                return sourceType == typeof(string) && canParse;
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string stringValue)
                {
                    var splited = stringValue.Split("=");
                    if (splited.Length == 2)
                    {
                        return new HelmParameter(null, splited[0], splited[1]);
                    }
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
