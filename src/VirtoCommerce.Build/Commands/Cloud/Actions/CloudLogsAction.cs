using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud logs command
/// </summary>
public static class CloudLogsAction
{
    /// <summary>
    ///     Executes the cloud logs command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>("--environment-name");
            var filter = parseResult.GetValue<string>("--filter");
            var tail = parseResult.GetValue<int>("--tail");
            var resourceName = parseResult.GetValue<string>("--resource-name");

            Log.Information("Executing cloud logs command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud logs");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            if (!string.IsNullOrEmpty(filter))
            {
                Log.Information("Log filter: {Filter}", filter);
            }

            if (tail > 0)
            {
                Log.Information("Tail lines: {Tail}", tail);
            }

            if (!string.IsNullOrEmpty(resourceName))
            {
                Log.Information("Resource name: {ResourceName}", resourceName);
            }

            Log.Information("Delegating to CloudEnvLogs method");

            // Call CloudEnvLogs method directly
            await Build.CloudEnvLogsMethod(environmentName, filter, tail, resourceName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud logs command");
            Console.Error.WriteLine($"Error executing cloud logs: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
