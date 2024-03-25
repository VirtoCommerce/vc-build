using System.Collections.Generic;

namespace PlatformTools.Modules.LocalModules;
internal class Local : ModuleSource
{
    public override string Name { get; set; } = nameof(Local);
    public List<LocalModuleItem> Modules { get; set; }
}
