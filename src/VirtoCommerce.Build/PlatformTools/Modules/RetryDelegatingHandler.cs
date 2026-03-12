using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Serilog;

namespace PlatformTools.Modules;

internal sealed class RetryDelegatingHandler : DelegatingHandler
{
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    public RetryDelegatingHandler(int maxRetries = 3, TimeSpan retryDelay = default)
        : base(new HttpClientHandler())
    {
        _maxRetries = maxRetries;
        _retryDelay = retryDelay == default ? TimeSpan.FromSeconds(3) : retryDelay;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var requestClone = CloneRequest(request);
            try
            {
                var response = base.Send(requestClone, cancellationToken);
                if (!IsTransientStatusCode(response.StatusCode) || attempt >= _maxRetries)
                    return response;

                Log.Warning("Request to {Uri} returned {StatusCode}, retrying ({Attempt}/{Max})...",
                    request.RequestUri, (int)response.StatusCode, attempt + 1, _maxRetries);
                response.Dispose();
            }
            catch (Exception ex) when (attempt < _maxRetries && IsTransientException(ex))
            {
                Log.Warning("Request to {Uri} failed: {Message}, retrying ({Attempt}/{Max})...",
                    request.RequestUri, ex.Message, attempt + 1, _maxRetries);
            }

            Thread.Sleep(_retryDelay);
        }
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
                   or HttpStatusCode.TooManyRequests
                   or HttpStatusCode.BadGateway
                   or HttpStatusCode.ServiceUnavailable
                   or HttpStatusCode.GatewayTimeout;

    private static bool IsTransientException(Exception ex) =>
        ex is HttpRequestException or IOException;

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return clone;
    }
}
