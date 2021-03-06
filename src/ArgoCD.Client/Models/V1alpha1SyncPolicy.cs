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
    /// SyncPolicy controls when a sync will be performed in response to
    /// updates in git
    /// </summary>
    public partial class V1alpha1SyncPolicy
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1SyncPolicy class.
        /// </summary>
        public V1alpha1SyncPolicy()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1SyncPolicy class.
        /// </summary>
        /// <param name="syncOptions">Options allow you to specify whole app
        /// sync-options</param>
        public V1alpha1SyncPolicy(V1alpha1SyncPolicyAutomated automated = default(V1alpha1SyncPolicyAutomated), V1alpha1RetryStrategy retry = default(V1alpha1RetryStrategy), IList<string> syncOptions = default(IList<string>))
        {
            Automated = automated;
            Retry = retry;
            SyncOptions = syncOptions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "automated")]
        public V1alpha1SyncPolicyAutomated Automated { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "retry")]
        public V1alpha1RetryStrategy Retry { get; set; }

        /// <summary>
        /// Gets or sets options allow you to specify whole app sync-options
        /// </summary>
        [JsonProperty(PropertyName = "syncOptions")]
        public IList<string> SyncOptions { get; set; }

    }
}
