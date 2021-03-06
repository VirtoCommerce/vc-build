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
    /// ApplicationSourceKustomize holds options specific to an Application
    /// source specific to Kustomize
    /// </summary>
    public partial class V1alpha1ApplicationSourceKustomize
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V1alpha1ApplicationSourceKustomize class.
        /// </summary>
        public V1alpha1ApplicationSourceKustomize()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V1alpha1ApplicationSourceKustomize class.
        /// </summary>
        /// <param name="commonAnnotations">CommonAnnotations is a list of
        /// additional annotations to add to rendered manifests</param>
        /// <param name="commonLabels">CommonLabels is a list of additional
        /// labels to add to rendered manifests</param>
        /// <param name="images">Images is a list of Kustomize image override
        /// specifications</param>
        /// <param name="namePrefix">NamePrefix is a prefix appended to
        /// resources for Kustomize apps</param>
        /// <param name="nameSuffix">NameSuffix is a suffix appended to
        /// resources for Kustomize apps</param>
        /// <param name="version">Version controls which version of Kustomize
        /// to use for rendering manifests</param>
        public V1alpha1ApplicationSourceKustomize(IDictionary<string, string> commonAnnotations = default(IDictionary<string, string>), IDictionary<string, string> commonLabels = default(IDictionary<string, string>), IList<string> images = default(IList<string>), string namePrefix = default(string), string nameSuffix = default(string), string version = default(string))
        {
            CommonAnnotations = commonAnnotations;
            CommonLabels = commonLabels;
            Images = images;
            NamePrefix = namePrefix;
            NameSuffix = nameSuffix;
            Version = version;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets commonAnnotations is a list of additional annotations
        /// to add to rendered manifests
        /// </summary>
        [JsonProperty(PropertyName = "commonAnnotations")]
        public IDictionary<string, string> CommonAnnotations { get; set; }

        /// <summary>
        /// Gets or sets commonLabels is a list of additional labels to add to
        /// rendered manifests
        /// </summary>
        [JsonProperty(PropertyName = "commonLabels")]
        public IDictionary<string, string> CommonLabels { get; set; }

        /// <summary>
        /// Gets or sets images is a list of Kustomize image override
        /// specifications
        /// </summary>
        [JsonProperty(PropertyName = "images")]
        public IList<string> Images { get; set; }

        /// <summary>
        /// Gets or sets namePrefix is a prefix appended to resources for
        /// Kustomize apps
        /// </summary>
        [JsonProperty(PropertyName = "namePrefix")]
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets nameSuffix is a suffix appended to resources for
        /// Kustomize apps
        /// </summary>
        [JsonProperty(PropertyName = "nameSuffix")]
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets version controls which version of Kustomize to use for
        /// rendering manifests
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

    }
}
