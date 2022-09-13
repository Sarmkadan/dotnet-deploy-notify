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
/// Interface for collecting system metrics and analytics
/// </summary>
public interface IMetricsService
{
    /// <summary>Records a notification being created</summary>
    void RecordNotificationCreated();

    /// <summary>Records a delivery attempt</summary>
    void RecordDeliveryAttempt(NotificationChannel channel, bool success, long durationMs);

    /// <summary>Records a validation failure</summary>
    void RecordValidationFailure();

    /// <summary>Records a configuration change</summary>
    void RecordConfigurationChange();

    /// <summary>Gets metrics snapshot</summary>
    Task<MetricsSnapshot> GetMetricsAsync();

    /// <summary>Gets metrics by time period</summary>
    Task<MetricsSnapshot> GetMetricsByPeriodAsync(DateTime startTime, DateTime endTime);

    /// <summary>Gets channel-specific metrics</summary>
    Task<ChannelMetrics> GetChannelMetricsAsync(NotificationChannel channel);
}

/// <summary>
/// Represents a snapshot of system metrics
/// </summary>
public class MetricsSnapshot
{
    /// <summary>Timestamp of the metrics snapshot</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Total notifications created</summary>
    public long NotificationsCreated { get; set; }

    /// <summary>Total delivery attempts</summary>
    public long DeliveryAttempts { get; set; }

    /// <summary>Total successful deliveries</summary>
    public long SuccessfulDeliveries { get; set; }

    /// <summary>Total failed deliveries</summary>
    public long FailedDeliveries { get; set; }

    /// <summary>Total validation failures</summary>
    public long ValidationFailures { get; set; }

    /// <summary>Total configuration changes</summary>
    public long ConfigurationChanges { get; set; }

    /// <summary>Average delivery time in milliseconds</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>Minimum delivery time recorded</summary>
    public long MinDeliveryTimeMs { get; set; }

    /// <summary>Maximum delivery time recorded</summary>
    public long MaxDeliveryTimeMs { get; set; }

    /// <summary>P95 (95th percentile) delivery time</summary>
    public long P95DeliveryTimeMs { get; set; }

    /// <summary>P99 (99th percentile) delivery time</summary>
    public long P99DeliveryTimeMs { get; set; }

    /// <summary>Per-channel metrics</summary>
    public Dictionary<NotificationChannel, ChannelMetrics> ChannelMetrics { get; set; } = new();

    /// <summary>Gets the overall success rate</summary>
    public double GetSuccessRate()
    {
        if (DeliveryAttempts == 0)
            return 0;
        return (SuccessfulDeliveries * 100.0) / DeliveryAttempts;
    }

    /// <summary>Gets the failure rate</summary>
    public double GetFailureRate()
    {
        if (DeliveryAttempts == 0)
            return 0;
        return (FailedDeliveries * 100.0) / DeliveryAttempts;
    }
}

/// <summary>
/// Metrics specific to a single notification channel
/// </summary>
public class ChannelMetrics
{
    /// <summary>Channel type</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Total delivery attempts to this channel</summary>
    public long DeliveryAttempts { get; set; }

    /// <summary>Successful deliveries to this channel</summary>
    public long SuccessfulDeliveries { get; set; }

    /// <summary>Failed deliveries to this channel</summary>
    public long FailedDeliveries { get; set; }

    /// <summary>Average delivery time for this channel</summary>
    public long AverageDeliveryTimeMs { get; set; }

    /// <summary>Last delivery time to this channel</summary>
    public DateTime? LastDeliveryAt { get; set; }

    /// <summary>Total notifications sent through this channel</summary>
    public long TotalNotifications { get; set; }

    /// <summary>Gets success rate for this channel</summary>
    public double GetSuccessRate()
    {
        if (DeliveryAttempts == 0)
            return 0;
        return (SuccessfulDeliveries * 100.0) / DeliveryAttempts;
    }

    /// <summary>Gets a summary string for this channel</summary>
    public string GetSummary()
    {
        return $"{Channel}: {SuccessfulDeliveries}/{DeliveryAttempts} successful ({GetSuccessRate():F1}%) | " +
               $"Avg: {AverageDeliveryTimeMs}ms | Last: {LastDeliveryAt?.ToString("u") ?? "Never"}";
    }
}

/// <summary>
/// In-memory implementation of metrics service
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly object _lockObject = new();

    private long _notificationsCreated;
    private long _deliveryAttempts;
    private long _successfulDeliveries;
    private long _failedDeliveries;
    private long _validationFailures;
    private long _configurationChanges;

    private readonly List<long> _deliveryTimes = new();
    private readonly Dictionary<NotificationChannel, ChannelMetrics> _channelMetrics = new();
    private readonly ILogger<MetricsService> _logger;

    private readonly List<DeliveryEvent> _deliveryEvents = new();
    private readonly List<DateTime> _notificationCreatedTimestamps = new();
    private readonly List<DateTime> _validationFailureTimestamps = new();
    private readonly List<DateTime> _configurationChangeTimestamps = new();

    private readonly record struct DeliveryEvent(DateTime Timestamp, NotificationChannel Channel, bool Success, long DurationMs);

    /// <summary>Initializes the metrics service</summary>
    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;

        // Initialize channel metrics
        foreach (NotificationChannel channel in Enum.GetValues(typeof(NotificationChannel)))
        {
            _channelMetrics[channel] = new ChannelMetrics { Channel = channel };
        }
    }

    /// <summary>
    /// Records a notification creation
    /// </summary>
    public void RecordNotificationCreated()
    {
        lock (_lockObject)
        {
            _notificationsCreated++;
            _notificationCreatedTimestamps.Add(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Records a delivery attempt result
    /// </summary>
    public void RecordDeliveryAttempt(NotificationChannel channel, bool success, long durationMs)
    {
        lock (_lockObject)
        {
            _deliveryAttempts++;
            _deliveryTimes.Add(durationMs);
            _deliveryEvents.Add(new DeliveryEvent(DateTime.UtcNow, channel, success, durationMs));

            if (success)
            {
                _successfulDeliveries++;
                _channelMetrics[channel].SuccessfulDeliveries++;
            }
            else
            {
                _failedDeliveries++;
                _channelMetrics[channel].FailedDeliveries++;
            }

            _channelMetrics[channel].DeliveryAttempts++;
            if (_deliveryTimes.Any())
            {
                _channelMetrics[channel].AverageDeliveryTimeMs = (long)_deliveryTimes.Average();
            }
            _channelMetrics[channel].LastDeliveryAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Records a validation failure
    /// </summary>
    public void RecordValidationFailure()
    {
        lock (_lockObject)
        {
            _validationFailures++;
            _validationFailureTimestamps.Add(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Records a configuration change
    /// </summary>
    public void RecordConfigurationChange()
    {
        lock (_lockObject)
        {
            _configurationChanges++;
            _configurationChangeTimestamps.Add(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Gets current metrics snapshot
    /// </summary>
    public Task<MetricsSnapshot> GetMetricsAsync()
    {
        lock (_lockObject)
        {
            var snapshot = new MetricsSnapshot
            {
                NotificationsCreated = _notificationsCreated,
                DeliveryAttempts = _deliveryAttempts,
                SuccessfulDeliveries = _successfulDeliveries,
                FailedDeliveries = _failedDeliveries,
                ValidationFailures = _validationFailures,
                ConfigurationChanges = _configurationChanges,
                ChannelMetrics = new Dictionary<NotificationChannel, ChannelMetrics>(
                    _channelMetrics.ToDictionary(x => x.Key, x => x.Value))
            };

            if (_deliveryTimes.Any())
            {
                snapshot.AverageDeliveryTimeMs = (long)_deliveryTimes.Average();
                snapshot.MinDeliveryTimeMs = _deliveryTimes.Min();
                snapshot.MaxDeliveryTimeMs = _deliveryTimes.Max();
                snapshot.P95DeliveryTimeMs = CalculatePercentile(95);
                snapshot.P99DeliveryTimeMs = CalculatePercentile(99);
            }

            return Task.FromResult(snapshot);
        }
    }

    /// <summary>
    /// Gets metrics restricted to events recorded within the given time period (inclusive).
    /// </summary>
    /// <param name="startTime">Start of the period (UTC).</param>
    /// <param name="endTime">End of the period (UTC).</param>
    /// <exception cref="ArgumentException"><paramref name="endTime"/> is earlier than <paramref name="startTime"/>.</exception>
    public Task<MetricsSnapshot> GetMetricsByPeriodAsync(DateTime startTime, DateTime endTime)
    {
        if (endTime < startTime)
            throw new ArgumentException("endTime must not be earlier than startTime.", nameof(endTime));

        _logger.LogDebug("Getting metrics for period {StartTime} - {EndTime}", startTime, endTime);

        lock (_lockObject)
        {
            var events = _deliveryEvents
                .Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime)
                .ToList();

            var channelMetrics = new Dictionary<NotificationChannel, ChannelMetrics>();
            foreach (var group in events.GroupBy(e => e.Channel))
            {
                channelMetrics[group.Key] = new ChannelMetrics
                {
                    Channel = group.Key,
                    DeliveryAttempts = group.Count(),
                    SuccessfulDeliveries = group.Count(e => e.Success),
                    FailedDeliveries = group.Count(e => !e.Success),
                    AverageDeliveryTimeMs = (long)group.Average(e => e.DurationMs),
                    LastDeliveryAt = group.Max(e => e.Timestamp)
                };
            }

            var snapshot = new MetricsSnapshot
            {
                NotificationsCreated = _notificationCreatedTimestamps.Count(t => t >= startTime && t <= endTime),
                DeliveryAttempts = events.Count,
                SuccessfulDeliveries = events.Count(e => e.Success),
                FailedDeliveries = events.Count(e => !e.Success),
                ValidationFailures = _validationFailureTimestamps.Count(t => t >= startTime && t <= endTime),
                ConfigurationChanges = _configurationChangeTimestamps.Count(t => t >= startTime && t <= endTime),
                ChannelMetrics = channelMetrics
            };

            if (events.Count > 0)
            {
                var times = events.Select(e => e.DurationMs).ToList();
                snapshot.AverageDeliveryTimeMs = (long)times.Average();
                snapshot.MinDeliveryTimeMs = times.Min();
                snapshot.MaxDeliveryTimeMs = times.Max();
                snapshot.P95DeliveryTimeMs = CalculatePercentile(times, 95);
                snapshot.P99DeliveryTimeMs = CalculatePercentile(times, 99);
            }

            return Task.FromResult(snapshot);
        }
    }

    /// <summary>
    /// Gets metrics for a specific channel
    /// </summary>
    public Task<ChannelMetrics> GetChannelMetricsAsync(NotificationChannel channel)
    {
        lock (_lockObject)
        {
            if (_channelMetrics.TryGetValue(channel, out var metrics))
            {
                return Task.FromResult(metrics);
            }

            return Task.FromResult(new ChannelMetrics { Channel = channel });
        }
    }

    private long CalculatePercentile(int percentile) => CalculatePercentile(_deliveryTimes, percentile);

    private static long CalculatePercentile(List<long> values, int percentile)
    {
        if (values.Count == 0)
            return 0;

        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }
}
