#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides extension methods for <see cref="DeploymentNotification"/> to enhance
/// deployment notification handling with common operations and validations.
/// </summary>
public static class DeploymentNotificationExtensions
{
    /// <summary>
    /// Determines whether the deployment notification represents a successful deployment.
    /// </summary>
    /// <param name="notification">The deployment notification to check.</param>
    /// <returns>True if the status is Success, DeploymentSuccess, or SuccessWithWarnings; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static bool IsSuccessful(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.Status is BuildStatus.Success
            or BuildStatus.DeploymentSuccess
            or BuildStatus.SuccessWithWarnings;
    }

    /// <summary>
    /// Determines whether the deployment notification represents a failed deployment.
    /// </summary>
    /// <param name="notification">The deployment notification to check.</param>
    /// <returns>True if the status is Failed or DeploymentFailed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static bool IsFailed(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return notification.Status is BuildStatus.Failed or BuildStatus.DeploymentFailed;
    }

    /// <summary>
    /// Gets the formatted deployment URL by combining the repository URL with the commit hash.
    /// Uses the BuildUrl if available, otherwise constructs a GitHub/GitLab URL based on the repository URL.
    /// </summary>
    /// <param name="notification">The deployment notification.</param>
    /// <returns>The formatted deployment URL, or null if no URL can be constructed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static string? GetDeploymentUrl(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            return notification.BuildUrl;
        }

        if (string.IsNullOrWhiteSpace(notification.RepositoryUrl) ||
            string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            return null;
        }

        string repoUrl = notification.RepositoryUrl.TrimEnd('/');

        if (repoUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return $"{repoUrl}/commit/{notification.CommitHash}";
        }

        if (repoUrl.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase))
        {
            return $"{repoUrl}/-/commit/{notification.CommitHash}";
        }

        return repoUrl;
    }

    /// <summary>
    /// Gets the formatted duration of the deployment in a human-readable format.
    /// </summary>
    /// <param name="notification">The deployment notification.</param>
    /// <returns>A formatted string representing the duration (e.g., "2m 30s", "1h 5m 15s"), or "N/A" if duration is not available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static string GetFormattedDuration(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification.DurationSeconds == null || notification.DurationSeconds <= 0)
        {
            return "N/A";
        }

        int totalSeconds = notification.DurationSeconds.Value;
        var parts = new List<string>();

        int hours = totalSeconds / 3600;
        if (hours > 0)
        {
            parts.Add($"{hours}h");
            totalSeconds %= 3600;
        }

        int minutes = totalSeconds / 60;
        if (minutes > 0)
        {
            parts.Add($"{minutes}m");
            totalSeconds %= 60;
        }

        if (totalSeconds > 0 || parts.Count == 0)
        {
            parts.Add($"{totalSeconds}s");
        }

        return string.Join(" ", parts);
    }
}