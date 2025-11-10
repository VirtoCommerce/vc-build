using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudUpCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string DockerUsernameOption = "--docker-username";
    public const string DockerPasswordOption = "--docker-password";
    public const string ServicePlanOption = "--service-plan";
    public const string DockerRegistryUrlOption = "--docker-registry-url";
    public const string DockerImageNameOption = "--docker-image-name";
    public const string DockerImageTagOption = "--docker-image-tag";
    public const string ClusterNameOption = "--cluster-name";
    public const string DbProviderOption = "--db-provider";
    public const string DbNameOption = "--db-name";

    public CloudUpCommand() : base("up", "Create a new environment and deploy custom Docker image")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Name of the new environment",
            Required = true
        };

        var dockerUsernameOption = new Option<string>(DockerUsernameOption)
        {
            Description = "Docker Hub username",
            Required = true
        };

        var dockerPasswordOption = new Option<string>(DockerPasswordOption)
        {
            Description = "Docker registry password",
            Required = true
        };

        var servicePlanOption = new Option<string>(ServicePlanOption)
        {
            Description = "Service plan for the environment",
            DefaultValueFactory = _ => "F1"
        };

        var dockerRegistryUrlOption = new Option<string>(DockerRegistryUrlOption)
        {
            Description = "Docker registry URL"
        };

        var dockerImageNameOption = new Option<string>(DockerImageNameOption)
        {
            Description = "Custom Docker image name"
        };

        var dockerImageTagOption = new Option<string>(DockerImageTagOption) { Description = "Docker image tag" };

        var clusterNameOption = new Option<string>(ClusterNameOption) { Description = "Target cluster name" };

        var dbProviderOption = new Option<string>(DbProviderOption) { Description = "Database provider" };

        var dbNameOption = new Option<string>(DbNameOption) { Description = "Database name" };

        Add(environmentNameOption);
        Add(dockerUsernameOption);
        Add(dockerPasswordOption);
        Add(servicePlanOption);
        Add(dockerRegistryUrlOption);
        Add(dockerImageNameOption);
        Add(dockerImageTagOption);
        Add(clusterNameOption);
        Add(dbProviderOption);
        Add(dbNameOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var environmentName = parseResult.GetValue<string>(EnvironmentNameOption);
            var dockerUsername = parseResult.GetValue<string>(DockerUsernameOption);
            var dockerPassword = parseResult.GetValue<string>(DockerPasswordOption);
            var servicePlan = parseResult.GetValue<string>(ServicePlanOption) ?? "F1";
            var dockerRegistryUrl = parseResult.GetValue<string>(DockerRegistryUrlOption);
            var dockerImageName = parseResult.GetValue<string>(DockerImageNameOption);
            var dockerImageTag = parseResult.GetValue<string>(DockerImageTagOption);
            var clusterName = parseResult.GetValue<string>(ClusterNameOption);
            var dbProvider = parseResult.GetValue<string>(DbProviderOption);
            var dbName = parseResult.GetValue<string>(DbNameOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);
            var organization = parseResult.GetValue<string>(CloudCommand.OrganizationOption);

            Log.Information("Executing cloud up command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud up");
                return 1;
            }

            if (string.IsNullOrEmpty(dockerUsername))
            {
                Log.Error("Docker username is required for cloud up");
                return 1;
            }

            if (string.IsNullOrEmpty(dockerPassword))
            {
                Log.Error("Docker password is required for cloud up");
                return 1;
            }

            Log.Information(
                "Docker configuration - Username: {DockerUsername}, Registry: {DockerRegistry}, Image: {DockerImage}:{DockerTag}",
                dockerUsername, dockerRegistryUrl ?? "default", dockerImageName ?? "auto-generated",
                dockerImageTag ?? "auto-generated");
            Log.Information("Service plan: {ServicePlan}", servicePlan);

            if (!string.IsNullOrEmpty(clusterName))
            {
                Log.Information("Target cluster: {ClusterName}", clusterName);
            }

            if (!string.IsNullOrEmpty(dbProvider))
            {
                Log.Information("Database provider: {DbProvider}", dbProvider);
            }

            Log.Information("Delegating to CloudUp workflow");

            // Call CloudUp workflow directly
            await Build.CloudUpMethod(cloudUrl, environmentName, dockerUsername, dockerPassword, servicePlan,
                dockerRegistryUrl, dockerImageName, dockerImageTag, clusterName, organization, dbProvider, dbName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud up command");
            return 1;
        }

        return 0;
    }
}
