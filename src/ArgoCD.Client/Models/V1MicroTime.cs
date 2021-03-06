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
    /// MicroTime is version of Time with microsecond level precision.
    ///
    /// +protobuf.options.marshal=false
    /// +protobuf.as=Timestamp
    /// +protobuf.options.(gogoproto.goproto_stringer)=false
    /// </summary>
    public partial class V1MicroTime
    {
        /// <summary>
        /// Initializes a new instance of the V1MicroTime class.
        /// </summary>
        public V1MicroTime()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1MicroTime class.
        /// </summary>
        /// <param name="nanos">Non-negative fractions of a second at
        /// nanosecond resolution. Negative
        /// second values with fractions must still have non-negative nanos
        /// values
        /// that count forward in time. Must be from 0 to 999,999,999
        /// inclusive. This field may be limited in precision depending on
        /// context.</param>
        /// <param name="seconds">Represents seconds of UTC time since Unix
        /// epoch
        /// 1970-01-01T00:00:00Z. Must be from 0001-01-01T00:00:00Z to
        /// 9999-12-31T23:59:59Z inclusive.</param>
        public V1MicroTime(int? nanos = default(int?), string seconds = default(string))
        {
            Nanos = nanos;
            Seconds = seconds;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets non-negative fractions of a second at nanosecond
        /// resolution. Negative
        /// second values with fractions must still have non-negative nanos
        /// values
        /// that count forward in time. Must be from 0 to 999,999,999
        /// inclusive. This field may be limited in precision depending on
        /// context.
        /// </summary>
        [JsonProperty(PropertyName = "nanos")]
        public int? Nanos { get; set; }

        /// <summary>
        /// Gets or sets represents seconds of UTC time since Unix epoch
        /// 1970-01-01T00:00:00Z. Must be from 0001-01-01T00:00:00Z to
        /// 9999-12-31T23:59:59Z inclusive.
        /// </summary>
        [JsonProperty(PropertyName = "seconds")]
        public string Seconds { get; set; }

    }
}
