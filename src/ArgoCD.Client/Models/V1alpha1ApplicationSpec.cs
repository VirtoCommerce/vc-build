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
    /// ApplicationSpec represents desired application state. Contains link to
    /// repository with application definition and additional parameters link
    /// definition revision.
    /// </summary>
    public partial class V1alpha1ApplicationSpec
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationSpec class.
        /// </summary>
        public V1alpha1ApplicationSpec()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationSpec class.
        /// </summary>
        /// <param name="ignoreDifferences">IgnoreDifferences is a list of
        /// resources and their fields which should be ignored during
        /// comparison</param>
        /// <param name="info">Info contains a list of information (URLs, email
        /// addresses, and plain text) that relates to the application</param>
        /// <param name="project">Project is a reference to the project this
        /// application belongs to.
        /// The empty string means that application belongs to the 'default'
        /// project.</param>
        /// <param name="revisionHistoryLimit">RevisionHistoryLimit limits the
        /// number of items kept in the application's revision history, which
        /// is used for informational purposes as well as for rollbacks to
        /// previous versions.
        /// This should only be changed in exceptional circumstances.
        /// Setting to zero will store no history. This will reduce storage
        /// used.
        /// Increasing will increase the space used to store the history, so we
        /// do not recommend increasing it.
        /// Default is 10.</param>
        public V1alpha1ApplicationSpec(V1alpha1ApplicationDestination destination = default(V1alpha1ApplicationDestination), IList<V1alpha1ResourceIgnoreDifferences> ignoreDifferences = default(IList<V1alpha1ResourceIgnoreDifferences>), IList<V1alpha1Info> info = default(IList<V1alpha1Info>), string project = default(string), string revisionHistoryLimit = default(string), V1alpha1ApplicationSource source = default(V1alpha1ApplicationSource), V1alpha1SyncPolicy syncPolicy = default(V1alpha1SyncPolicy))
        {
            Destination = destination;
            IgnoreDifferences = ignoreDifferences;
            Info = info;
            Project = project;
            RevisionHistoryLimit = revisionHistoryLimit;
            Source = source;
            SyncPolicy = syncPolicy;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "destination")]
        public V1alpha1ApplicationDestination Destination { get; set; }

        /// <summary>
        /// Gets or sets ignoreDifferences is a list of resources and their
        /// fields which should be ignored during comparison
        /// </summary>
        [JsonProperty(PropertyName = "ignoreDifferences")]
        public IList<V1alpha1ResourceIgnoreDifferences> IgnoreDifferences { get; set; }

        /// <summary>
        /// Gets or sets info contains a list of information (URLs, email
        /// addresses, and plain text) that relates to the application
        /// </summary>
        [JsonProperty(PropertyName = "info")]
        public IList<V1alpha1Info> Info { get; set; }

        /// <summary>
        /// Gets or sets project is a reference to the project this application
        /// belongs to.
        /// The empty string means that application belongs to the 'default'
        /// project.
        /// </summary>
        [JsonProperty(PropertyName = "project")]
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets revisionHistoryLimit limits the number of items kept
        /// in the application's revision history, which is used for
        /// informational purposes as well as for rollbacks to previous
        /// versions.
        /// This should only be changed in exceptional circumstances.
        /// Setting to zero will store no history. This will reduce storage
        /// used.
        /// Increasing will increase the space used to store the history, so we
        /// do not recommend increasing it.
        /// Default is 10.
        /// </summary>
        [JsonProperty(PropertyName = "revisionHistoryLimit")]
        public string RevisionHistoryLimit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        public V1alpha1ApplicationSource Source { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "syncPolicy")]
        public V1alpha1SyncPolicy SyncPolicy { get; set; }

    }
}
