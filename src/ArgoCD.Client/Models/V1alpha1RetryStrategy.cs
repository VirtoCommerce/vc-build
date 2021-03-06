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
    /// RetryStrategy contains information about the strategy to apply when a
    /// sync failed
    /// </summary>
    public partial class V1alpha1RetryStrategy
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1RetryStrategy class.
        /// </summary>
        public V1alpha1RetryStrategy()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1RetryStrategy class.
        /// </summary>
        /// <param name="limit">Limit is the maximum number of attempts for
        /// retrying a failed sync. If set to 0, no retries will be
        /// performed.</param>
        public V1alpha1RetryStrategy(V1alpha1Backoff backoff = default(V1alpha1Backoff), string limit = default(string))
        {
            Backoff = backoff;
            Limit = limit;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "backoff")]
        public V1alpha1Backoff Backoff { get; set; }

        /// <summary>
        /// Gets or sets limit is the maximum number of attempts for retrying a
        /// failed sync. If set to 0, no retries will be performed.
        /// </summary>
        [JsonProperty(PropertyName = "limit")]
        public string Limit { get; set; }

    }
}
