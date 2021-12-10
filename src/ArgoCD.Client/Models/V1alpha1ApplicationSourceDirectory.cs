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
    /// ApplicationSourceDirectory holds options for applications of type plain
    /// YAML or Jsonnet
    /// </summary>
    public partial class V1alpha1ApplicationSourceDirectory
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V1alpha1ApplicationSourceDirectory class.
        /// </summary>
        public V1alpha1ApplicationSourceDirectory()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V1alpha1ApplicationSourceDirectory class.
        /// </summary>
        /// <param name="exclude">Exclude contains a glob pattern to match
        /// paths against that should be explicitly excluded from being used
        /// during manifest generation</param>
        /// <param name="include">Include contains a glob pattern to match
        /// paths against that should be explicitly included during manifest
        /// generation</param>
        /// <param name="recurse">Recurse specifies whether to scan a directory
        /// recursively for manifests</param>
        public V1alpha1ApplicationSourceDirectory(string exclude = default(string), string include = default(string), V1alpha1ApplicationSourceJsonnet jsonnet = default(V1alpha1ApplicationSourceJsonnet), bool? recurse = default(bool?))
        {
            Exclude = exclude;
            Include = include;
            Jsonnet = jsonnet;
            Recurse = recurse;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets exclude contains a glob pattern to match paths against
        /// that should be explicitly excluded from being used during manifest
        /// generation
        /// </summary>
        [JsonProperty(PropertyName = "exclude")]
        public string Exclude { get; set; }

        /// <summary>
        /// Gets or sets include contains a glob pattern to match paths against
        /// that should be explicitly included during manifest generation
        /// </summary>
        [JsonProperty(PropertyName = "include")]
        public string Include { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "jsonnet")]
        public V1alpha1ApplicationSourceJsonnet Jsonnet { get; set; }

        /// <summary>
        /// Gets or sets recurse specifies whether to scan a directory
        /// recursively for manifests
        /// </summary>
        [JsonProperty(PropertyName = "recurse")]
        public bool? Recurse { get; set; }

    }
}
