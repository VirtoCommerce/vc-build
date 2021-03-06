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
    /// Backoff is the backoff strategy to use on subsequent retries for
    /// failing syncs
    /// </summary>
    public partial class V1alpha1Backoff
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1Backoff class.
        /// </summary>
        public V1alpha1Backoff()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1Backoff class.
        /// </summary>
        /// <param name="duration">Duration is the amount to back off. Default
        /// unit is seconds, but could also be a duration (e.g. "2m",
        /// "1h")</param>
        /// <param name="factor">Factor is a factor to multiply the base
        /// duration after each failed retry</param>
        /// <param name="maxDuration">MaxDuration is the maximum amount of time
        /// allowed for the backoff strategy</param>
        public V1alpha1Backoff(string duration = default(string), string factor = default(string), string maxDuration = default(string))
        {
            Duration = duration;
            Factor = factor;
            MaxDuration = maxDuration;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets duration is the amount to back off. Default unit is
        /// seconds, but could also be a duration (e.g. "2m", "1h")
        /// </summary>
        [JsonProperty(PropertyName = "duration")]
        public string Duration { get; set; }

        /// <summary>
        /// Gets or sets factor is a factor to multiply the base duration after
        /// each failed retry
        /// </summary>
        [JsonProperty(PropertyName = "factor")]
        public string Factor { get; set; }

        /// <summary>
        /// Gets or sets maxDuration is the maximum amount of time allowed for
        /// the backoff strategy
        /// </summary>
        [JsonProperty(PropertyName = "maxDuration")]
        public string MaxDuration { get; set; }

    }
}
