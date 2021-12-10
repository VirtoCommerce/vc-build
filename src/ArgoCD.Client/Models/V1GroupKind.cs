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
    /// GroupKind specifies a Group and a Kind, but does not force a version.
    /// This is useful for identifying
    /// concepts during lookup stages without having partially valid types
    /// </summary>
    /// <remarks>
    /// +protobuf.options.(gogoproto.goproto_stringer)=false
    /// </remarks>
    public partial class V1GroupKind
    {
        /// <summary>
        /// Initializes a new instance of the V1GroupKind class.
        /// </summary>
        public V1GroupKind()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1GroupKind class.
        /// </summary>
        public V1GroupKind(string group = default(string), string kind = default(string))
        {
            Group = group;
            Kind = kind;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

    }
}
