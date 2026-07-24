#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents the result of sending a batch of notifications
/// </summary>
public sealed class BatchNotificationResult
{
    /// <summary>Unique identifier for this batch result</summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>Overall batch status</summary>
    public BatchStatus Status { get; set; }

    /// <summary>When the batch processing started</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the batch processing completed</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Total number of notifications processed</summary>
    public int NotificationCount { get; set; }

    /// <summary>Total number of delivery attempts made</summary>
    public int TotalDeliveryAttempts { get; set; }

    /// <summary>Number of successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Number of failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>Number of skipped deliveries (due to filters)</summary>
    public int SkippedDeliveries { get; set; }

    /// <summary>Average delivery time in milliseconds</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>Maximum delivery time in milliseconds</summary>
    public long MaxDeliveryTimeMs { get; set; }

    /// <summary>Minimum delivery time in milliseconds</summary>
    public long MinDeliveryTimeMs { get; set; } = long.MaxValue;

    /// <summary>Per-notification results</summary>
    public List<NotificationResult> PerNotificationResults { get; } = new();

    /// <summary>Per-channel results</summary>
    public List<ChannelDeliveryResult> PerChannelResults { get; } = new();

    /// <summary>Whether all deliveries were successful</summary>
    public bool IsSuccessful => FailedDeliveries == 0;

    /// <summary>Success rate percentage</summary>
    public double SuccessRate => TotalDeliveryAttempts > 0
        ? (SuccessfulDeliveries * 100.0) / TotalDeliveryAttempts
        : 0;

    /// <summary>
    /// Adds a notification result to the batch result
    /// </summary>
    public void AddNotificationResult(NotificationResult result)
    {
        PerNotificationResults.Add(result);

        // Update counters
        TotalDeliveryAttempts++;
        if (result.IsSuccessful)
        {
            SuccessfulDeliveries++;
        }
        else if (result.Status == DeliveryStatus.Skipped)
        {
            SkippedDeliveries++;
        }
        else
        {
            FailedDeliveries++;
        }

        // Update timing statistics
        var durationMs = result.DurationMs;
        if (durationMs > 0)
        {
            AverageDeliveryTimeMs = (AverageDeliveryTimeMs * (TotalDeliveryAttempts - 1) + durationMs) / TotalDeliveryAttempts;
            MaxDeliveryTimeMs = Math.Max(MaxDeliveryTimeMs, durationMs);
            MinDeliveryTimeMs = Math.Min(MinDeliveryTimeMs, durationMs);
        }

        // Add to channel results
        var channelResult = PerChannelResults.FirstOrDefault(cr => cr.Channel == result.Channel);
        if (channelResult is null)
        {
            channelResult = new ChannelDeliveryResult
            {
                Channel = result.Channel,
                ConfigurationId = result.ConfigurationId
            };
            PerChannelResults.Add(channelResult);
        }

        channelResult.AddResult(result);
    }

    /// <summary>
    /// Marks the batch result as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        CompletedAt = DateTime.UtcNow;
        Status = IsSuccessful ? BatchStatus.Sent : BatchStatus.PartiallyCompleted;
    }

    /// <summary>
    /// Gets a summary of the batch result
    /// </summary>
    public string GetSummary()
    {
        var duration = CompletedAt.HasValue ? (CompletedAt.Value - StartedAt).TotalSeconds : 0;
        return $"Batch Result: {BatchId} | Status: {Status} | " +
               $"Notifications: {NotificationCount} | Attempts: {TotalDeliveryAttempts} | " +
               $"Success: {SuccessRate:F1}% | Duration: {duration:F1}s";
    }
}

/// <summary>
/// Represents aggregated results for a specific channel
/// </summary>
public sealed class ChannelDeliveryResult
{
    /// <summary>Channel type</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Configuration ID used</summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>Total attempts for this channel</summary>
    public int TotalAttempts { get; set; }

    /// <summary>Successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>Skipped deliveries</summary>
    public int SkippedDeliveries { get; set; }

    /// <summary>Average delivery time</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>All individual results for this channel</summary>
    public List<NotificationResult> Results { get; } = new();

    /// <summary>Adds a result to this channel's statistics</summary>
    public void AddResult(NotificationResult result)
    {
        Results.Add(result);
        TotalAttempts++;

        if (result.IsSuccessful)
        {
            SuccessfulDeliveries++;
        }
        else if (result.Status == DeliveryStatus.Skipped)
        {
            SkippedDeliveries++;
        }
        else
        {
            FailedDeliveries++;
        }

        if (result.DurationMs > 0)
        {
            AverageDeliveryTimeMs = (AverageDeliveryTimeMs * (TotalAttempts - 1) + result.DurationMs) / TotalAttempts;
        }
    }

    /// <summary>Success rate for this channel</summary>
    public double SuccessRate => TotalAttempts > 0 ? (SuccessfulDeliveries * 100.0) / TotalAttempts : 0;
}