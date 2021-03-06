// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// HelmFileParameter is a file parameter that's passed to helm template
    /// during manifest generation
    /// </summary>
    public partial class V1alpha1HelmFileParameter
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1HelmFileParameter class.
        /// </summary>
        public V1alpha1HelmFileParameter()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1HelmFileParameter class.
        /// </summary>
        /// <param name="name">Name is the name of the Helm parameter</param>
        /// <param name="path">Path is the path to the file containing the
        /// values for the Helm parameter</param>
        public V1alpha1HelmFileParameter(string name = default(string), string path = default(string))
        {
            Name = name;
            Path = path;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name is the name of the Helm parameter
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets path is the path to the file containing the values for
        /// the Helm parameter
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

    }
}
