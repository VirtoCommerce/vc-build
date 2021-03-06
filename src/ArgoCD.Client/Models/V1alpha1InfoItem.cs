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
    /// InfoItem contains arbitrary, human readable information about an
    /// application
    /// </summary>
    public partial class V1alpha1InfoItem
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1InfoItem class.
        /// </summary>
        public V1alpha1InfoItem()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1InfoItem class.
        /// </summary>
        /// <param name="name">Name is a human readable title for this piece of
        /// information.</param>
        /// <param name="value">Value is human readable content.</param>
        public V1alpha1InfoItem(string name = default(string), string value = default(string))
        {
            Name = name;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name is a human readable title for this piece of
        /// information.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value is human readable content.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

    }
}
