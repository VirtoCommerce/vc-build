using System.IO;
using System.Linq;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Extensions
{
    public static class LocalModuleCatalogExtensions
    {
        public static bool IsModuleSymlinked(this ILocalModuleCatalog moduleCatalog, string moduleId)
        {
            var moduleInfo = moduleCatalog.Modules.OfType<ManifestModuleInfo>().FirstOrDefault(m => m.ModuleName == moduleId);
            if (moduleInfo == null)
            {
                return false;
            }

            var moduleDirectory = Path.GetDirectoryName(moduleInfo.FullPhysicalPath);
            if (string.IsNullOrEmpty(moduleDirectory))
            {
                return false;
            }

            var directoryInfo = new DirectoryInfo(moduleDirectory);

            return directoryInfo.LinkTarget != null;
        }
    }
}
