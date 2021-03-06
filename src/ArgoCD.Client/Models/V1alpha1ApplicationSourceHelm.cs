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
    /// ApplicationSourceHelm holds helm specific options
    /// </summary>
    public partial class V1alpha1ApplicationSourceHelm
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationSourceHelm
        /// class.
        /// </summary>
        public V1alpha1ApplicationSourceHelm()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationSourceHelm
        /// class.
        /// </summary>
        /// <param name="fileParameters">FileParameters are file parameters to
        /// the helm template</param>
        /// <param name="parameters">Parameters is a list of Helm parameters
        /// which are passed to the helm template command upon manifest
        /// generation</param>
        /// <param name="releaseName">ReleaseName is the Helm release name to
        /// use. If omitted it will use the application name</param>
        /// <param name="valueFiles">ValuesFiles is a list of Helm value files
        /// to use when generating a template</param>
        /// <param name="values">Values specifies Helm values to be passed to
        /// helm template, typically defined as a block</param>
        /// <param name="version">Version is the Helm version to use for
        /// templating (either "2" or "3")</param>
        public V1alpha1ApplicationSourceHelm(IList<V1alpha1HelmFileParameter> fileParameters = default(IList<V1alpha1HelmFileParameter>), IList<V1alpha1HelmParameter> parameters = default(IList<V1alpha1HelmParameter>), string releaseName = default(string), IList<string> valueFiles = default(IList<string>), string values = default(string), string version = default(string))
        {
            FileParameters = fileParameters;
            Parameters = parameters;
            ReleaseName = releaseName;
            ValueFiles = valueFiles;
            Values = values;
            Version = version;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets fileParameters are file parameters to the helm
        /// template
        /// </summary>
        [JsonProperty(PropertyName = "fileParameters")]
        public IList<V1alpha1HelmFileParameter> FileParameters { get; set; }

        /// <summary>
        /// Gets or sets parameters is a list of Helm parameters which are
        /// passed to the helm template command upon manifest generation
        /// </summary>
        [JsonProperty(PropertyName = "parameters")]
        public IList<V1alpha1HelmParameter> Parameters { get; set; }

        /// <summary>
        /// Gets or sets releaseName is the Helm release name to use. If
        /// omitted it will use the application name
        /// </summary>
        [JsonProperty(PropertyName = "releaseName")]
        public string ReleaseName { get; set; }

        /// <summary>
        /// Gets or sets valuesFiles is a list of Helm value files to use when
        /// generating a template
        /// </summary>
        [JsonProperty(PropertyName = "valueFiles")]
        public IList<string> ValueFiles { get; set; }

        /// <summary>
        /// Gets or sets values specifies Helm values to be passed to helm
        /// template, typically defined as a block
        /// </summary>
        [JsonProperty(PropertyName = "values")]
        public string Values { get; set; }

        /// <summary>
        /// Gets or sets version is the Helm version to use for templating
        /// (either "2" or "3")
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

    }
}
