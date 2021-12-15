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
    /// LoadBalancerIngress represents the status of a load-balancer ingress
    /// point:
    /// traffic intended for the service should be sent to an ingress point.
    /// </summary>
    public partial class V1LoadBalancerIngress
    {
        /// <summary>
        /// Initializes a new instance of the V1LoadBalancerIngress class.
        /// </summary>
        public V1LoadBalancerIngress()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1LoadBalancerIngress class.
        /// </summary>
        /// <param name="hostname">Hostname is set for load-balancer ingress
        /// points that are DNS based
        /// (typically AWS load-balancers)
        /// +optional</param>
        /// <param name="ip">IP is set for load-balancer ingress points that
        /// are IP based
        /// (typically GCE or OpenStack load-balancers)
        /// +optional</param>
        /// <param name="ports">Ports is a list of records of service ports
        /// If used, every port defined in the service should have an entry in
        /// it
        /// +listType=atomic
        /// +optional</param>
        public V1LoadBalancerIngress(string hostname = default(string), string ip = default(string), IList<V1PortStatus> ports = default(IList<V1PortStatus>))
        {
            Hostname = hostname;
            Ip = ip;
            Ports = ports;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets hostname is set for load-balancer ingress points that
        /// are DNS based
        /// (typically AWS load-balancers)
        /// +optional
        /// </summary>
        [JsonProperty(PropertyName = "hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets IP is set for load-balancer ingress points that are IP
        /// based
        /// (typically GCE or OpenStack load-balancers)
        /// +optional
        /// </summary>
        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets ports is a list of records of service ports
        /// If used, every port defined in the service should have an entry in
        /// it
        /// +listType=atomic
        /// +optional
        /// </summary>
        [JsonProperty(PropertyName = "ports")]
        public IList<V1PortStatus> Ports { get; set; }

    }
}