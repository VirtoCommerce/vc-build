// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ExecProviderConfig is config used to call an external command to
    /// perform cluster authentication
    /// See: https://godoc.org/k8s.io/client-go/tools/clientcmd/api#ExecConfig
    /// </summary>
    public partial class V1alpha1ExecProviderConfig
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ExecProviderConfig class.
        /// </summary>
        public V1alpha1ExecProviderConfig()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ExecProviderConfig class.
        /// </summary>
        /// <param name="apiVersion">Preferred input version of the
        /// ExecInfo</param>
        /// <param name="args">Arguments to pass to the command when executing
        /// it</param>
        /// <param name="command">Command to execute</param>
        /// <param name="env">Env defines additional environment variables to
        /// expose to the process</param>
        /// <param name="installHint">This text is shown to the user when the
        /// executable doesn't seem to be present</param>
        public V1alpha1ExecProviderConfig(string apiVersion = default(string), IList<string> args = default(IList<string>), string command = default(string), IDictionary<string, string> env = default(IDictionary<string, string>), string installHint = default(string))
        {
            ApiVersion = apiVersion;
            Args = args;
            Command = command;
            Env = env;
            InstallHint = installHint;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets preferred input version of the ExecInfo
        /// </summary>
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets arguments to pass to the command when executing it
        /// </summary>
        [JsonProperty(PropertyName = "args")]
        public IList<string> Args { get; set; }

        /// <summary>
        /// Gets or sets command to execute
        /// </summary>
        [JsonProperty(PropertyName = "command")]
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets env defines additional environment variables to expose
        /// to the process
        /// </summary>
        [JsonProperty(PropertyName = "env")]
        public IDictionary<string, string> Env { get; set; }

        /// <summary>
        /// Gets or sets this text is shown to the user when the executable
        /// doesn't seem to be present
        /// </summary>
        [JsonProperty(PropertyName = "installHint")]
        public string InstallHint { get; set; }

    }
}