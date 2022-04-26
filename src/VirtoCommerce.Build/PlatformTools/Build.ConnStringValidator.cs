using Nuke.Common;
using Nuke.Common.IO;
using PlatformTools.Validation;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("MSSQL Connection String")] public MsSqlConnectionString Sql { get; set; }
        [Parameter("Redis Connection String")] public RedisConnectionString Redis { get; set; }
        [Parameter("Azure Blob Connection String")] public AzureBlobConnectionString AzureBlob { get; set; }

        private Target Configure => _ => _
         .Executes(() =>
         {
             CheckAndUpdateConnectionString(Sql);
             CheckAndUpdateConnectionString(Redis);
             CheckAndUpdateConnectionString(AzureBlob);
         });

        private void CheckAndUpdateConnectionString(ConnectionString connectionString)
        {
            if (!connectionString?.IsEmpty() ?? false)
            {
                var validationResult = AzureBlob.Validate();
                if (validationResult != string.Empty)
                {
                    Assert.Fail(validationResult);
                }

                switch (connectionString)
                {
                    case MsSqlConnectionString mssql:
                        UpdateMsSqlConnectionString(mssql.GetConnectionString());
                        break;

                    case RedisConnectionString redis:
                        UpdateRedisConnectionString(redis.GetConnectionString());
                        break;

                    case AzureBlobConnectionString azureBlob:
                        UpdateAzureBlobConnectionString(azureBlob.GetConnectionString());
                        break;
                }
            }
        }

        private void UpdateAzureBlobConnectionString(string value)
        {
            UpdateContentConnectionString("AzureBlobStorage", value);
        }

        private void UpdateContentConnectionString(string provider, string value)
        {
            var appsettingsObject = SerializationTasks.JsonDeserializeFromFile(AppsettingsPath);
            var contentSection = appsettingsObject["Content"];
            var providerObject = contentSection[provider];
            providerObject["ConnectionString"] = value;
            SerializationTasks.JsonSerializeToFile(appsettingsObject, AppsettingsPath);
        }

        private void UpdateRedisConnectionString(string value)
        {
            UpdateConnectionString("RedisConnectionString", value);
        }

        private void UpdateMsSqlConnectionString(string value)
        {
            UpdateConnectionString("VirtoCommerce", value);
        }

        private void UpdateConnectionString(string name, string value)
        {
            var appsettingsObject = SerializationTasks.JsonDeserializeFromFile(AppsettingsPath);
            var connStrings = appsettingsObject["ConnectionStrings"];
            connStrings[name] = value;
            SerializationTasks.JsonSerializeToFile(appsettingsObject, AppsettingsPath);
        }
    }
}
