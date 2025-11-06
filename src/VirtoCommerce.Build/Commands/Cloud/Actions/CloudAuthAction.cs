using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud authentication command
/// </summary>
public static class CloudAuthAction
{
    /// <summary>
    ///     Executes the cloud auth command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var cloudUrl = parseResult.GetValue<string>("--url");
            var azureAd = parseResult.GetValue<bool>("--azure-ad");
            var token = parseResult.GetValue<string>("--token");
            var tokenFile = parseResult.GetValue<string>("--token-file");

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
