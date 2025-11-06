using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud logs command
    /// </summary>
    public static class CloudLogsAction
    {
        /// <summary>
        /// Executes the cloud logs command
        /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult
                var environmentName = parseResult.GetValue<string>("--environment-name");
                var filter = parseResult.GetValue<string>("--filter");
                var tail = parseResult.GetValue<int>("--tail");
                var resourceName = parseResult.GetValue<string>("--resource-name");

                // Validate required parameters
                if (string.IsNullOrEmpty(environmentName))
                {
                    Console.Error.WriteLine("Error: --environment-name is required");
                    return 1;
                }

                // Set the cloud parameters for the existing Nuke.Build infrastructure
                Build.EnvironmentName = environmentName;

                if (!string.IsNullOrEmpty(filter))
                    Build.Filter = filter;

                if (tail > 0)
                    Build.Tail = tail;

                if (!string.IsNullOrEmpty(resourceName))
                    Build.ResourceName = resourceName;

                // Execute the existing CloudEnvLogs target logic
                return Build.Execute<Build>(x => x.CloudEnvLogs);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing cloud logs: {ex.Message}");
                return 1;
            }
        }
    }
}
