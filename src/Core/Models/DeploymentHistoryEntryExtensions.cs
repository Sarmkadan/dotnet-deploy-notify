#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides extension methods for <see cref="DeploymentHistoryEntry"/> to enable
/// common operations on deployment history records without modifying the original class.
/// </summary>
public static class DeploymentHistoryEntryExtensions
{
    /// <summary>
    /// Determines whether the deployment occurred within the specified time window.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <param name="timeWindow">The time window to check against.</param>
    /// <returns>True if the deployment occurred within the time window; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsWithinTimeWindow(this DeploymentHistoryEntry entry, TimeSpan timeWindow)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var now = DateTime.UtcNow;
        var windowStart = now - timeWindow;
        return entry.DeployedAt >= windowStart && entry.DeployedAt <= now;
    }

    /// <summary>
    /// Determines whether the deployment occurred within the specified time window
    /// relative to a reference time.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <param name="referenceTime">The reference time to calculate the window from.</param>
    /// <param name="timeWindow">The time window to check against.</param>
    /// <returns>True if the deployment occurred within the time window; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsWithinTimeWindow(this DeploymentHistoryEntry entry, DateTime referenceTime, TimeSpan timeWindow)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var windowStart = referenceTime - timeWindow;
        var windowEnd = referenceTime + timeWindow;
        return entry.DeployedAt >= windowStart && entry.DeployedAt <= windowEnd;
    }

    /// <summary>
    /// Checks if the deployment has a specific tag with the given key.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <param name="tagKey">The tag key to look for.</param>
    /// <returns>True if the tag exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tagKey"/> is null.</exception>
    public static bool HasTag(this DeploymentHistoryEntry entry, string tagKey)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(tagKey);

        return entry.Tags.ContainsKey(tagKey);
    }

    /// <summary>
    /// Gets the value of a specific tag if it exists.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <param name="tagKey">The tag key to look for.</param>
    /// <returns>The tag value if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tagKey"/> is null.</exception>
    public static string? GetTagValue(this DeploymentHistoryEntry entry, string tagKey)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(tagKey);

        if (entry.Tags.TryGetValue(tagKey, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Gets the deployment duration as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="entry">The deployment history entry.</param>
    /// <returns>The deployment duration as a TimeSpan, or null if not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static TimeSpan? GetDuration(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.DurationSeconds.HasValue
            ? TimeSpan.FromSeconds(entry.DurationSeconds.Value)
            : null;
    }

    /// <summary>
    /// Determines whether the deployment was successful.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <returns>True if the deployment was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsSuccessful(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.FinalStatus is BuildStatus.Success or BuildStatus.DeploymentSuccess;
    }

    /// <summary>
    /// Determines whether the deployment failed.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <returns>True if the deployment failed; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsFailed(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.FinalStatus is BuildStatus.Failed or BuildStatus.DeploymentFailed;
    }

    /// <summary>
    /// Gets a formatted display string for the deployment duration.
    /// </summary>
    /// <param name="entry">The deployment history entry.</param>
    /// <returns>A formatted duration string (e.g., "2m 30s"), or "N/A" if not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static string GetFormattedDuration(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.DurationSeconds.HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(entry.DurationSeconds.Value);
            return FormatTimeSpan(timeSpan);
        }

        return "N/A";
    }

    /// <summary>
    /// Formats a TimeSpan into a human-readable string (e.g., "2m 30s").
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>A formatted time span string.</returns>
    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        var parts = new List<string>();

        if (timeSpan.TotalHours >= 1)
        {
            parts.Add($"{timeSpan.TotalHours:F0}h");
        }

        if (timeSpan.TotalMinutes >= 1)
        {
            parts.Add($"{timeSpan.Minutes}m");
        }

        parts.Add($"{timeSpan.Seconds}s");

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Determines whether the deployment is a rollback.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <returns>True if this is a rollback deployment; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsRollback(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.IsRollback;
    }

    /// <summary>
    /// Gets a summary of the deployment status as a string.
    /// </summary>
    /// <param name="entry">The deployment history entry.</param>
    /// <returns>A status summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static string GetStatusSummary(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.IsSuccessful()
            ? "SUCCESS"
            : entry.IsFailed()
                ? "FAILED"
                : entry.FinalStatus.ToString();
    }

    /// <summary>
    /// Checks if the deployment matches the specified environment.
    /// </summary>
    /// <param name="entry">The deployment history entry to check.</param>
    /// <param name="environment">The environment to match against.</param>
    /// <returns>True if the deployment matches the environment; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static bool IsInEnvironment(this DeploymentHistoryEntry entry, Environment environment)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.TargetEnvironment == environment;
    }

    /// <summary>
    /// Gets a dictionary of all tags formatted as key-value pairs.
    /// </summary>
    /// <param name="entry">The deployment history entry.</param>
    /// <returns>A read-only dictionary of tag key-value pairs.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetTags(this DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return entry.Tags.AsReadOnly();
    }
}
