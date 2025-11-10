using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudLogsCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string FilterOption = "--filter";
    public const string TailOption = "--tail";
    public const string ResourceNameOption = "--resource-name";

    public CloudLogsCommand() : base("logs", "Show environment logs")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name to show logs for",
            Required = true
        };

        var filterOption = new Option<string>(FilterOption) { Description = "Log filter" };

        var tailOption = new Option<int>(TailOption) { Description = "Number of lines to tail" };

        var resourceNameOption = new Option<string>(ResourceNameOption)
        {
            Description = "Specific resource to show logs for"
        };

        Add(environmentNameOption);
        Add(filterOption);
        Add(tailOption);
        Add(resourceNameOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var filter = parseResult.GetValue<string>(FilterOption);
            var tail = parseResult.GetValue<int>(TailOption);
            var resourceName = parseResult.GetValue<string>(ResourceNameOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);

            Log.Information("Executing cloud logs command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud logs");
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
            await Build.CloudEnvLogsMethod(cloudUrl, environmentName, filter, tail, resourceName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud logs command");
            return 1;
        }

        return 0;
    }
}
