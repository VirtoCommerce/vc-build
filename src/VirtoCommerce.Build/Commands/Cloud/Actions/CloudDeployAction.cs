using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud deploy command
    /// </summary>
    public static class CloudDeployAction
    {
        /// <summary>
        /// Executes the cloud deploy command
        /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult using correct API
                var environmentName = parseResult.GetValue<string>("--environment-name");
                var dockerUsername = parseResult.GetValue<string>("--docker-username");
                var dockerPassword = parseResult.GetValue<string>("--docker-password");
                var dockerRegistryUrl = parseResult.GetValue<string>("--docker-registry-url");
                var dockerImageName = parseResult.GetValue<string>("--docker-image-name");
                var dockerImageTag = parseResult.GetValue<string[]>("--docker-image-tag");
                var organization = parseResult.GetValue<string>("--organization");

                Log.Information("Executing cloud deploy command for environment: {EnvironmentName}", environmentName);

                // Validate required parameters
                if (string.IsNullOrEmpty(environmentName))
                {
                    Log.Error("Environment name is required for cloud deploy");
                    Console.Error.WriteLine("Error: --environment-name is required");
                    return 1;
                }

                if (string.IsNullOrEmpty(dockerUsername))
                {
                    Log.Error("Docker username is required for cloud deploy");
                    Console.Error.WriteLine("Error: --docker-username is required");
                    return 1;
                }

                if (string.IsNullOrEmpty(dockerPassword))
                {
                    Log.Error("Docker password is required for cloud deploy");
                    Console.Error.WriteLine("Error: --docker-password is required");
                    return 1;
                }

                Log.Information("Docker configuration - Username: {DockerUsername}, Registry: {DockerRegistry}, Image: {DockerImage}:{DockerTag}",
                      dockerUsername, dockerRegistryUrl ?? "default", dockerImageName ?? "auto-generated", dockerImageTag ?? ["auto-generated"]);

                // Set the cloud parameters for the existing Nuke.Build infrastructure
                Build.EnvironmentName = environmentName;
                Build.DockerUsername = dockerUsername;
                Build.DockerPassword = dockerPassword;

                if (!string.IsNullOrEmpty(dockerRegistryUrl))
                    Build.DockerRegistryUrl = dockerRegistryUrl;

                if (!string.IsNullOrEmpty(dockerImageName))
                    Build.DockerImageName = dockerImageName;

                if (!string.IsNullOrEmpty(dockerImageTag))
                    Build.DockerImageTag = dockerImageTag;

                if (!string.IsNullOrEmpty(organization))
                    Build.Organization = organization;

                Log.Information("Delegating to existing CloudDeploy target");

                // Execute the existing CloudDeploy target logic
                return Build.Execute<Build>(x => x.CloudDeploy);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error executing cloud deploy command");
                Console.Error.WriteLine($"Error executing cloud deploy: {ex.Message}");
                return 1;
            }
        }
    }
}
