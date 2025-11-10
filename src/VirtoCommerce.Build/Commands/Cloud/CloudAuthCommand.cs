using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud
{
    public class CloudAuthCommand : Command
    {
        public const string AzureAdOption = "--azure-ad";
        public const string TokenFileOption = "--token-file";

        public CloudAuthCommand() : base("auth", "Authenticate with VirtoCloud")
        {
            var azureAdOption = new Option<bool>(AzureAdOption)
            {
                Description = "Use Azure AD authentication"
            };

            var tokenFileOption = new Option<string>(TokenFileOption)
            {
                Description = "Path to file containing authentication token",
                DefaultValueFactory = _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud")
            };

            Add(azureAdOption);
            Add(tokenFileOption);

            SetAction(ExecuteAsync);
        }

        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult using correct API
                var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);
                var token = parseResult.GetValue<string>(CloudCommand.CloudTokenOption);
                var azureAd = parseResult.GetValue<bool>(AzureAdOption);
                var tokenFile = parseResult.GetValue<string>(TokenFileOption);

                Log.Information("Executing cloud authentication command");

                if (azureAd)
                {
                    Log.Information("Using Azure AD authentication");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Log.Information("Using provided authentication token");
                }

                Log.Information("Delegating to existing CloudAuth target");

                await Build.CloudAuthMethod(cloudUrl, tokenFile, token, azureAd);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing cloud auth command");
                return 1;
            }

            return 0;
        }
    }
}
