using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud set-parameter command
    /// </summary>
    public static class CloudSetParameterAction
    {
        /// <summary>
        /// Executes the cloud set-parameter command
        /// </summary>
        /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
        {
            try
            {
                // Extract option values from ParseResult
                var environmentName = parseResult.GetValue<string>("--environment-name");
                var helmParameters = parseResult.GetValue<string[]>("--helm-parameters");
                var organization = parseResult.GetValue<string>("--organization");

                // Validate required parameters
                if (string.IsNullOrEmpty(environmentName))
                {
                    Console.Error.WriteLine("Error: --environment-name is required");
                    return 1;
                }

                if (helmParameters == null || helmParameters.Length == 0)
                {
                    Console.Error.WriteLine("Error: --helm-parameters is required");
                    return 1;
                }

                // Set the cloud parameters for the existing Nuke.Build infrastructure
                Build.EnvironmentName = environmentName;
                Build.HelmParameters = helmParameters;

                if (!string.IsNullOrEmpty(organization))
                    Build.Organization = organization;

                // Execute the existing CloudEnvSetParameter target logic
                return Build.Execute<Build>(x => x.CloudEnvSetParameter);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error executing cloud set-parameter: {ex.Message}");
                return 1;
            }
        }
    }
}
