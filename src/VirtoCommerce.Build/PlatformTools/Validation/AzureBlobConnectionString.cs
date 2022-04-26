using System;
using System.ComponentModel;
using System.Globalization;
using Azure.Storage.Blobs;

namespace PlatformTools.Validation
{
    [TypeConverter(typeof(TypeConverter))]
    public class AzureBlobConnectionString: ConnectionString
    {
        public AzureBlobConnectionString(string connectionString) : base(connectionString)
        {
        }

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
                    return string.IsNullOrEmpty(stringValue) ? null : new AzureBlobConnectionString(stringValue);
                }

                if (value is null)
                {
                    return null;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }

        public override string Validate()
        {
            try
            {
                var client = new BlobContainerClient(new Uri(_connectionString));
                var blobClient = client.GetBlobClient("");
                blobClient.DownloadContentAsync();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }
    }
}
