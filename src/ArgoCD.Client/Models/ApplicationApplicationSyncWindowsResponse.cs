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

    public partial class ApplicationApplicationSyncWindowsResponse
    {
        /// <summary>
        /// Initializes a new instance of the
        /// ApplicationApplicationSyncWindowsResponse class.
        /// </summary>
        public ApplicationApplicationSyncWindowsResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// ApplicationApplicationSyncWindowsResponse class.
        /// </summary>
        public ApplicationApplicationSyncWindowsResponse(IList<ApplicationApplicationSyncWindow> activeWindows = default(IList<ApplicationApplicationSyncWindow>), IList<ApplicationApplicationSyncWindow> assignedWindows = default(IList<ApplicationApplicationSyncWindow>), bool? canSync = default(bool?))
        {
            ActiveWindows = activeWindows;
            AssignedWindows = assignedWindows;
            CanSync = canSync;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "activeWindows")]
        public IList<ApplicationApplicationSyncWindow> ActiveWindows { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "assignedWindows")]
        public IList<ApplicationApplicationSyncWindow> AssignedWindows { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "canSync")]
        public bool? CanSync { get; set; }

    }
}