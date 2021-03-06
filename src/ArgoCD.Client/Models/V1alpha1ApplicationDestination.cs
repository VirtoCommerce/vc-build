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
    /// ApplicationDestination holds information about the application's
    /// destination
    /// </summary>
    public partial class V1alpha1ApplicationDestination
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationDestination
        /// class.
        /// </summary>
        public V1alpha1ApplicationDestination()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationDestination
        /// class.
        /// </summary>
        /// <param name="name">Name is an alternate way of specifying the
        /// target cluster by its symbolic name</param>
        /// <param name="namespaceProperty">Namespace specifies the target
        /// namespace for the application's resources.
        /// The namespace will only be set for namespace-scoped resources that
        /// have not set a value for .metadata.namespace</param>
        /// <param name="server">Server specifies the URL of the target cluster
        /// and must be set to the Kubernetes control plane API</param>
        public V1alpha1ApplicationDestination(string name = default(string), string namespaceProperty = default(string), string server = default(string))
        {
            Name = name;
            NamespaceProperty = namespaceProperty;
            Server = server;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name is an alternate way of specifying the target
        /// cluster by its symbolic name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets namespace specifies the target namespace for the
        /// application's resources.
        /// The namespace will only be set for namespace-scoped resources that
        /// have not set a value for .metadata.namespace
        /// </summary>
        [JsonProperty(PropertyName = "namespace")]
        public string NamespaceProperty { get; set; }

        /// <summary>
        /// Gets or sets server specifies the URL of the target cluster and
        /// must be set to the Kubernetes control plane API
        /// </summary>
        [JsonProperty(PropertyName = "server")]
        public string Server { get; set; }

    }
}
