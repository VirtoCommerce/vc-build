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
    /// GnuPGPublicKeyList is a collection of GnuPGPublicKey objects
    /// </summary>
    public partial class V1alpha1GnuPGPublicKeyList
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1GnuPGPublicKeyList class.
        /// </summary>
        public V1alpha1GnuPGPublicKeyList()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1GnuPGPublicKeyList class.
        /// </summary>
        public V1alpha1GnuPGPublicKeyList(IList<V1alpha1GnuPGPublicKey> items = default(IList<V1alpha1GnuPGPublicKey>), V1ListMeta metadata = default(V1ListMeta))
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
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IList<V1alpha1GnuPGPublicKey> Items { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ListMeta Metadata { get; set; }

    }
}