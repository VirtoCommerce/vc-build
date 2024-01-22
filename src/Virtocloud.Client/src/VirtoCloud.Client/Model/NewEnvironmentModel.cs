/*
 * VirtoCommerce.SaaS
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using FileParameter = VirtoCloud.Client.Client.FileParameter;
using OpenAPIDateConverter = VirtoCloud.Client.Client.OpenAPIDateConverter;

namespace VirtoCloud.Client.Model
{
    /// <summary>
    /// NewEnvironmentModel
    /// </summary>
    [DataContract(Name = "NewEnvironmentModel")]
    public partial class NewEnvironmentModel : IEquatable<NewEnvironmentModel>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewEnvironmentModel" /> class.
        /// </summary>
        /// <param name="name">name.</param>
        /// <param name="dbName">dbName.</param>
        /// <param name="appProjectId">appProjectId.</param>
        /// <param name="cluster">cluster.</param>
        /// <param name="servicePlan">servicePlan.</param>
        /// <param name="dbProvider">dbProvider.</param>
        /// <param name="helm">helm.</param>
        public NewEnvironmentModel(string name = default(string), string dbName = default(string), string appProjectId = default(string), string cluster = default(string), string servicePlan = default(string), string dbProvider = default(string), HelmObject helm = default(HelmObject))
        {
            this.Name = name;
            this.DbName = dbName;
            this.AppProjectId = appProjectId;
            this.Cluster = cluster;
            this.ServicePlan = servicePlan;
            this.DbProvider = dbProvider;
            this.Helm = helm;
        }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets DbName
        /// </summary>
        [DataMember(Name = "dbName", EmitDefaultValue = true)]
        public string DbName { get; set; }

        /// <summary>
        /// Gets or Sets AppProjectId
        /// </summary>
        [DataMember(Name = "appProjectId", EmitDefaultValue = true)]
        public string AppProjectId { get; set; }

        /// <summary>
        /// Gets or Sets Cluster
        /// </summary>
        [DataMember(Name = "cluster", EmitDefaultValue = true)]
        public string Cluster { get; set; }

        /// <summary>
        /// Gets or Sets ServicePlan
        /// </summary>
        [DataMember(Name = "servicePlan", EmitDefaultValue = true)]
        public string ServicePlan { get; set; }

        /// <summary>
        /// Gets or Sets DbProvider
        /// </summary>
        [DataMember(Name = "dbProvider", EmitDefaultValue = true)]
        public string DbProvider { get; set; }

        /// <summary>
        /// Gets or Sets Helm
        /// </summary>
        [DataMember(Name = "helm", EmitDefaultValue = true)]
        public HelmObject Helm { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class NewEnvironmentModel {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  DbName: ").Append(DbName).Append("\n");
            sb.Append("  AppProjectId: ").Append(AppProjectId).Append("\n");
            sb.Append("  Cluster: ").Append(Cluster).Append("\n");
            sb.Append("  ServicePlan: ").Append(ServicePlan).Append("\n");
            sb.Append("  DbProvider: ").Append(DbProvider).Append("\n");
            sb.Append("  Helm: ").Append(Helm).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as NewEnvironmentModel);
        }

        /// <summary>
        /// Returns true if NewEnvironmentModel instances are equal
        /// </summary>
        /// <param name="input">Instance of NewEnvironmentModel to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NewEnvironmentModel input)
        {
            if (input == null)
            {
                return false;
            }
            return 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.DbName == input.DbName ||
                    (this.DbName != null &&
                    this.DbName.Equals(input.DbName))
                ) && 
                (
                    this.AppProjectId == input.AppProjectId ||
                    (this.AppProjectId != null &&
                    this.AppProjectId.Equals(input.AppProjectId))
                ) && 
                (
                    this.Cluster == input.Cluster ||
                    (this.Cluster != null &&
                    this.Cluster.Equals(input.Cluster))
                ) && 
                (
                    this.ServicePlan == input.ServicePlan ||
                    (this.ServicePlan != null &&
                    this.ServicePlan.Equals(input.ServicePlan))
                ) && 
                (
                    this.DbProvider == input.DbProvider ||
                    (this.DbProvider != null &&
                    this.DbProvider.Equals(input.DbProvider))
                ) && 
                (
                    this.Helm == input.Helm ||
                    (this.Helm != null &&
                    this.Helm.Equals(input.Helm))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Name != null)
                {
                    hashCode = (hashCode * 59) + this.Name.GetHashCode();
                }
                if (this.DbName != null)
                {
                    hashCode = (hashCode * 59) + this.DbName.GetHashCode();
                }
                if (this.AppProjectId != null)
                {
                    hashCode = (hashCode * 59) + this.AppProjectId.GetHashCode();
                }
                if (this.Cluster != null)
                {
                    hashCode = (hashCode * 59) + this.Cluster.GetHashCode();
                }
                if (this.ServicePlan != null)
                {
                    hashCode = (hashCode * 59) + this.ServicePlan.GetHashCode();
                }
                if (this.DbProvider != null)
                {
                    hashCode = (hashCode * 59) + this.DbProvider.GetHashCode();
                }
                if (this.Helm != null)
                {
                    hashCode = (hashCode * 59) + this.Helm.GetHashCode();
                }
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}