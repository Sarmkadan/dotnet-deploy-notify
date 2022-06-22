// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Interface for logging HTTP requests and responses
/// </summary>
public interface IRequestLogger
{
    /// <summary>Logs an outgoing webhook request</summary>
    void LogWebhookRequest(string webhookUrl, string payload, Dictionary<string, string> headers);

    /// <summary>Logs an incoming webhook response</summary>
    void LogWebhookResponse(string webhookUrl, int statusCode, string responseBody, long durationMs);

    /// <summary>Logs an HTTP error</summary>
    void LogWebhookError(string webhookUrl, string errorMessage, Exception? exception = null);

    /// <summary>Gets request/response history</summary>
    List<RequestLogEntry> GetRequestHistory(int limit = 100);

    /// <summary>Clears old request logs</summary>
    void ClearOldLogs(DateTime olderThan);
}

/// <summary>
/// Represents a single request/response log entry
/// </summary>
public class RequestLogEntry
{
    /// <summary>Unique entry ID</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Webhook URL that was called</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>HTTP method (POST, PUT, etc.)</summary>
    public string Method { get; set; } = "POST";

    /// <summary>Request headers</summary>
    public Dictionary<string, string> RequestHeaders { get; set; } = new();

    /// <summary>Payload that was sent</summary>
    public string RequestPayload { get; set; } = string.Empty;

    /// <summary>HTTP response status code</summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>Response body</summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>Request duration in milliseconds</summary>
    public long DurationMs { get; set; }

    /// <summary>Timestamp of the request</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Error message if request failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Is the request successful</summary>
    public bool IsSuccessful => ResponseStatusCode.HasValue && ResponseStatusCode >= 200 && ResponseStatusCode < 300;

    /// <summary>Gets a summary of the request</summary>
    public string GetSummary()
    {
        if (!IsSuccessful && !string.IsNullOrEmpty(ErrorMessage))
            return $"ERROR: {ErrorMessage}";

        return $"HTTP {ResponseStatusCode} in {DurationMs}ms";
    }
}

/// <summary>
/// Implementation of request logger using in-memory storage
/// </summary>
public class RequestLogger : IRequestLogger
{
    private readonly List<RequestLogEntry> _logs = new();
    private readonly ILogger<RequestLogger> _logger;
    private readonly object _lockObject = new();
    private const int MaxLogEntries = 10000;

    /// <summary>Initializes the request logger</summary>
    public RequestLogger(ILogger<RequestLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs an outgoing webhook request
    /// </summary>
    public void LogWebhookRequest(string webhookUrl, string payload, Dictionary<string, string> headers)
    {
        lock (_lockObject)
        {
            var entry = new RequestLogEntry
            {
                WebhookUrl = webhookUrl,
                Method = "POST",
                RequestHeaders = new Dictionary<string, string>(headers ?? new Dictionary<string, string>()),
                RequestPayload = payload
            };

            _logs.Add(entry);

            // Trim old logs if necessary
            if (_logs.Count > MaxLogEntries)
            {
                _logs.RemoveRange(0, _logs.Count - MaxLogEntries);
            }

            _logger.LogDebug("Logged outgoing webhook request to {Url}", MaskUrl(webhookUrl));
        }
    }

    /// <summary>
    /// Logs an incoming webhook response
    /// </summary>
    public void LogWebhookResponse(string webhookUrl, int statusCode, string responseBody, long durationMs)
    {
        lock (_lockObject)
        {
            var entry = _logs.FirstOrDefault(l => l.WebhookUrl == webhookUrl && l.ResponseStatusCode == null);
            if (entry != null)
            {
                entry.ResponseStatusCode = statusCode;
                entry.ResponseBody = responseBody;
                entry.DurationMs = durationMs;

                _logger.LogDebug(
                    "Logged webhook response from {Url}: {Status} ({DurationMs}ms)",
                    MaskUrl(webhookUrl),
                    statusCode,
                    durationMs);
            }
        }
    }

    /// <summary>
    /// Logs a webhook error
    /// </summary>
    public void LogWebhookError(string webhookUrl, string errorMessage, Exception? exception = null)
    {
        lock (_lockObject)
        {
            var entry = new RequestLogEntry
            {
                WebhookUrl = webhookUrl,
                Method = "POST",
                ErrorMessage = errorMessage
            };

            _logs.Add(entry);

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "Webhook error for {Url}: {Message}",
                    MaskUrl(webhookUrl),
                    errorMessage);
            }
            else
            {
                _logger.LogWarning(
                    "Webhook error for {Url}: {Message}",
                    MaskUrl(webhookUrl),
                    errorMessage);
            }
        }
    }

    /// <summary>
    /// Retrieves request history
    /// </summary>
    public List<RequestLogEntry> GetRequestHistory(int limit = 100)
    {
        lock (_lockObject)
        {
            return _logs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Clears old logs
    /// </summary>
    public void ClearOldLogs(DateTime olderThan)
    {
        lock (_lockObject)
        {
            var oldLogs = _logs.Where(l => l.Timestamp < olderThan).ToList();
            foreach (var log in oldLogs)
            {
                _logs.Remove(log);
            }
            _logger.LogDebug("Cleared {Count} request logs older than {Date}", oldLogs.Count, olderThan);
        }
    }

    private static string MaskUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length < 20)
            return "***MASKED***";
        return url[..10] + "***" + url[^5..];
    }
}
