#nullable enable
using System.Net;

namespace DotNetDeployNotify.Tests.Fakes;

/// <summary>
/// A fake HTTP transport that records every outgoing webhook request instead of
/// touching the network. It lets tests assert on the exact payload, headers, and
/// target URL a channel would have sent, and lets them script the response.
/// </summary>
public sealed class FakeWebhookTransport : HttpMessageHandler
{
    private readonly Func<CapturedRequest, HttpResponseMessage> _responder;

    /// <summary>All requests captured, in the order they were sent</summary>
    public List<CapturedRequest> Requests { get; } = new();

    /// <summary>The most recently captured request, or null if none were sent</summary>
    public CapturedRequest? LastRequest => Requests.Count > 0 ? Requests[^1] : null;

    /// <summary>
    /// Creates a transport that returns the given status code for every request.
    /// </summary>
    public FakeWebhookTransport(HttpStatusCode statusCode = HttpStatusCode.OK, string responseBody = "ok")
        : this(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody)
        })
    {
    }

    /// <summary>
    /// Creates a transport whose response is produced by the supplied responder,
    /// allowing per-request behaviour (e.g. failing only certain URLs).
    /// </summary>
    public FakeWebhookTransport(Func<CapturedRequest, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        var headers = new Dictionary<string, string>();
        if (request.Content is not null)
        {
            foreach (var h in request.Content.Headers)
                headers[h.Key] = string.Join(",", h.Value);
        }
        foreach (var h in request.Headers)
            headers[h.Key] = string.Join(",", h.Value);

        var captured = new CapturedRequest
        {
            Method = request.Method.Method,
            Url = request.RequestUri?.ToString() ?? string.Empty,
            Body = body,
            Headers = headers
        };

        Requests.Add(captured);
        return _responder(captured);
    }
}

/// <summary>
/// An immutable snapshot of an outgoing HTTP request captured by the fake transport.
/// </summary>
public sealed class CapturedRequest
{
    /// <summary>HTTP method (POST, GET, ...)</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Destination URL the request was addressed to</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Raw request body</summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>Combined content and request headers</summary>
    public Dictionary<string, string> Headers { get; init; } = new();
}
