using System;
using Nuke.Common.Tooling;

namespace PlatformTools.Modules.Azure
{
    [Serializable]
    internal class AzureCliToolSettings : ToolSettings
    {
        public override Action<OutputType, string> ProcessLogger => ProcessTasks.DefaultLogger;
    }
}
