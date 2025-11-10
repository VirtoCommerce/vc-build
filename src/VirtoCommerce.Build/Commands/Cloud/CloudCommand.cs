using System;
using System.CommandLine;
using System.IO;

namespace Commands.Cloud;

/// <summary>
///     Main cloud command that contains all cloud-related subcommands
/// </summary>
public class CloudCommand : Command
{
    public const string CloudUrlOption = "--url";
    public const string OrganizationOption = "--organization";
    public const string CloudTokenOption = "--token";

    private readonly string _defaultCloudTokenFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vc-build", "cloud");

    public CloudCommand() : base("cloud", "Cloud environment management commands")
    {
        // Options
        var cloudUrlOption = new Option<string>(CloudUrlOption)
        {
            Description = "VirtoCloud URL",
            DefaultValueFactory = _ => "https://portal.virtocommerce.cloud"
        };

        var organizationOption = new Option<string>(OrganizationOption) { Description = "Organization name" };

        var cloudTokenOption = new Option<string>(CloudTokenOption)
        {
            Description = "Cloud authentication token",
            DefaultValueFactory = parseResult =>
            {
                if (parseResult.Tokens.Count == 0 && File.Exists(_defaultCloudTokenFilePath))
                {
                    return File.ReadAllText(_defaultCloudTokenFilePath);
                }

                parseResult.AddError("Cloud authentication token is required.");
                return null;
            }
        };

        Add(cloudUrlOption);
        Add(organizationOption);
        Add(cloudTokenOption);

        //Commands
        Add(new CloudAuthCommand());
        Add(new CloudInitCommand());
        Add(new CloudUpCommand());
        Add(new CloudDownCommand());
        Add(new CloudDeployCommand());
        Add(new CloudListCommand());
        Add(new CloudRestartCommand());
        Add(new CloudLogsCommand());
        Add(new CloudStatusCommand());
        Add(new CloudSetParameterCommand());
        Add(new CloudUpdateCommand());
    }
}
