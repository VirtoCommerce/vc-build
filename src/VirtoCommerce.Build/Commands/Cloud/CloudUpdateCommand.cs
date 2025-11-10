using System;
using System.CommandLine;
using System.Threading.Tasks;
using Serilog;
using VirtoCommerce.Build;

namespace Commands.Cloud;

public class CloudUpdateCommand : Command
{
    public const string ManifestOption = "--manifest";
    public const string RoutesFileOption = "--routes-file";

    public CloudUpdateCommand() : base("update", "Update applications in the cloud")
    {
        var manifestOption = new Option<string>(ManifestOption)
        {
            Description = "Path to application manifest file",
            Required = true
        };

        var routesFileOption = new Option<string>(RoutesFileOption) { Description = "Path to routes file" };

        Add(manifestOption);
        Add(routesFileOption);

        SetAction(ExecuteAsync);
    }

    public static async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        try
        {
            // Extract option values from ParseResult using correct API
            var manifest = parseResult.GetValue<string>(ManifestOption);
            var routesFile = parseResult.GetValue<string>(RoutesFileOption);
            var cloudUrl = parseResult.GetValue<string>(CloudCommand.CloudUrlOption);
            var cloudToken = parseResult.GetValue<string>(CloudCommand.CloudTokenOption);
            var organization = parseResult.GetValue<string>(CloudCommand.OrganizationOption);

            Log.Information("Executing cloud update command");

            // Validate required parameters
            if (string.IsNullOrEmpty(manifest))
            {
                Log.Error("Manifest file path is required for cloud update");
                return 1;
            }

            Log.Information("Manifest file: {Manifest}", manifest);

            if (!string.IsNullOrEmpty(routesFile))
            {
                Log.Information("Routes file: {RoutesFile}", routesFile);
            }

            Log.Information("Delegating to CloudEnvUpdate method");

            // Call CloudEnvUpdate method directly
            await Build.CloudEnvUpdateMethod(manifest, routesFile, organization, cloudUrl, cloudToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing cloud update command");
            return 1;
        }

        return 0;
    }
}
