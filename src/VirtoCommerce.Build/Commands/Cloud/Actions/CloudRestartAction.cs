using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud restart command
/// </summary>
public static class CloudRestartAction
{
    /// <summary>
    ///     Executes the cloud restart command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>("--environment-name");

            Log.Information("Executing cloud restart command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud restart");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            Log.Information("Delegating to CloudEnvRestart method");

            // Call CloudEnvRestart method directly
            await Build.CloudEnvRestartMethod(environmentName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud restart command");
            Console.Error.WriteLine($"Error executing cloud restart: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
