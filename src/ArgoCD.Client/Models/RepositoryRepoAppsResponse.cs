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
    /// RepoAppsResponse contains applications of specified repository
    /// </summary>
    public partial class RepositoryRepoAppsResponse
    {
        /// <summary>
        /// Initializes a new instance of the RepositoryRepoAppsResponse class.
        /// </summary>
        public RepositoryRepoAppsResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the RepositoryRepoAppsResponse class.
        /// </summary>
        public RepositoryRepoAppsResponse(IList<RepositoryAppInfo> items = default(IList<RepositoryAppInfo>))
        {
            Items = items;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        public IList<RepositoryAppInfo> Items { get; set; }

    }
}
