using System;
using Nuke.Common.Tooling;

namespace VirtoCommerce.Build.PlatformTools.Azure
{
    [Serializable]
    internal class AzureCliToolSettings : ToolSettings
    {
        public override Action<OutputType, string> ProcessCustomLogger => ProcessTasks.DefaultLogger;
    }
}
