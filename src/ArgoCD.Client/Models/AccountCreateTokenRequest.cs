// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountCreateTokenRequest
    {
        /// <summary>
        /// Initializes a new instance of the AccountCreateTokenRequest class.
        /// </summary>
        public AccountCreateTokenRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountCreateTokenRequest class.
        /// </summary>
        /// <param name="expiresIn">expiresIn represents a duration in
        /// seconds</param>
        public AccountCreateTokenRequest(string expiresIn = default(string), string id = default(string), string name = default(string))
        {
            ExpiresIn = expiresIn;
            Id = id;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets expiresIn represents a duration in seconds
        /// </summary>
        [JsonProperty(PropertyName = "expiresIn")]
        public string ExpiresIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

    }
}
