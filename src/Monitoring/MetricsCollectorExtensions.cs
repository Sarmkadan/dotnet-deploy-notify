#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetDeployNotify.Monitoring;

/// <summary>
/// Extension methods that add convenient query and bulk‑operation capabilities to <see cref="MetricsCollector"/>.
/// </summary>
public static class MetricsCollectorExtensions
{
    /// <summary>
    /// Returns the names of all recorded metrics.
    /// </summary>
    /// <param name="collector">The <see cref="MetricsCollector"/> instance.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing the metric names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetMetricNames(this MetricsCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);
        return collector.GetAllMetrics()
                        .Select(m => m.Name)
                        .ToList()
                        .AsReadOnly();
    }

    /// <summary>
    /// Retrieves the top <paramref name="count"/> metrics ordered by their average value (descending).
    /// </summary>
    /// <param name="collector">The <see cref="MetricsCollector"/> instance.</param>
    /// <param name="count">The maximum number of metrics to return. Must be greater than zero.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of <see cref="MetricValue"/> objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public static IReadOnlyList<MetricValue> GetTopMetricsByAverage(this MetricsCollector collector, int count)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        return collector.GetAllMetrics()
                        .OrderByDescending(m => m.Average)
                        .Take(count)
                        .ToList()
                        .AsReadOnly();
    }

    /// <summary>
    /// Produces a concise, culture‑invariant summary string for the specified metric.
    /// </summary>
    /// <param name="collector">The <see cref="MetricsCollector"/> instance.</param>
    /// <param name="name">The name of the metric to summarise.</param>
    /// <returns>A formatted string containing count, average, min, max and 95th percentile, or <c>null</c> if the metric does not exist.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> or <paramref name="name"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is an empty string.</exception>
    public static string? GetMetricSummary(this MetricsCollector collector, string name)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentException.ThrowIfNullOrEmpty(name);

        var stats = collector.GetStatistics(name);
        if (stats is null)
            return null;

        // Use invariant culture for machine‑readable formatting.
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}: Count={1}, Avg={2:F2}, Min={3:F2}, Max={4:F2}, P95={5:F2}",
            stats.Name,
            stats.Count,
            stats.Average,
            stats.Min,
            stats.Max,
            stats.Percentile95);
    }

    /// <summary>
    /// Resets every metric recorded by the collector.
    /// </summary>
    /// <param name="collector">The <see cref="MetricsCollector"/> instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collector"/> is <c>null</c>.</exception>
    public static void ResetAll(this MetricsCollector collector)
    {
        ArgumentNullException.ThrowIfNull(collector);

        foreach (var name in collector.GetAllMetrics().Select(m => m.Name))
        {
            collector.ResetMetric(name);
        }
    }
}
