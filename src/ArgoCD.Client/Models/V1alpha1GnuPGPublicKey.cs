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
    /// GnuPGPublicKey is a representation of a GnuPG public key
    /// </summary>
    public partial class V1alpha1GnuPGPublicKey
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1GnuPGPublicKey class.
        /// </summary>
        public V1alpha1GnuPGPublicKey()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1GnuPGPublicKey class.
        /// </summary>
        /// <param name="fingerprint">Fingerprint is the fingerprint of the
        /// key</param>
        /// <param name="keyData">KeyData holds the raw key data, in base64
        /// encoded format</param>
        /// <param name="keyID">KeyID specifies the key ID, in hexadecimal
        /// string format</param>
        /// <param name="owner">Owner holds the owner identification, e.g. a
        /// name and e-mail address</param>
        /// <param name="subType">SubType holds the key's sub type (e.g.
        /// rsa4096)</param>
        /// <param name="trust">Trust holds the level of trust assigned to this
        /// key</param>
        public V1alpha1GnuPGPublicKey(string fingerprint = default(string), string keyData = default(string), string keyID = default(string), string owner = default(string), string subType = default(string), string trust = default(string))
        {
            Fingerprint = fingerprint;
            KeyData = keyData;
            KeyID = keyID;
            Owner = owner;
            SubType = subType;
            Trust = trust;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets fingerprint is the fingerprint of the key
        /// </summary>
        [JsonProperty(PropertyName = "fingerprint")]
        public string Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets keyData holds the raw key data, in base64 encoded
        /// format
        /// </summary>
        [JsonProperty(PropertyName = "keyData")]
        public string KeyData { get; set; }

        /// <summary>
        /// Gets or sets keyID specifies the key ID, in hexadecimal string
        /// format
        /// </summary>
        [JsonProperty(PropertyName = "keyID")]
        public string KeyID { get; set; }

        /// <summary>
        /// Gets or sets owner holds the owner identification, e.g. a name and
        /// e-mail address
        /// </summary>
        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets subType holds the key's sub type (e.g. rsa4096)
        /// </summary>
        [JsonProperty(PropertyName = "subType")]
        public string SubType { get; set; }

        /// <summary>
        /// Gets or sets trust holds the level of trust assigned to this key
        /// </summary>
        [JsonProperty(PropertyName = "trust")]
        public string Trust { get; set; }

    }
}
