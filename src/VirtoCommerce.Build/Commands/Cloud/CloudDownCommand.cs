using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudDownCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string OrganizationOption = "--organization";

    public CloudDownCommand() : base("down", "Delete cloud environment")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name to delete",
            Required = true
        };

        var organizationOption = new Option<string>(OrganizationOption) { Description = "Organization name" };

        Add(environmentNameOption);
        Add(organizationOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var organization = parseResult.GetValue<string>(OrganizationOption);

            Log.Information("Executing cloud down command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud down");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            if (!string.IsNullOrEmpty(organization))
            {
                Log.Information("Organization: {Organization}", organization);
            }

            Log.Information("Delegating to CloudDown method");

            // Call CloudDown method directly
            await Build.CloudDownMethod(environmentName, organization);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud down command");
            Console.Error.WriteLine($"Error executing cloud down: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
