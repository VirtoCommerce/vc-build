using System.IO;
using System.Linq;
using PlatformTools.Modules;

namespace PlatformTools.Extensions
{
    public static class LocalModuleCatalogExtensions
    {
        public static bool IsModuleSymlinked(this LocalModuleCatalog moduleCatalog, string moduleId)
        {
            var moduleInfo = moduleCatalog.Modules.FirstOrDefault(x => x.ModuleName == moduleId);
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
