using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common.Tooling;

namespace VirtoCommerce.Build.PlatformTools.Azure
{
    [Serializable]
    internal class AzureCliToolSettings : ToolSettings
    {
        public override Action<OutputType, string> ProcessCustomLogger => ProcessTasks.DefaultLogger;
    }
}
