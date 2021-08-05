using Nuke.Common;

internal partial class Build
{
    [Parameter("Grab-migrator config file")]
    public static string GrabMigratorConfig { get; set; }

    public Target GrabMigrator => _ => _
        .Requires(() => GrabMigratorConfig)
        .Executes(() =>
        {
            new GrabMigrator.GrabMigrator().Do(GrabMigratorConfig);
        });
}
