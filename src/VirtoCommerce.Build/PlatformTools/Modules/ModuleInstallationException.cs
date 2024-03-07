using System;
using System.Runtime.Serialization;

namespace PlatformTools.Modules
{
    [Serializable]
    public class ModuleInstallationException : Exception
    {
        public ModuleInstallationException()
        {
        }

        public ModuleInstallationException(string message) : base(message)
        {
        }

        public ModuleInstallationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ModuleInstallationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
