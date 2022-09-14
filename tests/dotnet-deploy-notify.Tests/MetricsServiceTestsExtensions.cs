#nullable enable
using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Services;
using FluentAssertions;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods that make writing <see cref="MetricsServiceTests"/> more expressive and DRY.
/// </summary>
public static class MetricsServiceTestsExtensions
{
    /// <summary>
    /// Retrieves the private <c>_metricsService</c> field from a <see cref="MetricsServiceTests"/> instance.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <returns>The underlying <see cref="MetricsService"/> used by the test.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static MetricsService GetMetricsService(this MetricsServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        var field = typeof(MetricsServiceTests).GetField("_metricsService", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("Unable to locate _metricsService field.");
        return (MetricsService)field.GetValue(tests)!;
    }

    /// <summary>
    /// Asynchronously obtains a fresh snapshot of the current metrics from the test's <see cref="MetricsService"/>.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <returns>A <see cref="Task{MetricsSnapshot}"/> that resolves to the latest snapshot.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static Task<MetricsSnapshot> GetCurrentMetricsAsync(this MetricsServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return tests.GetMetricsService().GetMetricsAsync();
    }

    /// <summary>
    /// Asserts that the total number of notifications created matches the expected value.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="expected">The expected notification count (must be non‑negative).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expected"/> is negative.</exception>
    public static void AssertNotificationCreatedCount(this MetricsServiceTests tests, int expected)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentOutOfRangeException.ThrowIfNegative(expected);
        var metrics = tests.GetCurrentMetricsAsync().Result;
        metrics.NotificationsCreated.Should().Be(expected);
    }

    /// <summary>
    /// Asserts that delivery attempts (optionally filtered by <paramref name="channel"/>) match the expected count.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="expected">The expected number of delivery attempts (must be non‑negative).</param>
    /// <param name="channel">
    /// If supplied, the assertion is performed against the <see cref="ChannelMetrics"/> for that channel;
    /// otherwise the global <c>DeliveryAttempts</c> count is used.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expected"/> is negative.</exception>
    public static void AssertDeliveryAttemptsCount(this MetricsServiceTests tests, int expected, NotificationChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentOutOfRangeException.ThrowIfNegative(expected);
        var metrics = tests.GetCurrentMetricsAsync().Result;

        if (channel is null)
        {
            metrics.DeliveryAttempts.Should().Be(expected);
        }
        else
        {
            metrics.ChannelMetrics.TryGetValue(channel.Value, out var chMetrics).Should().BeTrue($"channel {channel.Value} should have metrics recorded");
            chMetrics!.DeliveryAttempts.Should().Be(expected);
        }
    }
}
