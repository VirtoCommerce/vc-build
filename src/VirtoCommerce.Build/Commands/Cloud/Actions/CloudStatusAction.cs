using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud status command
    /// </summary>
    public static class CloudStatusAction
    {
        /// <summary>
        /// Executes the cloud status command
        /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult
                var environmentName = parseResult.GetValue<string>("--environment-name");
                var healthStatus = parseResult.GetValue<string>("--health-status");
                var syncStatus = parseResult.GetValue<string>("--sync-status");

                // Validate required parameters
                if (string.IsNullOrEmpty(environmentName))
                {
                    Console.Error.WriteLine("Error: --environment-name is required");
                    return 1;
                }

                // Set the cloud parameters for the existing Nuke.Build infrastructure
                Build.EnvironmentName = environmentName;

                if (!string.IsNullOrEmpty(healthStatus))
                    Build.HealthStatus = healthStatus;

                if (!string.IsNullOrEmpty(syncStatus))
                    Build.SyncStatus = syncStatus;

                // Execute the existing CloudEnvStatus target logic
                return Build.Execute<Build>(x => x.CloudEnvStatus);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing cloud status: {ex.Message}");
                return 1;
            }
        }
    }
}
