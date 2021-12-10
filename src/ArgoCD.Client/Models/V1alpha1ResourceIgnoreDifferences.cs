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
    /// ResourceIgnoreDifferences contains resource filter and list of json
    /// paths which should be ignored during comparison with live state.
    /// </summary>
    public partial class V1alpha1ResourceIgnoreDifferences
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ResourceIgnoreDifferences
        /// class.
        /// </summary>
        public V1alpha1ResourceIgnoreDifferences()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ResourceIgnoreDifferences
        /// class.
        /// </summary>
        public V1alpha1ResourceIgnoreDifferences(string group = default(string), IList<string> jsonPointers = default(IList<string>), string kind = default(string), string name = default(string), string namespaceProperty = default(string))
        {
            Group = group;
            JsonPointers = jsonPointers;
            Kind = kind;
            Name = name;
            NamespaceProperty = namespaceProperty;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "jsonPointers")]
        public IList<string> JsonPointers { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "namespace")]
        public string NamespaceProperty { get; set; }

    }
}
