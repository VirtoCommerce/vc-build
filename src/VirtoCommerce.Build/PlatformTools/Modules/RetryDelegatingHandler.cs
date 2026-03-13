using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        byte[] contentBuffer = null;
        if (request.Content != null)
        {
            using var ms = new MemoryStream();
            request.Content.ReadAsStream(cancellationToken).CopyTo(ms);
            contentBuffer = ms.ToArray();
        }

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            using var requestClone = CloneRequest(request, contentBuffer);
            try
            {
                var response = base.Send(requestClone, cancellationToken);
                if (!IsTransientStatusCode(response.StatusCode) || attempt == _maxRetries)
                {
                    return response;
                }

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

        throw new InvalidOperationException("Unreachable");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        byte[] contentBuffer = null;
        if (request.Content != null)
        {
            contentBuffer = await request.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            var requestClone = CloneRequest(request, contentBuffer);
            try
            {
                var response = await base.SendAsync(requestClone, cancellationToken);
                if (!IsTransientStatusCode(response.StatusCode) || attempt == _maxRetries)
                {
                    return response;
                }

                Log.Warning("Request to {Uri} returned {StatusCode}, retrying ({Attempt}/{Max})...",
                    request.RequestUri, (int)response.StatusCode, attempt + 1, _maxRetries);
                response.Dispose();
            }
            catch (Exception ex) when (attempt < _maxRetries && IsTransientException(ex))
            {
                Log.Warning("Request to {Uri} failed: {Message}, retrying ({Attempt}/{Max})...",
                    request.RequestUri, ex.Message, attempt + 1, _maxRetries);
            }

            await Task.Delay(_retryDelay, cancellationToken);
        }

        throw new InvalidOperationException("Unreachable");
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout
                   or HttpStatusCode.TooManyRequests
                   or HttpStatusCode.GatewayTimeout;
    }

    private static bool IsTransientException(Exception ex) =>
        ex is HttpRequestException or IOException;

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request, byte[] contentBuffer)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (contentBuffer != null)
        {
            clone.Content = new ByteArrayContent(contentBuffer);
            foreach (var header in request.Content!.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
