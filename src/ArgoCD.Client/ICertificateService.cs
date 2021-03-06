// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client
{
    using Microsoft.Rest;
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// CertificateService operations.
    /// </summary>
    public partial interface ICertificateService
    {
        /// <summary>
        /// List all available repository certificates
        /// </summary>
        /// <param name='hostNamePattern'>
        /// A file-glob pattern (not regular expression) the host name has to
        /// match.
        /// </param>
        /// <param name='certType'>
        /// The type of the certificate to match (ssh or https).
        /// </param>
        /// <param name='certSubType'>
        /// The sub type of the certificate to match (protocol dependent,
        /// usually only used for ssh certs).
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="RuntimeErrorException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        Task<HttpOperationResponse<V1alpha1RepositoryCertificateList>> ListCertificatesWithHttpMessagesAsync(string hostNamePattern = default(string), string certType = default(string), string certSubType = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Creates repository certificates on the server
        /// </summary>
        /// <param name='body'>
        /// List of certificates to be created
        /// </param>
        /// <param name='upsert'>
        /// Whether to upsert already existing certificates.
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="RuntimeErrorException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        Task<HttpOperationResponse<V1alpha1RepositoryCertificateList>> CreateCertificateWithHttpMessagesAsync(V1alpha1RepositoryCertificateList body, bool? upsert = default(bool?), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Delete the certificates that match the RepositoryCertificateQuery
        /// </summary>
        /// <param name='hostNamePattern'>
        /// A file-glob pattern (not regular expression) the host name has to
        /// match.
        /// </param>
        /// <param name='certType'>
        /// The type of the certificate to match (ssh or https).
        /// </param>
        /// <param name='certSubType'>
        /// The sub type of the certificate to match (protocol dependent,
        /// usually only used for ssh certs).
        /// </param>
        /// <param name='customHeaders'>
        /// The headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="RuntimeErrorException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        Task<HttpOperationResponse<V1alpha1RepositoryCertificateList>> DeleteCertificateWithHttpMessagesAsync(string hostNamePattern = default(string), string certType = default(string), string certSubType = default(string), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
