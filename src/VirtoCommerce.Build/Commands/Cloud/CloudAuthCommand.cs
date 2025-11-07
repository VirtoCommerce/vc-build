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
        public const string CloudUrlOption = "--url";
        public const string AzureAdOption = "--azure-ad";
        public const string TokenOption = "--token";
        public const string TokenFileOption = "--token-file";

        public CloudAuthCommand() : base("auth", "Authenticate with VirtoCloud")
        {

            var cloudUrl = new Option<string>(CloudUrlOption)
            {
                Description = "VirtoCloud URL",
                DefaultValueFactory = _ => "https://portal.virtocommerce.cloud"
            };

            var azureAdOption = new Option<bool>(AzureAdOption)
            {
                Description = "Use Azure AD authentication"
            };
            var tokenOption = new Option<string>(TokenOption)
            {
                Description = "Provide authentication token directly"
            };

            var tokenFileOption = new Option<string>(TokenFileOption)
            {
                Description = "Path to file containing authentication token",
                DefaultValueFactory = _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud")
            };

            Add(cloudUrl);
            Add(azureAdOption);
            Add(tokenOption);
            Add(tokenFileOption);

            SetAction(ExecuteAsync);
        }

        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult using correct API
                var cloudUrl = parseResult.GetValue<string>(CloudUrlOption);
                var azureAd = parseResult.GetValue<bool>(AzureAdOption);
                var token = parseResult.GetValue<string>(TokenOption);
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
                Console.Error.WriteLine($"Error executing cloud auth: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
