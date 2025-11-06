using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud update command
/// </summary>
public static class CloudUpdateAction
{
    /// <summary>
    ///     Executes the cloud update command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var manifest = parseResult.GetValue<string>("--manifest");
            var routesFile = parseResult.GetValue<string>("--routes-file");

            Log.Information("Executing cloud update command");

            // Validate required parameters
            if (string.IsNullOrEmpty(manifest))
            {
                Log.Error("Manifest path is required for cloud update");
                Console.Error.WriteLine("Error: --manifest is required");
                return 1;
            }

            Log.Information("Manifest path: {Manifest}", manifest);
            if (!string.IsNullOrEmpty(routesFile))
            {
                Log.Information("Routes file: {RoutesFile}", routesFile);
            }

            Log.Information("Delegating to CloudEnvUpdate method");

            // Call CloudEnvUpdate method directly
            await Build.CloudEnvUpdateMethod(manifest, routesFile);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud update command");
            Console.Error.WriteLine($"Error executing cloud update: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
