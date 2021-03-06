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

    public partial class RuntimeError
    {
        /// <summary>
        /// Initializes a new instance of the RuntimeError class.
        /// </summary>
        public RuntimeError()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the RuntimeError class.
        /// </summary>
        public RuntimeError(int? code = default(int?), IList<ProtobufAny> details = default(IList<ProtobufAny>), string error = default(string), string message = default(string))
        {
            Code = code;
            Details = details;
            Error = error;
            Message = message;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public int? Code { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "details")]
        public IList<ProtobufAny> Details { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

    }
}
