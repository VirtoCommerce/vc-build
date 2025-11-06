using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace VirtoCommerce.Build.Commands.Cloud.Actions
{
    /// <summary>
    /// Action for cloud down command
    /// </summary>
    public static class CloudDownAction
    {
        /// <summary>
        /// Executes the cloud down command
        /// </summary>
      /// <param name="parseResult">Command line parse result</param>
        /// <returns>Task representing the operation</returns>
        public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
    try
            {
              // Extract option values from ParseResult using correct API
   var environmentName = parseResult.GetValue<string>("--environment-name");
          var organization = parseResult.GetValue<string>("--organization");

       Log.Information("Executing cloud down command for environment: {EnvironmentName}", environmentName);

        // Validate required parameters
  if (string.IsNullOrEmpty(environmentName))
                {
           Log.Error("Environment name is required for cloud down");
           Console.Error.WriteLine("Error: --environment-name is required");
  return 1;
                }

       // Set the cloud parameters for the existing Nuke.Build infrastructure
      Build.EnvironmentName = environmentName;

        if (!string.IsNullOrEmpty(organization))
         {
         Build.Organization = organization;
         Log.Information("Organization: {Organization}", organization);
         }

     Log.Information("Delegating to existing CloudDown target");

   // Execute the existing CloudDown target logic
                return Build.Execute<Build>(x => x.CloudDown);
    }
          catch (Exception ex)
  {
  Log.Error(ex, "Error executing cloud down command");
       Console.Error.WriteLine($"Error executing cloud down: {ex.Message}");
    return 1;
            }
        }
    }
}
