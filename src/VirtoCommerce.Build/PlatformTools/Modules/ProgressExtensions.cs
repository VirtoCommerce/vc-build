using System;
using VirtoCommerce.Platform.Core.Modularity;

namespace PlatformTools.Modules
{
    public static class ProgressExtensions
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

