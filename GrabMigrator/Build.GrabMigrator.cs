using Nuke.Common;

internal partial class Build : NukeBuild
{
    [Parameter("Grab-migrator config file")]
    private readonly string GrabMigratorConfig;

    private Target GrabMigrator => _ => _
        .Requires(() => GrabMigratorConfig)
        .Executes(() =>
        {
            new GrabMigrator.GrabMigrator().Do(GrabMigratorConfig);
        });
}
