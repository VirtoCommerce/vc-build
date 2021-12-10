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
    /// ProjectTokenCreateRequest defines project token creation parameters.
    /// </summary>
    public partial class ProjectProjectTokenCreateRequest
    {
        /// <summary>
        /// Initializes a new instance of the ProjectProjectTokenCreateRequest
        /// class.
        /// </summary>
        public ProjectProjectTokenCreateRequest()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ProjectProjectTokenCreateRequest
        /// class.
        /// </summary>
        /// <param name="expiresIn">expiresIn represents a duration in
        /// seconds</param>
        public ProjectProjectTokenCreateRequest(string description = default(string), string expiresIn = default(string), string id = default(string), string project = default(string), string role = default(string))
        {
            Description = description;
            ExpiresIn = expiresIn;
            Id = id;
            Project = project;
            Role = role;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

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
        [JsonProperty(PropertyName = "project")]
        public string Project { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

    }
}
