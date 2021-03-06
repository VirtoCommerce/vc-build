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
    /// RepositoryList is a collection of Repositories.
    /// </summary>
    public partial class V1alpha1RepoCredsList
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1RepoCredsList class.
        /// </summary>
        public V1alpha1RepoCredsList()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1RepoCredsList class.
        /// </summary>
        public V1alpha1RepoCredsList(IList<V1alpha1RepoCreds> items = default(IList<V1alpha1RepoCreds>), V1ListMeta metadata = default(V1ListMeta))
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
        public IList<V1alpha1RepoCreds> Items { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ListMeta Metadata { get; set; }

    }
}
