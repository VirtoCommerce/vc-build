using System;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules
{
    public abstract class ModuleInstallerBase
    {
        public virtual Task Install(ModuleSource source, IProgress<ProgressMessage> progress)
        {
            try
            {
                return InnerInstall(source, progress);
            }
            catch (Exception exception)
            {
                progress.ReportError(exception.Message);
                throw new ModuleInstallationException("Error while module installation", exception);
            }
        }

        protected abstract Task InnerInstall(ModuleSource source, IProgress<ProgressMessage> progress);
    }
}

