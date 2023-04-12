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
                InnerInstall(source);
            }
            catch (Exception exception)
            {
                throw new ModuleInstallationException("Error while module installation", exception);
            }

            return Task.CompletedTask;
        }

        protected abstract Task InnerInstall(ModuleSource source);
    }
}

