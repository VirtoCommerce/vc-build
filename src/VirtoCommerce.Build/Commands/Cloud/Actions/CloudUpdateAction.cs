using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud update command
    /// </summary>
    public static class CloudUpdateAction
    {
        /// <summary>
        /// Executes the cloud update command
        /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult
                var manifest = parseResult.GetValue<string>("--manifest");
                var routesFile = parseResult.GetValue<string>("--routes-file");

                // Validate required parameters
                if (string.IsNullOrEmpty(manifest))
                {
                    Console.Error.WriteLine("Error: --manifest is required");
                    return 1;
                }

                // Set the cloud parameters for the existing Nuke.Build infrastructure
                Build.Manifest = manifest;

                if (!string.IsNullOrEmpty(routesFile))
                    Build.RoutesFile = routesFile;

                // Execute the existing CloudEnvUpdate target logic
                return Build.Execute<Build>(x => x.CloudEnvUpdate);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing cloud update: {ex.Message}");
                return 1;
            }
        }
    }
}
