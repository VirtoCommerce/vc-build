using System;
using System.Runtime.Serialization;

namespace VirtoCommerce.Build.PlatformTools
{
    [Serializable]
    internal class ModuleInstallationException : Exception
    {
        private Exception exception;

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
