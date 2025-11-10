using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudRestartCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";

    public CloudRestartCommand() : base("restart", "Restart cloud environment")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name to restart",
            Required = true
        };

        Add(environmentNameOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);

            Log.Information("Executing cloud restart command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud restart");
                return 1;
            }

            Log.Information("Delegating to CloudEnvRestart method");

            // Call CloudEnvRestart method directly
            await Build.CloudEnvRestartMethod(cloudUrl, environmentName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud restart command");
            return 1;
        }

        return 0;
    }
}
