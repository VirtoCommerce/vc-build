using System.IO;
using System.Linq;
using PlatformTools.Modules;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Extensions
{
    public static class LocalModuleCatalogExtensions
    {
        public static bool IsModuleSymlinked(this ILocalModuleCatalog moduleCatalog, string moduleId)
        {
            var moduleInfo = moduleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.ModuleName == moduleId);
            if(moduleInfo == null)
            {
                return false;
            }

            var fileInfo = new FileInfo(moduleInfo.FullPhysicalPath);

            return fileInfo.LinkTarget != null;
        }
    }
}
