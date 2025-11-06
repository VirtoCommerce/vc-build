using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud init command
/// </summary>
public static class CloudInitAction
{
    /// <summary>
    ///     Executes the cloud init command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>("--environment-name");
            var servicePlan = parseResult.GetValue<string>("--service-plan") ?? "F1";
            var clusterName = parseResult.GetValue<string>("--cluster-name");
            var organization = parseResult.GetValue<string>("--organization");

            Log.Information("Executing cloud init command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud init");
                Console.Error.WriteLine("Error: --environment-name is required");
                return 1;
            }

            Log.Information("Using service plan: {ServicePlan}", servicePlan);
            if (!string.IsNullOrEmpty(clusterName))
            {
                Log.Information("Target cluster: {ClusterName}", clusterName);
            }

            if (!string.IsNullOrEmpty(organization))
            {
                Log.Information("Organization: {Organization}", organization);
            }

            // Set the cloud parameters for the existing Nuke.Build infrastructure
            Build.EnvironmentName = environmentName;
            Build.ServicePlan = servicePlan;

            if (!string.IsNullOrEmpty(clusterName))
            {
                Build.ClusterName = clusterName;
            }

            if (!string.IsNullOrEmpty(organization))
            {
                Build.Organization = organization;
            }

            Log.Information("Delegating to existing CloudInit target");

            // Execute the existing CloudInit target logic
            return Build.Execute<Build>(x => x.CloudInit);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud init command");
            Console.Error.WriteLine($"Error executing cloud init: {ex.Message}");
            return 1;
        }
    }
}
