using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudDownCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";

    public CloudDownCommand() : base("down", "Delete cloud environment")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name to delete",
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
            var organization = parseResult.GetValue<string>(CloudCommand.OrganizationOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);

            Log.Information("Executing cloud down command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud down");
                return 1;
            }

            if (!string.IsNullOrEmpty(organization))
            {
                Log.Information("Organization: {Organization}", organization);
            }

            Log.Information("Delegating to CloudDown method");

            // Call CloudDown method directly
            await Build.CloudDownMethod(cloudUrl, environmentName, organization);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud down command");
            return 1;
        }

        return 0;
    }
}
