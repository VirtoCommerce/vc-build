// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client
{
    using Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for RepoCredsService.
    /// </summary>
    public static partial class RepoCredsServiceExtensions
    {
            /// <summary>
            /// ListRepositoryCredentials gets a list of all configured repository
            /// credential sets
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='url'>
            /// Repo URL for query.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<V1alpha1RepoCredsList> ListRepositoryCredentialsAsync(this IRepoCredsService operations, string url = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.ListRepositoryCredentialsWithHttpMessagesAsync(url, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// CreateRepositoryCredentials creates a new repository credential set
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='body'>
            /// Repository definition
            /// </param>
            /// <param name='upsert'>
            /// Whether to create in upsert mode.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<V1alpha1RepoCreds> CreateRepositoryCredentialsAsync(this IRepoCredsService operations, V1alpha1RepoCreds body, bool? upsert = default(bool?), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateRepositoryCredentialsWithHttpMessagesAsync(body, upsert, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// UpdateRepositoryCredentials updates a repository credential set
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='credsurl'>
            /// URL is the URL that this credentials matches to
            /// </param>
            /// <param name='body'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<V1alpha1RepoCreds> UpdateRepositoryCredentialsAsync(this IRepoCredsService operations, string credsurl, V1alpha1RepoCreds body, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.UpdateRepositoryCredentialsWithHttpMessagesAsync(credsurl, body, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// DeleteRepositoryCredentials deletes a repository credential set from the
            /// configuration
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='url'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> DeleteRepositoryCredentialsAsync(this IRepoCredsService operations, string url, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.DeleteRepositoryCredentialsWithHttpMessagesAsync(url, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
