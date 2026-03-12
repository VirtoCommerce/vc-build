using System.Collections.Generic;

namespace PlatformTools.Modules.LocalModules;
internal class LocalModuleSource : ModuleSource
{
    public override string Name { get; set; } = "Local";
    public List<LocalModuleItem> Modules { get; set; }
}
