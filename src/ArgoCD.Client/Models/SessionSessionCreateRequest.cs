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
    /// SessionCreateRequest is for logging in.
    /// </summary>
    public partial class SessionSessionCreateRequest
    {
        /// <summary>
        /// Initializes a new instance of the SessionSessionCreateRequest
        /// class.
        /// </summary>
        public SessionSessionCreateRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SessionSessionCreateRequest
        /// class.
        /// </summary>
        public SessionSessionCreateRequest(string password = default(string), string token = default(string), string username = default(string))
        {
            Password = password;
            Token = token;
            Username = username;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

    }
}
