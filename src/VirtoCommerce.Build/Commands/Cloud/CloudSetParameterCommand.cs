using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudSetParameterCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string HelmParametersOption = "--helm-parameters";

    public CloudSetParameterCommand() : base("set-parameter", "Update cloud environment parameters")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Environment name",
            Required = true
        };

        var helmParametersOption = new Option<string[]>(HelmParametersOption)
        {
            Description = "Helm parameters in key=value format",
            Required = true
        };

        Add(environmentNameOption);
        Add(helmParametersOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var helmParameters = parseResult.GetValue<string[]>(HelmParametersOption);
            var organization = parseResult.GetValue<string>(CloudCommand.OrganizationOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);

            Log.Information("Executing cloud set-parameter command for environment: {EnvironmentName}",
                environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud set-parameter");
                return 1;
            }

            if (helmParameters == null || helmParameters.Length == 0)
            {
                Log.Error("Helm parameters are required for cloud set-parameter");
                return 1;
            }

            Log.Information("Helm parameters: {HelmParameters}", string.Join(", ", helmParameters));

            if (!string.IsNullOrEmpty(organization))
            {
                Log.Information("Organization: {Organization}", organization);
            }

            Log.Information("Delegating to CloudEnvSetParameter method");

            // Call CloudEnvSetParameter method directly
            await Build.CloudEnvSetParameterMethod(cloudUrl, environmentName, helmParameters, organization);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud set-parameter command");
            return 1;
        }

        return 0;
    }
}
