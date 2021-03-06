using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;

namespace PlatformTools.Validation
{
    [TypeConverter(typeof(TypeConverter))]
    public class MsSqlConnectionString: ConnectionString
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
                    return string.IsNullOrEmpty(stringValue) ? null : new MsSqlConnectionString(stringValue);
                }

                if (value is null)
                {
                    return null;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        public MsSqlConnectionString(string connectionString): base(connectionString)
        {
        }

        public override string Validate()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

    }
}
