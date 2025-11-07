using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudInitCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string ServicePlanOption = "--service-plan";
    public const string ClusterNameOption = "--cluster-name";
    public const string OrganizationOption = "--organization";
    public const string DbProviderOption = "--db-provider";
    public const string DbNameOption = "--db-name";
    public const string AppProjectOption = "--app-project";
    public const string CloudUrlOption = "--url";

    public CloudInitCommand() : base("init", "Create a new cloud environment")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Name of the new environment",
            Required = true
        };

        var servicePlanOption = new Option<string>(ServicePlanOption)
        {
            Description = "Service plan for the environment",
            DefaultValueFactory = _ => "F1"
        };

        var clusterNameOption = new Option<string>(ClusterNameOption) { Description = "Target cluster name" };

        var organizationOption = new Option<string>(OrganizationOption) { Description = "Organization name" };

        var dbProviderOption = new Option<string>(DbProviderOption) { Description = "Database provider" };

        var dbNameOption = new Option<string>(DbNameOption) { Description = "Database name" };

        var appProjectOption = new Option<string>(AppProjectOption) { Description = "Application project name" };

        var cloudUrlOption = new Option<string>(CloudUrlOption)
        {
            Description = "VirtoCloud URL",
            DefaultValueFactory = _ => "https://portal.virtocommerce.cloud"
        };

        Add(environmentNameOption);
        Add(servicePlanOption);
        Add(clusterNameOption);
        Add(organizationOption);
        Add(dbProviderOption);
        Add(dbNameOption);
        Add(appProjectOption);
        Add(cloudUrlOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var servicePlan = parseResult.GetValue<string>(ServicePlanOption) ?? "F1";
            var clusterName = parseResult.GetValue<string>(ClusterNameOption);
            var organization = parseResult.GetValue<string>(OrganizationOption);
            var dbProvider = parseResult.GetValue<string>(DbProviderOption);
            var dbName = parseResult.GetValue<string>(DbNameOption);
            var appProject = parseResult.GetValue<string>(AppProjectOption);
            var cloudUrl = parseResult.GetValue<string>(CloudUrlOption);

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

            if (!string.IsNullOrEmpty(dbProvider))
            {
                Log.Information("Database provider: {DbProvider}", dbProvider);
            }

            if (!string.IsNullOrEmpty(dbName))
            {
                Log.Information("Database name: {DbName}", dbName);
            }

            Log.Information("Delegating to CloudInit method");

            // Call CloudInit method directly with all parameters including database options
            await Build.CloudInitMethod(environmentName, servicePlan, clusterName, organization,
                appProject, dbProvider, dbName, cloudUrl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud init command");
            Console.Error.WriteLine($"Error executing cloud init: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
