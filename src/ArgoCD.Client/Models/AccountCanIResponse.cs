// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountCanIResponse
    {
        /// <summary>
        /// Initializes a new instance of the AccountCanIResponse class.
        /// </summary>
        public AccountCanIResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountCanIResponse class.
        /// </summary>
        public AccountCanIResponse(string value = default(string))
        {
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

    }
}
