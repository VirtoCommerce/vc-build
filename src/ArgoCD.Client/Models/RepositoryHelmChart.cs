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

    public partial class RepositoryHelmChart
    {
        /// <summary>
        /// Initializes a new instance of the RepositoryHelmChart class.
        /// </summary>
        public RepositoryHelmChart()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the RepositoryHelmChart class.
        /// </summary>
        public RepositoryHelmChart(string name = default(string), IList<string> versions = default(IList<string>))
        {
            Name = name;
            Versions = versions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "versions")]
        public IList<string> Versions { get; set; }

    }
}
