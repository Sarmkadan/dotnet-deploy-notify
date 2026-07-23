#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Integration;

/// <summary>
/// Interface for HTTP webhook client for sending notifications to external services
/// </summary>
public interface IWebhookClient
{
    /// <summary>
    /// Sends a webhook payload to the specified URL
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to send to</param>
    /// <param name="payload">The payload content to send</param>
    /// <param name="customHeaders">Optional custom headers to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Webhook result indicating success or failure</returns>
    Task<HttpResponse<string>> SendWebhookAsync(
        string webhookUrl,
        string payload,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adapter class that implements IWebhookClient using HttpClient
/// </summary>
public class WebhookClientAdapter : IWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookClientAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookClientAdapter"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests</param>
    /// <param name="logger">Logger instance</param>
    public WebhookClientAdapter(HttpClient httpClient, ILogger<WebhookClientAdapter> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL
    /// </summary>
    /// <param name="webhookUrl">The webhook URL to send to</param>
    /// <param name="payload">The payload content to send</param>
    /// <param name="customHeaders">Optional custom headers to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Webhook result</returns>
    public async Task<HttpResponse<string>> SendWebhookAsync(
        string webhookUrl,
        string payload,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhookUrl);
        ArgumentNullException.ThrowIfNull(payload);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            };

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponse<string>
            {
                IsSuccessful = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Content = responseContent,
                ErrorMessage = response.IsSuccessStatusCode ? null : responseContent,
                ElapsedTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook via adapter");
            return new HttpResponse<string>
            {
                IsSuccessful = false,
                StatusCode = 0,
                ErrorMessage = ex.Message,
                ElapsedTime = stopwatch.Elapsed
            };
        }
    }
}