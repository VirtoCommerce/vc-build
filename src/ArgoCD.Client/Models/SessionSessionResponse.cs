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
    /// SessionResponse wraps the created token or returns an empty string if
    /// deleted.
    /// </summary>
    public partial class SessionSessionResponse
    {
        /// <summary>
        /// Initializes a new instance of the SessionSessionResponse class.
        /// </summary>
        public SessionSessionResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SessionSessionResponse class.
        /// </summary>
        public SessionSessionResponse(string token = default(string))
        {
            Token = token;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

    }
}
