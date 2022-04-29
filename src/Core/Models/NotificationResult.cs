// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents the result of a notification delivery attempt
/// </summary>
public class NotificationResult
{
    /// <summary>Unique identifier for this result record</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Reference to the notification that was sent</summary>
    public string NotificationId { get; set; } = string.Empty;

    /// <summary>Channel this notification was sent to</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Configuration ID used for sending</summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>Current delivery status</summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>HTTP status code from the webhook response</summary>
    public int? HttpStatusCode { get; set; }

    /// <summary>Response body or error message</summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>Error details if delivery failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Exception type name if an exception occurred</summary>
    public string? ExceptionType { get; set; }

    /// <summary>Attempt number (1st attempt, 2nd retry, etc.)</summary>
    public int AttemptNumber { get; set; } = 1;

    /// <summary>Time taken to deliver in milliseconds</summary>
    public long DurationMs { get; set; }

    /// <summary>Timestamp of the delivery attempt</summary>
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp of the last retry (if applicable)</summary>
    public DateTime? LastRetryAt { get; set; }

    /// <summary>Next scheduled retry time</summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>Whether this delivery was successful</summary>
    public bool IsSuccessful => Status == DeliveryStatus.Delivered;

    /// <summary>
    /// Validates the notification result has required fields
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(NotificationId) &&
               !string.IsNullOrWhiteSpace(ConfigurationId) &&
               DurationMs >= 0;
    }

    /// <summary>
    /// Marks the delivery as successful with HTTP response details
    /// </summary>
    public void MarkAsSuccessful(int statusCode, string responseBody)
    {
        Status = DeliveryStatus.Delivered;
        HttpStatusCode = statusCode;
        ResponseBody = responseBody;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the delivery as failed with error details
    /// </summary>
    public void MarkAsFailed(string errorMessage, string? exceptionType = null, int? statusCode = null)
    {
        Status = DeliveryStatus.Failed;
        ErrorMessage = errorMessage;
        ExceptionType = exceptionType;
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// Marks the delivery as scheduled for retry
    /// </summary>
    public void MarkForRetry(DateTime nextRetryTime)
    {
        Status = DeliveryStatus.Retried;
        LastRetryAt = DateTime.UtcNow;
        NextRetryAt = nextRetryTime;
    }

    /// <summary>
    /// Marks the delivery as skipped
    /// </summary>
    public void MarkAsSkipped(string reason)
    {
        Status = DeliveryStatus.Skipped;
        ResponseBody = reason;
    }

    /// <summary>
    /// Marks the delivery as timed out
    /// </summary>
    public void MarkAsTimeout()
    {
        Status = DeliveryStatus.Timeout;
        ErrorMessage = "Webhook request timed out";
    }

    /// <summary>
    /// Gets a summary description of the result
    /// </summary>
    public string GetSummary()
    {
        return Status switch
        {
            DeliveryStatus.Delivered => $"Successfully delivered in {DurationMs}ms",
            DeliveryStatus.Failed => $"Failed: {ErrorMessage}",
            DeliveryStatus.Timeout => "Delivery timed out",
            DeliveryStatus.Skipped => $"Skipped: {ResponseBody}",
            DeliveryStatus.Retried => $"Retry #{AttemptNumber} scheduled for {NextRetryAt:u}",
            _ => $"Status: {Status}"
        };
    }

    /// <summary>
    /// Creates a result object with common failure scenario
    /// </summary>
    public static NotificationResult CreateFailure(string notificationId, NotificationChannel channel,
        string configId, string errorMessage, string exceptionType)
    {
        return new NotificationResult
        {
            NotificationId = notificationId,
            Channel = channel,
            ConfigurationId = configId,
            Status = DeliveryStatus.Failed,
            ErrorMessage = errorMessage,
            ExceptionType = exceptionType
        };
    }

    /// <summary>
    /// Creates a result object with common success scenario
    /// </summary>
    public static NotificationResult CreateSuccess(string notificationId, NotificationChannel channel,
        string configId, int statusCode, long durationMs)
    {
        return new NotificationResult
        {
            NotificationId = notificationId,
            Channel = channel,
            ConfigurationId = configId,
            Status = DeliveryStatus.Delivered,
            HttpStatusCode = statusCode,
            DurationMs = durationMs
        };
    }
}
