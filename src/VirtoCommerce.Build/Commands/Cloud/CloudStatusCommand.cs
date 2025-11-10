using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudStatusCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string HealthStatusOption = "--health-status";
    public const string SyncStatusOption = "--sync-status";

    public CloudStatusCommand() : base("status", "Wait for environment health and sync status")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name",
            Required = true
        };

        var healthStatusOption = new Option<string>(HealthStatusOption)
        {
            Description = "Expected health status to wait for"
        };

        var syncStatusOption = new Option<string>(SyncStatusOption)
        {
            Description = "Expected sync status to wait for"
        };

        Add(environmentNameOption);
        Add(healthStatusOption);
        Add(syncStatusOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var healthStatus = parseResult.GetValue<string>(HealthStatusOption);
            var syncStatus = parseResult.GetValue<string>(SyncStatusOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);

            Log.Information("Executing cloud status command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud status");
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
            await Build.CloudEnvStatusMethod(cloudUrl, environmentName, healthStatus, syncStatus);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud status command");
            return 1;
        }

        return 0;
    }
}
