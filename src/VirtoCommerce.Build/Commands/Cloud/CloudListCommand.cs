using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudListCommand : Command
{
    public CloudListCommand() : base("list", "List cloud environments with statuses")
    {
        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            Log.Information("Executing cloud list command");

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
