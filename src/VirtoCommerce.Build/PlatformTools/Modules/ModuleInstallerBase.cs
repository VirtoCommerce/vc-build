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

    public static class ProgressExtension
    {
        public static void ReportInfo(this IProgress<ProgressMessage> progress, string message)
        {
            progress.Report(new ProgressMessage { Level = ProgressMessageLevel.Info, Message = message });
        }
        public static void ReportError(this IProgress<ProgressMessage> progress, string message)
        {
            progress.Report(new ProgressMessage { Level = ProgressMessageLevel.Error, Message = message });
        }
    }
}

