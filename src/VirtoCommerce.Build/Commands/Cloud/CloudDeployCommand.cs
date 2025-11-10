using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudDeployCommand : Command
{
    public const string EnvironmentNameOption = "--environment-name";
    public const string DockerUsernameOption = "--docker-username";
    public const string DockerPasswordOption = "--docker-password";
    public const string DockerRegistryUrlOption = "--docker-registry-url";
    public const string DockerImageNameOption = "--docker-image-name";
    public const string DockerImageTagOption = "--docker-image-tag";

    public CloudDeployCommand() : base("deploy", "Deploy custom Docker image to existing environment")
    {
        var environmentNameOption = new Option<string>(EnvironmentNameOption)
        {
            Description = "Target environment name",
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

        var dockerRegistryUrlOption = new Option<string>(DockerRegistryUrlOption)
        {
            Description = "Docker registry URL"
        };

        var dockerImageNameOption = new Option<string>(DockerImageNameOption)
        {
            Description = "Custom Docker image name"
        };

        var dockerImageTagOption = new Option<string>(DockerImageTagOption) { Description = "Docker image tag" };

        Add(environmentNameOption);
        Add(dockerUsernameOption);
        Add(dockerPasswordOption);
        Add(dockerRegistryUrlOption);
        Add(dockerImageNameOption);
        Add(dockerImageTagOption);

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
            var dockerRegistryUrl = parseResult.GetValue<string>(DockerRegistryUrlOption);
            var dockerImageName = parseResult.GetValue<string>(DockerImageNameOption);
            var dockerImageTag = parseResult.GetValue<string>(DockerImageTagOption);
            var organization = parseResult.GetValue<string>(CloudCommand.OrganizationOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);
            var token = parseResult.GetValue<string>(CloudCommand.CloudTokenOption);

            Log.Information("Executing cloud deploy command for environment: {EnvironmentName}", environmentName);

            // Validate required parameters
            if (string.IsNullOrEmpty(environmentName))
            {
                Log.Error("Environment name is required for cloud deploy");
                return 1;
            }

            if (string.IsNullOrEmpty(dockerUsername))
            {
                Log.Error("Docker username is required for cloud deploy");
                return 1;
            }

            if (string.IsNullOrEmpty(dockerPassword))
            {
                Log.Error("Docker password is required for cloud deploy");
                return 1;
            }

            Log.Information(
                "Docker configuration - Username: {DockerUsername}, Registry: {DockerRegistry}, Image: {DockerImage}:{DockerTag}",
                dockerUsername, dockerRegistryUrl ?? "default", dockerImageName ?? "auto-generated",
                dockerImageTag ?? "auto-generated");

            Log.Information("Delegating to CloudDeploy workflow");

            // Call CloudDeploy workflow directly
            await Build.CloudDeployMethod(environmentName, dockerUsername, dockerPassword,
                dockerRegistryUrl, dockerImageName, dockerImageTag, organization, cloudUrl, token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud deploy command");
            return 1;
        }

        return 0;
    }
}
