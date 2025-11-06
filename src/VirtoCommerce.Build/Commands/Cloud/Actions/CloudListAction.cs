using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;

namespace VirtoCommerce.Build.Commands.Cloud.Actions;

/// <summary>
///     Action for cloud list command
/// </summary>
public static class CloudListAction
{
    /// <summary>
    ///     Executes the cloud list command
    /// </summary>
    /// <param name="parseResult">Command line parse result</param>
    /// <returns>Task representing the operation</returns>
    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            Log.Information("Executing cloud list command");

            // Call CloudEnvList method directly
            await Build.CloudEnvListMethod();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud list command");
            Console.Error.WriteLine($"Error executing cloud list: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
