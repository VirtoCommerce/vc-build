using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud set-parameter command
/// </summary>
public static class CloudSetParameterAction
{
    /// <summary>
    ///     Executes the cloud set-parameter command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>("--environment-name");
            var helmParameters = parseResult.GetValue<string[]>("--helm-parameters");
            var organization = parseResult.GetValue<string>("--organization");

            Log.Information("Executing cloud set-parameter command for environment: {EnvironmentName}",
                environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud set-parameter");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            if (helmParameters == null || helmParameters.Length == 0)
            {
                Log.Error("Helm parameters are required for cloud set-parameter");
                Console.Error.WriteLine("Error: --helm-parameters is required");
                return 1;
            }

            Log.Information("Helm parameters: {HelmParameters}", string.Join(", ", helmParameters));
            if (!string.IsNullOrEmpty(organization))
            {
                Log.Information("Organization: {Organization}", organization);
            }

            Log.Information("Delegating to CloudEnvSetParameter method");

            // Call CloudEnvSetParameter method directly
            await Build.CloudEnvSetParameterMethod(environmentName, helmParameters, organization);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud set-parameter command");
            Console.Error.WriteLine($"Error executing cloud set-parameter: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
