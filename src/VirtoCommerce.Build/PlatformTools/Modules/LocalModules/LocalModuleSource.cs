using System.Collections.Generic;

namespace PlatformTools.Modules.LocalModules;
internal class LocalModuleSource : ModuleSource
{
    public const string SourceName = "Local";
    public override string Name { get; set; } = SourceName;
    public List<LocalModuleItem> Modules { get; set; }
}
