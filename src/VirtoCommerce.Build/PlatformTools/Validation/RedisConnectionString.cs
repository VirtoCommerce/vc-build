using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using StackExchange.Redis;

namespace PlatformTools.Validation
{
    public class RedisConnectionString: ConnectionString
    {

        public class TypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string stringValue)
                {
                    return string.IsNullOrEmpty(stringValue) ? null : new RedisConnectionString(stringValue);
                }

                if (value is null)
                {
                    return null;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
        public RedisConnectionString(string connectionString): base(connectionString)
        {
        }

        public override string Validate()
        {
            try
            {
                var memoryStream = new MemoryStream();
                TextWriter textWriter = new StreamWriter(memoryStream);
                var client = ConnectionMultiplexer.Connect(_connectionString, textWriter);
                Serilog.Log.Information(textWriter.ToString());
                Serilog.Log.Information(client.GetStatus());
                var counters = client.GetCounters();
                Serilog.Log.Information(counters.Subscription.ConnectionType.ToString());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }
    }
}
