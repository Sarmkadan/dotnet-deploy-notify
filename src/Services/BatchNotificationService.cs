#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Utilities;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for managing batch notifications
/// </summary>
public interface IBatchNotificationService
{
    /// <summary>Creates a new batch notification</summary>
    Task<string> CreateBatchAsync(BatchNotification batch);

    /// <summary>Gets a batch by ID</summary>
    Task<BatchNotification?> GetBatchAsync(string batchId);

    /// <summary>Adds a notification to a batch</summary>
    Task AddNotificationAsync(string batchId, DeploymentNotification notification);

    /// <summary>Removes a notification from a batch</summary>
    Task RemoveNotificationAsync(string batchId, string notificationId);

    /// <summary>Sends a batch of notifications</summary>
    Task<BatchNotificationResult> SendBatchAsync(string batchId);

    /// <summary>Gets all pending batches</summary>
    Task<List<BatchNotification>> GetPendingBatchesAsync();

    /// <summary>Cancels a batch</summary>
    Task CancelBatchAsync(string batchId);

    /// <summary>Gets batch statistics</summary>
    Task<BatchStatistics> GetBatchStatisticsAsync(string batchId);
}

/// <summary>
/// Statistics for a batch notification
/// </summary>
public class BatchStatistics
{
    /// <summary>Batch ID</summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>Total notifications in batch</summary>
    public int NotificationCount { get; set; }

    /// <summary>Total delivery targets</summary>
    public int TotalDeliveryTargets { get; set; }

    /// <summary>Completed deliveries</summary>
    public int CompletedDeliveries { get; set; }

    /// <summary>Pending deliveries</summary>
    public int PendingDeliveries { get; set; }

    /// <summary>Successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>Average delivery time</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>Success rate percentage</summary>
    public double SuccessRate { get; set; }

    /// <summary>Processing progress percentage</summary>
    public double ProgressPercentage => TotalDeliveryTargets > 0 ? (CompletedDeliveries * 100.0) / TotalDeliveryTargets : 0;
}

/// <summary>
/// In-memory implementation of batch notification service
/// </summary>
public class BatchNotificationService : IBatchNotificationService
{
    private readonly List<BatchNotification> _batches = new();
    private readonly INotificationService _notificationService;
    private readonly ILogger<BatchNotificationService> _logger;
    private readonly object _lockObject = new();

    /// <summary>Initializes the batch notification service</summary>
    public BatchNotificationService(
        INotificationService notificationService,
        ILogger<BatchNotificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(logger);
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new batch notification
    /// </summary>
    public Task<string> CreateBatchAsync(BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        if (!batch.IsValid())
        {
            throw new ArgumentException("Batch is not valid", nameof(batch));
        }

        lock (_lockObject)
        {
            _batches.Add(batch);
            _logger.LogInformation(
                "Batch {BatchId} created with {NotificationCount} notifications",
                batch.Id,
                batch.GetNotificationCount());
        }

        return Task.FromResult(batch.Id);
    }

    /// <summary>
    /// Retrieves a batch by ID
    /// </summary>
    public Task<BatchNotification?> GetBatchAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        lock (_lockObject)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == batchId);
            return Task.FromResult(batch);
        }
    }

    /// <summary>
    /// Adds a notification to a batch
    /// </summary>
    public Task AddNotificationAsync(string batchId, DeploymentNotification notification)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        ArgumentNullException.ThrowIfNull(notification);
        lock (_lockObject)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == batchId);
            if (batch is null)
            {
                throw new ArgumentException("Batch not found", nameof(batchId));
            }

            batch.AddNotification(notification);
            _logger.LogDebug(
                "Notification {NotificationId} added to batch {BatchId}",
                notification.Id,
                batchId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a notification from a batch
    /// </summary>
    public Task RemoveNotificationAsync(string batchId, string notificationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        ArgumentException.ThrowIfNullOrEmpty(notificationId);
        lock (_lockObject)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == batchId);
            if (batch is null)
            {
                throw new ArgumentException("Batch not found", nameof(batchId));
            }

            var removed = batch.RemoveNotification(notificationId);
            if (removed)
            {
                _logger.LogDebug(
                    "Notification {NotificationId} removed from batch {BatchId}",
                    notificationId,
                    batchId);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends all notifications in a batch
    /// </summary>
    public async Task<BatchNotificationResult> SendBatchAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        BatchNotification? batch;
        lock (_lockObject)
        {
            batch = _batches.FirstOrDefault(b => b.Id == batchId);
        }

        if (batch is null)
        {
            throw new ArgumentException("Batch not found", nameof(batchId));
        }

        if (!batch.IsReadyToSend())
        {
            throw new InvalidOperationException("Batch is not ready to send");
        }

        _logger.LogInformation("Sending batch {BatchId} with {NotificationCount} notifications",
            batchId, batch.GetNotificationCount());

        try
        {
            batch.Status = BatchStatus.Processing;

            // Send each notification with parallel dispatch and error isolation
            var batchResult = await ParallelBatchDispatcher.SendBatchWithParallelismAsync(
                batchId,
                batch.Notifications,
                async notification =>
                {
                    var results = await _notificationService.SendNotificationAsync(notification.Id, batch.Channels);
                    batch.TotalDeliveryAttempts += batch.Channels.Count;
                    batch.SuccessfulDeliveries += results.Count(r => r.IsSuccessful);
                    batch.FailedDeliveries += results.Count(r => !r.IsSuccessful);
                    return results;
                },
                _logger);

            // Update batch statistics from the results
            batch.TotalDeliveryAttempts = batchResult.TotalDeliveryAttempts;
            batch.SuccessfulDeliveries = batchResult.SuccessfulDeliveries;
            batch.FailedDeliveries = batchResult.FailedDeliveries;

            if (batchResult.IsSuccessful)
            {
                batch.MarkAsSent();
                _logger.LogInformation(
                    "Batch {BatchId} sent: {Summary}",
                    batchId,
                    batch.GetSummary());
            }
            else
            {
                batch.Status = BatchStatus.PartiallyCompleted;
                _logger.LogWarning(
                    "Batch {BatchId} partially completed: {Summary}",
                    batchId,
                    batch.GetSummary());
            }

            return batchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch {BatchId}", batchId);
            batch.MarkAsFailed();
            throw;
        }
    }

    /// <summary>
    /// Gets all pending batches
    /// </summary>
    public Task<List<BatchNotification>> GetPendingBatchesAsync()
    {
        lock (_lockObject)
        {
            var pending = _batches
                .Where(b => b.Status == BatchStatus.Pending && b.IsReadyToSend())
                .OrderBy(b => b.ScheduledAt ?? b.CreatedAt)
                .ToList();

            return Task.FromResult(pending);
        }
    }

    /// <summary>
    /// Cancels a batch
    /// </summary>
    public Task CancelBatchAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        lock (_lockObject)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == batchId);
            if (batch is null)
            {
                throw new ArgumentException("Batch not found", nameof(batchId));
            }

            if (batch.Status == BatchStatus.Processing || batch.Status == BatchStatus.Sent)
            {
                throw new InvalidOperationException("Cannot cancel a batch that is being processed or already sent");
            }

            batch.MarkAsCancelled();
            _logger.LogInformation("Batch {BatchId} cancelled", batchId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets statistics for a batch
    /// </summary>
    public Task<BatchStatistics> GetBatchStatisticsAsync(string batchId)
    {
        ArgumentException.ThrowIfNullOrEmpty(batchId);
        lock (_lockObject)
        {
            var batch = _batches.FirstOrDefault(b => b.Id == batchId);
            if (batch is null)
            {
                throw new ArgumentException("Batch not found", nameof(batchId));
            }

            var stats = new BatchStatistics
            {
                BatchId = batch.Id,
                NotificationCount = batch.GetNotificationCount(),
                TotalDeliveryTargets = batch.GetTotalDeliveryTargets(),
                CompletedDeliveries = batch.SuccessfulDeliveries + batch.FailedDeliveries,
                PendingDeliveries = batch.GetTotalDeliveryTargets() - (batch.SuccessfulDeliveries + batch.FailedDeliveries),
                SuccessfulDeliveries = batch.SuccessfulDeliveries,
                FailedDeliveries = batch.FailedDeliveries,
                SuccessRate = batch.GetSuccessRate()
            };

            return Task.FromResult(stats);
        }
    }
}