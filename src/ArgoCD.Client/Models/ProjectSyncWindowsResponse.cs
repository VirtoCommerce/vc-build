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

    public partial class ProjectSyncWindowsResponse
    {
        /// <summary>
        /// Initializes a new instance of the ProjectSyncWindowsResponse class.
        /// </summary>
        public ProjectSyncWindowsResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ProjectSyncWindowsResponse class.
        /// </summary>
        public ProjectSyncWindowsResponse(IList<V1alpha1SyncWindow> windows = default(IList<V1alpha1SyncWindow>))
        {
            Windows = windows;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "windows")]
        public IList<V1alpha1SyncWindow> Windows { get; set; }

    }
}
