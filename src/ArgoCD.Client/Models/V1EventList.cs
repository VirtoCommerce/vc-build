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
    /// EventList is a list of events.
    /// </summary>
    public partial class V1EventList
    {
        /// <summary>
        /// Initializes a new instance of the V1EventList class.
        /// </summary>
        public V1EventList()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1EventList class.
        /// </summary>
        /// <param name="items">List of events</param>
        public V1EventList(IList<V1Event> items = default(IList<V1Event>), V1ListMeta metadata = default(V1ListMeta))
        {
            Items = items;
            Metadata = metadata;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets list of events
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IList<V1Event> Items { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ListMeta Metadata { get; set; }

    }
}