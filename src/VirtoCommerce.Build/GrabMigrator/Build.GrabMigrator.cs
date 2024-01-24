using Nuke.Common;
using GrabMigratorNamespace = GrabMigrator;

namespace VirtoCommerce.Build
{
    internal partial class Build
    {
        [Parameter("Grab-migrator config file")]
        public static string GrabMigratorConfig { get; set; }

        public Target GrabMigrator => _ => _
            .Requires(() => GrabMigratorConfig)
            .Executes(() =>
            {
               GrabMigratorNamespace.GrabMigrator.Do(GrabMigratorConfig);
            });
    }
}
