#nullable enable

using System.Diagnostics.CodeAnalysis;

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="MetricsSnapshot"/> and <see cref="ChannelMetrics"/> types used in MetricsServiceTests.
/// </summary>
public static class MetricsServiceTestsValidation
{
    /// <summary>
    /// Validates a <see cref="MetricsSnapshot"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The metrics snapshot to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MetricsSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Timestamp (should be recent, not default or in the future)
        if (value.Timestamp == default)
        {
            problems.Add("MetricsSnapshot.Timestamp must not be default(DateTime).");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("MetricsSnapshot.Timestamp must not be in the future.");
        }

        // Validate counts (should be non-negative)
        if (value.NotificationsCreated < 0)
        {
            problems.Add("MetricsSnapshot.NotificationsCreated must be non-negative.");
        }

        if (value.DeliveryAttempts < 0)
        {
            problems.Add("MetricsSnapshot.DeliveryAttempts must be non-negative.");
        }

        if (value.SuccessfulDeliveries < 0)
        {
            problems.Add("MetricsSnapshot.SuccessfulDeliveries must be non-negative.");
        }

        if (value.FailedDeliveries < 0)
        {
            problems.Add("MetricsSnapshot.FailedDeliveries must be non-negative.");
        }

        if (value.ValidationFailures < 0)
        {
            problems.Add("MetricsSnapshot.ValidationFailures must be non-negative.");
        }

        if (value.ConfigurationChanges < 0)
        {
            problems.Add("MetricsSnapshot.ConfigurationChanges must be non-negative.");
        }

        // Validate time metrics (should be non-negative)
        if (value.AverageDeliveryTimeMs < 0)
        {
            problems.Add("MetricsSnapshot.AverageDeliveryTimeMs must be non-negative.");
        }

        if (value.MinDeliveryTimeMs < 0)
        {
            problems.Add("MetricsSnapshot.MinDeliveryTimeMs must be non-negative.");
        }

        if (value.MaxDeliveryTimeMs < 0)
        {
            problems.Add("MetricsSnapshot.MaxDeliveryTimeMs must be non-negative.");
        }

        if (value.P95DeliveryTimeMs < 0)
        {
            problems.Add("MetricsSnapshot.P95DeliveryTimeMs must be non-negative.");
        }

        if (value.P99DeliveryTimeMs < 0)
        {
            problems.Add("MetricsSnapshot.P99DeliveryTimeMs must be non-negative.");
        }

        // Validate time metric relationships
        if (value.MinDeliveryTimeMs > value.MaxDeliveryTimeMs)
        {
            problems.Add("MetricsSnapshot.MinDeliveryTimeMs must not be greater than MaxDeliveryTimeMs.");
        }

        if (value.AverageDeliveryTimeMs < value.MinDeliveryTimeMs || value.AverageDeliveryTimeMs > value.MaxDeliveryTimeMs)
        {
            problems.Add("MetricsSnapshot.AverageDeliveryTimeMs must be between MinDeliveryTimeMs and MaxDeliveryTimeMs.");
        }

        // Validate ChannelMetrics dictionary
        if (value.ChannelMetrics is null)
        {
            problems.Add("MetricsSnapshot.ChannelMetrics must not be null.");
        }
        else
        {
            foreach (var kvp in value.ChannelMetrics)
            {
                if (kvp.Key == default)
                {
                    problems.Add("MetricsSnapshot.ChannelMetrics contains an entry with null or default NotificationChannel.");
                }

                if (kvp.Value is null)
                {
                    problems.Add($"MetricsSnapshot.ChannelMetrics[{kvp.Key}] is null.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="ChannelMetrics"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The channel metrics to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ChannelMetrics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Channel
        if (value.Channel == default)
        {
            problems.Add("ChannelMetrics.Channel must not be default(NotificationChannel).");
        }

        // Validate counts (should be non-negative)
        if (value.DeliveryAttempts < 0)
        {
            problems.Add("ChannelMetrics.DeliveryAttempts must be non-negative.");
        }

        if (value.SuccessfulDeliveries < 0)
        {
            problems.Add("ChannelMetrics.SuccessfulDeliveries must be non-negative.");
        }

        if (value.FailedDeliveries < 0)
        {
            problems.Add("ChannelMetrics.FailedDeliveries must be non-negative.");
        }

        if (value.AverageDeliveryTimeMs < 0)
        {
            problems.Add("ChannelMetrics.AverageDeliveryTimeMs must be non-negative.");
        }

        if (value.TotalNotifications < 0)
        {
            problems.Add("ChannelMetrics.TotalNotifications must be non-negative.");
        }

        // Validate time metric relationships
        if (value.SuccessfulDeliveries > value.DeliveryAttempts)
        {
            problems.Add("ChannelMetrics.SuccessfulDeliveries must not exceed DeliveryAttempts.");
        }

        if (value.FailedDeliveries > value.DeliveryAttempts)
        {
            problems.Add("ChannelMetrics.FailedDeliveries must not exceed DeliveryAttempts.");
        }

        // Validate LastDeliveryAt (should be recent or null)
        if (value.LastDeliveryAt.HasValue && value.LastDeliveryAt.Value > DateTime.UtcNow.AddHours(1))
        {
            problems.Add("ChannelMetrics.LastDeliveryAt must not be in the future.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="MetricsSnapshot"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics snapshot to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this MetricsSnapshot value) => Validate(value).Count == 0;

    /// <summary>
    /// Determines whether a <see cref="ChannelMetrics"/> instance is valid.
    /// </summary>
    /// <param name="value">The channel metrics to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ChannelMetrics value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="MetricsSnapshot"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The metrics snapshot to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the metrics snapshot is invalid.</exception>
    public static void EnsureValid(this MetricsSnapshot value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MetricsSnapshot is invalid:{System.Environment.NewLine} - {string.Join($"{System.Environment.NewLine} - ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a <see cref="ChannelMetrics"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The channel metrics to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the channel metrics are invalid.</exception>
    public static void EnsureValid(this ChannelMetrics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ChannelMetrics is invalid:{System.Environment.NewLine} - {string.Join($"{System.Environment.NewLine} - ", problems)}");
        }
    }
}