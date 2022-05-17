#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for background notification processing
/// </summary>
public interface INotificationProcessor
{
    /// <summary>Processes a batch of pending notifications</summary>
    Task<ProcessingResult> ProcessBatchAsync(int batchSize = 50);

    /// <summary>Retries failed notifications</summary>
    Task<ProcessingResult> ProcessFailedAsync(int maxRetries = 3);

    /// <summary>Processes notifications by priority</summary>
    Task<ProcessingResult> ProcessByPriorityAsync();

    /// <summary>Gets current processing statistics</summary>
    Task<ProcessingStatistics> GetStatisticsAsync();
}

/// <summary>
/// Result of a batch processing operation
/// </summary>
public class ProcessingResult
{
    /// <summary>Total notifications processed</summary>
    public int TotalProcessed { get; set; }

    /// <summary>Number of successful deliveries</summary>
    public int SuccessCount { get; set; }

    /// <summary>Number of failed deliveries</summary>
    public int FailureCount { get; set; }

    /// <summary>Number of skipped notifications</summary>
    public int SkippedCount { get; set; }

    /// <summary>Total time taken in milliseconds</summary>
    public long DurationMs { get; set; }

    /// <summary>List of processing errors if any</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Gets the success rate percentage</summary>
    public double SuccessRate => TotalProcessed > 0 ? (SuccessCount * 100.0) / TotalProcessed : 0;

    /// <summary>Gets a summary of the processing result</summary>
    public string GetSummary()
    {
        return $"Processed: {TotalProcessed} | Success: {SuccessCount} ({SuccessRate:F1}%) | " +
               $"Failed: {FailureCount} | Skipped: {SkippedCount} | Duration: {DurationMs}ms";
    }
}

/// <summary>
/// Processing statistics for the notification system
/// </summary>
public class ProcessingStatistics
{
    /// <summary>Total notifications in the system</summary>
    public int TotalNotifications { get; set; }

    /// <summary>Number of pending notifications</summary>
    public int PendingCount { get; set; }

    /// <summary>Number of processed notifications</summary>
    public int ProcessedCount { get; set; }

    /// <summary>Total delivery attempts</summary>
    public int TotalDeliveryAttempts { get; set; }

    /// <summary>Successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>Average delivery time in milliseconds</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>Number of active configurations</summary>
    public int ActiveConfigurations { get; set; }

    /// <summary>Timestamp of last processing</summary>
    public DateTime? LastProcessedAt { get; set; }

    /// <summary>Gets overall system health percentage</summary>
    public double HealthPercentage => TotalDeliveryAttempts > 0
        ? (SuccessfulDeliveries * 100.0) / TotalDeliveryAttempts
        : 100;
}

/// <summary>
/// Implementation of notification processor
/// </summary>
public class NotificationProcessor : INotificationProcessor
{
    private readonly INotificationService _notificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IChannelConfigRepository _configRepository;
    private readonly INotificationResultRepository _resultRepository;
    private readonly ILogger<NotificationProcessor> _logger;
    private DateTime _lastProcessedAt = DateTime.UtcNow;

    /// <summary>Initializes the notification processor</summary>
    public NotificationProcessor(
        INotificationService notificationService,
        INotificationRepository notificationRepository,
        IChannelConfigRepository configRepository,
        INotificationResultRepository resultRepository,
        ILogger<NotificationProcessor> logger)
    {
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
        _configRepository = configRepository;
        _resultRepository = resultRepository;
        _logger = logger;
    }

    /// <summary>
    /// Processes a batch of pending notifications
    /// </summary>
    public async Task<ProcessingResult> ProcessBatchAsync(int batchSize = 50)
    {
        _logger.LogInformation("Starting batch processing of {BatchSize} notifications", batchSize);

        var result = new ProcessingResult();
        var startTime = DateTime.UtcNow;

        try
        {
            var results = await _notificationService.SendPendingNotificationsAsync().ConfigureAwait(false);

            result.TotalProcessed = results.Count;
            result.SuccessCount = results.Count(r => r.IsSuccessful);
            result.FailureCount = results.Count(r => !r.IsSuccessful && r.Status != DeliveryStatus.Skipped);
            result.SkippedCount = results.Count(r => r.Status == DeliveryStatus.Skipped);

            _lastProcessedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Batch processing completed: {Summary}",
                result.GetSummary());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch processing");
            result.Errors.Add(ex.Message);
        }
        finally
        {
            result.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds > 0
                ? (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                : 1;
        }

        return result;
    }

    /// <summary>
    /// Processes and retries failed notifications
    /// </summary>
    public async Task<ProcessingResult> ProcessFailedAsync(int maxRetries = 3)
    {
        _logger.LogInformation("Starting retry processing for failed notifications");

        var result = new ProcessingResult();
        var startTime = DateTime.UtcNow;

        try
        {
            var allResults = await _resultRepository.GetAllAsync(0, 1000).ConfigureAwait(false);
            var failedResults = allResults.Where(r => r.Status == DeliveryStatus.Failed).ToList();

            foreach (var failedResult in failedResults)
            {
                if (failedResult.AttemptNumber >= maxRetries)
                {
                    result.SkippedCount++;
                    continue;
                }

                try
                {
                    var retryResults = await _notificationService.RetryFailedDeliveriesAsync(
                        failedResult.NotificationId);

                    result.TotalProcessed += retryResults.Count;
                    result.SuccessCount += retryResults.Count(r => r.IsSuccessful);
                    result.FailureCount += retryResults.Count(r => !r.IsSuccessful);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to retry notification {NotificationId}",
                        failedResult.NotificationId);
                    result.FailureCount++;
                }
            }

            _logger.LogInformation(
                "Retry processing completed: {Summary}",
                result.GetSummary());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during retry processing");
            result.Errors.Add(ex.Message);
        }
        finally
        {
            result.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// Processes notifications prioritizing by importance level
    /// </summary>
    public async Task<ProcessingResult> ProcessByPriorityAsync()
    {
        _logger.LogInformation("Starting priority-based notification processing");

        var result = new ProcessingResult();
        var startTime = DateTime.UtcNow;

        try
        {
            // Process in priority order: Critical > High > Normal > Low
            var priorityOrder = new[]
            {
                NotificationPriority.Critical,
                NotificationPriority.High,
                NotificationPriority.Normal,
                NotificationPriority.Low
            };

            foreach (var priority in priorityOrder)
            {
                _logger.LogDebug("Processing {Priority} priority notifications", priority);

                var batchResult = await ProcessBatchAsync(50).ConfigureAwait(false);
                result.TotalProcessed += batchResult.TotalProcessed;
                result.SuccessCount += batchResult.SuccessCount;
                result.FailureCount += batchResult.FailureCount;
                result.SkippedCount += batchResult.SkippedCount;
            }

            _logger.LogInformation(
                "Priority-based processing completed: {Summary}",
                result.GetSummary());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during priority-based processing");
            result.Errors.Add(ex.Message);
        }
        finally
        {
            result.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// Calculates current system statistics
    /// </summary>
    public async Task<ProcessingStatistics> GetStatisticsAsync()
    {
        try
        {
            var allResults = await _resultRepository.GetAllAsync(0, 10000).ConfigureAwait(false);
            var configs = await _configRepository.GetEnabledAsync().ConfigureAwait(false);

            var stats = new ProcessingStatistics
            {
                PendingCount = (await _notificationRepository.GetPendingAsync()).Count,
                ProcessedCount = (await _notificationRepository.GetAllAsync()).Count,
                TotalDeliveryAttempts = allResults.Count,
                SuccessfulDeliveries = allResults.Count(r => r.IsSuccessful),
                FailedDeliveries = allResults.Count(r => r.Status == DeliveryStatus.Failed),
                AverageDeliveryTimeMs = allResults.Any() ? (long)allResults.Average(r => r.DurationMs) : 0,
                ActiveConfigurations = configs.Count,
                LastProcessedAt = _lastProcessedAt
            };

            stats.TotalNotifications = stats.PendingCount + stats.ProcessedCount;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistics");
            return new ProcessingStatistics();
        }
    }
}
