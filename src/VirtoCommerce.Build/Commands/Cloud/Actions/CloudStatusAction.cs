using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud status command
/// </summary>
public static class CloudStatusAction
{
    /// <summary>
    ///     Executes the cloud status command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>("--environment-name");
            var healthStatus = parseResult.GetValue<string>("--health-status");
            var syncStatus = parseResult.GetValue<string>("--sync-status");

            Log.Information("Executing cloud status command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud status");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            if (!string.IsNullOrEmpty(healthStatus))
            {
                Log.Information("Expected health status: {HealthStatus}", healthStatus);
            }

            if (!string.IsNullOrEmpty(syncStatus))
            {
                Log.Information("Expected sync status: {SyncStatus}", syncStatus);
            }

            Log.Information("Delegating to CloudEnvStatus method");

            // Call CloudEnvStatus method directly
            await Build.CloudEnvStatusMethod(environmentName, healthStatus, syncStatus);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud status command");
            Console.Error.WriteLine($"Error executing cloud status: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
