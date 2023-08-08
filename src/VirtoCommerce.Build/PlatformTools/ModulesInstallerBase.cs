using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Build.PlatformTools
{
    public abstract class ModulesInstallerBase
    {
        public virtual Task Install(ModuleSource source)
        {
            try
            {
                return InnerInstall(source);
            }
            catch (Exception exception)
            {
                throw new ModuleInstallationException("Error while module installation", exception);
            }
        }

        protected abstract Task InnerInstall(ModuleSource source);
    }
}

