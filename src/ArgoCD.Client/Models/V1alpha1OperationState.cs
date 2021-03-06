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
    /// OperationState contains information about state of a running operation
    /// </summary>
    public partial class V1alpha1OperationState
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1OperationState class.
        /// </summary>
        public V1alpha1OperationState()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1OperationState class.
        /// </summary>
        /// <param name="message">Message holds any pertinent messages when
        /// attempting to perform operation (typically errors).</param>
        /// <param name="phase">Phase is the current phase of the
        /// operation</param>
        /// <param name="retryCount">RetryCount contains time of operation
        /// retries</param>
        public V1alpha1OperationState(V1Time finishedAt = default(V1Time), string message = default(string), V1alpha1Operation operation = default(V1alpha1Operation), string phase = default(string), string retryCount = default(string), V1Time startedAt = default(V1Time), V1alpha1SyncOperationResult syncResult = default(V1alpha1SyncOperationResult))
        {
            FinishedAt = finishedAt;
            Message = message;
            Operation = operation;
            Phase = phase;
            RetryCount = retryCount;
            StartedAt = startedAt;
            SyncResult = syncResult;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "finishedAt")]
        public V1Time FinishedAt { get; set; }

        /// <summary>
        /// Gets or sets message holds any pertinent messages when attempting
        /// to perform operation (typically errors).
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "operation")]
        public V1alpha1Operation Operation { get; set; }

        /// <summary>
        /// Gets or sets phase is the current phase of the operation
        /// </summary>
        [JsonProperty(PropertyName = "phase")]
        public string Phase { get; set; }

        /// <summary>
        /// Gets or sets retryCount contains time of operation retries
        /// </summary>
        [JsonProperty(PropertyName = "retryCount")]
        public string RetryCount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "startedAt")]
        public V1Time StartedAt { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "syncResult")]
        public V1alpha1SyncOperationResult SyncResult { get; set; }

    }
}
