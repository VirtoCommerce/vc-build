using System.CommandLine;

namespace Commands.Cloud;

/// <summary>
///     Main cloud command that contains all cloud-related subcommands
/// </summary>
public class CloudCommand : Command
{
    public CloudCommand() : base("cloud", "Cloud environment management commands")
    {
        // Add authentication command
        Add(new CloudAuthCommand());

        // Add environment management commands
        Add(new CloudInitCommand());
        Add(new CloudUpCommand());
        Add(new CloudDownCommand());
        Add(new CloudDeployCommand());
        Add(new CloudListCommand());
        Add(new CloudRestartCommand());
        Add(new CloudLogsCommand());
        Add(new CloudStatusCommand());

        // Add environment configuration commands
        Add(new CloudSetParameterCommand());
        Add(new CloudUpdateCommand());
    }
}
