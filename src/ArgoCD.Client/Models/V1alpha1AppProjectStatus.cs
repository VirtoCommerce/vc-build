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
    /// AppProjectStatus contains status information for AppProject CRs
    /// </summary>
    public partial class V1alpha1AppProjectStatus
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1AppProjectStatus class.
        /// </summary>
        public V1alpha1AppProjectStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1AppProjectStatus class.
        /// </summary>
        /// <param name="jwtTokensByRole">JWTTokensByRole contains a list of
        /// JWT tokens issued for a given role</param>
        public V1alpha1AppProjectStatus(IDictionary<string, V1alpha1JWTTokens> jwtTokensByRole = default(IDictionary<string, V1alpha1JWTTokens>))
        {
            JwtTokensByRole = jwtTokensByRole;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets jWTTokensByRole contains a list of JWT tokens issued
        /// for a given role
        /// </summary>
        [JsonProperty(PropertyName = "jwtTokensByRole")]
        public IDictionary<string, V1alpha1JWTTokens> JwtTokensByRole { get; set; }

    }
}
