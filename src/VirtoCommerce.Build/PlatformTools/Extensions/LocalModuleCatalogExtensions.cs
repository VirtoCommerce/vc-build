using System.IO;
using System.Linq;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Extensions
{
    public static class LocalModuleCatalogExtensions
    {
        public static bool IsModuleSymlinked(this ILocalModuleCatalog moduleCatalog, string moduleId)
        {
            var moduleInfo = moduleCatalog.Modules.OfType<ManifestModuleInfo>()
                .FirstOrDefault(m => m.ModuleName == moduleId);
            if (moduleInfo == null)
            {
                return false;
            }

            var directoryInfo = new DirectoryInfo(moduleInfo.FullPhysicalPath);

            return IsSymlinked(directoryInfo);
        }

        private static bool IsSymlinked(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                return false;
            }

            if (directoryInfo.LinkTarget != null)
            {
                return true;
            }

            return IsSymlinked(directoryInfo.Parent);
        }
    }
}
