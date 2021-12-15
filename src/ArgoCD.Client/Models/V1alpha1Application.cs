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
    /// Application is a definition of Application resource.
    /// +genclient
    /// +genclient:noStatus
    /// +k8s:deepcopy-gen:interfaces=k8s.io/apimachinery/pkg/runtime.Object
    /// +kubebuilder:resource:path=applications,shortName=app;apps
    /// +kubebuilder:printcolumn:name="Sync
    /// Status",type=string,JSONPath=`.status.sync.status`
    /// +kubebuilder:printcolumn:name="Health
    /// Status",type=string,JSONPath=`.status.health.status`
    /// +kubebuilder:printcolumn:name="Revision",type=string,JSONPath=`.status.sync.revision`,priority=10
    /// </summary>
    public partial class V1alpha1Application
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1Application class.
        /// </summary>
        public V1alpha1Application()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1Application class.
        /// </summary>
        public V1alpha1Application(V1ObjectMeta metadata = default(V1ObjectMeta), V1alpha1Operation operation = default(V1alpha1Operation), V1alpha1ApplicationSpec spec = default(V1alpha1ApplicationSpec), V1alpha1ApplicationStatus status = default(V1alpha1ApplicationStatus))
        {
            Metadata = metadata;
            Operation = operation;
            Spec = spec;
            Status = status;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "operation")]
        public V1alpha1Operation Operation { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "spec")]
        public V1alpha1ApplicationSpec Spec { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public V1alpha1ApplicationStatus Status { get; set; }

    }
}