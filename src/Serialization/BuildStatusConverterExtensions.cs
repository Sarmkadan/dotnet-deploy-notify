#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Serialization;

/// <summary>
/// Provides extension methods for <see cref="BuildStatusConverter"/> to work with <see cref="BuildStatus"/> values.
/// </summary>
public static class BuildStatusConverterExtensions
{
    /// <summary>
    /// Determines whether the specified build status represents a successful completion.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to check.</param>
    /// <returns>True if the status is Success, SuccessWithWarnings, or DeploymentSuccess; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static bool IsSuccessful(this BuildStatusConverter converter, BuildStatus status)
    {
        return status is BuildStatus.Success or BuildStatus.SuccessWithWarnings or BuildStatus.DeploymentSuccess;
    }

    /// <summary>
    /// Determines whether the specified build status represents a failure.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to check.</param>
    /// <returns>True if the status is Failed, DeploymentFailed, or Cancelled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static bool IsFailed(this BuildStatusConverter converter, BuildStatus status)
    {
        return status is BuildStatus.Failed or BuildStatus.DeploymentFailed or BuildStatus.Cancelled;
    }

    /// <summary>
    /// Determines whether the specified build status represents an in-progress or pending state.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to check.</param>
    /// <returns>True if the status is Started, InProgress, or Deploying; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static bool IsInProgress(this BuildStatusConverter converter, BuildStatus status)
    {
        return status is BuildStatus.Started or BuildStatus.InProgress or BuildStatus.Deploying;
    }

    /// <summary>
    /// Gets a user-friendly display name for the build status.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to get the display name for.</param>
    /// <returns>A localized display name for the build status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static string GetDisplayName(this BuildStatusConverter converter, BuildStatus status)
    {
        return status switch
        {
            BuildStatus.Started => "Build Started",
            BuildStatus.InProgress => "Build In Progress",
            BuildStatus.Success => "Build Success",
            BuildStatus.Failed => "Build Failed",
            BuildStatus.Cancelled => "Build Cancelled",
            BuildStatus.SuccessWithWarnings => "Build Success (with warnings)",
            BuildStatus.Deploying => "Deploying",
            BuildStatus.DeploymentSuccess => "Deployment Success",
            BuildStatus.DeploymentFailed => "Deployment Failed",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Parses a string value into a BuildStatus enum value.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="statusString">The string representation of the build status.</param>
    /// <returns>The parsed BuildStatus value.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="statusString"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when the string cannot be parsed to a valid BuildStatus.</exception>
    public static BuildStatus ParseStatus(this BuildStatusConverter converter, string statusString)
    {
        ArgumentException.ThrowIfNullOrEmpty(statusString, nameof(statusString));

        return Enum.Parse<BuildStatus>(statusString, ignoreCase: true);
    }

    /// <summary>
    /// Attempts to parse a string value into a BuildStatus enum value.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="statusString">The string representation of the build status.</param>
    /// <param name="status">Receives the parsed BuildStatus value if successful.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParseStatus(this BuildStatusConverter converter, string statusString, out BuildStatus status)
    {
        status = BuildStatus.Started; // Default value

        if (string.IsNullOrWhiteSpace(statusString))
        {
            return false;
        }

        return Enum.TryParse(statusString, ignoreCase: true, out status);
    }

    /// <summary>
    /// Gets the priority level for a build status (useful for notification routing).
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to get the priority for.</param>
    /// <returns>A NotificationPriority value indicating the severity level.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static NotificationPriority GetPriority(this BuildStatusConverter converter, BuildStatus status)
    {
        return status switch
        {
            BuildStatus.Failed or BuildStatus.DeploymentFailed or BuildStatus.Cancelled => NotificationPriority.Critical,
            BuildStatus.Deploying => NotificationPriority.High,
            BuildStatus.InProgress => NotificationPriority.Normal,
            BuildStatus.Started => NotificationPriority.Low,
            BuildStatus.Success or BuildStatus.SuccessWithWarnings or BuildStatus.DeploymentSuccess => NotificationPriority.Low,
            _ => NotificationPriority.Normal
        };
    }

    /// <summary>
    /// Gets a CSS class name suitable for styling build status indicators.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to get the CSS class for.</param>
    /// <returns>A CSS class name (e.g., "status-success", "status-failed").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static string GetCssClass(this BuildStatusConverter converter, BuildStatus status)
    {
        return status switch
        {
            BuildStatus.Success or BuildStatus.SuccessWithWarnings or BuildStatus.DeploymentSuccess => "status-success",
            BuildStatus.Failed or BuildStatus.DeploymentFailed => "status-failed",
            BuildStatus.Cancelled => "status-cancelled",
            BuildStatus.InProgress => "status-in-progress",
            BuildStatus.Deploying => "status-deploying",
            BuildStatus.Started => "status-started",
            _ => "status-unknown"
        };
    }

    /// <summary>
    /// Determines whether two BuildStatus values represent the same state.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The first build status to compare.</param>
    /// <param name="other">The second build status to compare.</param>
    /// <returns>True if both status values are equal; otherwise, false.</returns>
    public static bool IsSameAs(this BuildStatusConverter converter, BuildStatus status, BuildStatus other)
    {
        return status == other;
    }

    /// <summary>
    /// Gets a numeric value representing the build status severity (0 = lowest, 10 = highest).
    /// Useful for sorting and comparison operations.
    /// </summary>
    /// <param name="converter">The converter instance (used for extension method syntax).</param>
    /// <param name="status">The build status to get the severity for.</param>
    /// <returns>A numeric severity value between 0 and 10.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="status"/> is not a valid BuildStatus value.</exception>
    public static int GetSeverity(this BuildStatusConverter converter, BuildStatus status)
    {
        return status switch
        {
            BuildStatus.Cancelled => 10,
            BuildStatus.Failed or BuildStatus.DeploymentFailed => 9,
            BuildStatus.Deploying => 8,
            BuildStatus.InProgress => 7,
            BuildStatus.Started => 6,
            BuildStatus.SuccessWithWarnings => 5,
            BuildStatus.DeploymentSuccess => 4,
            BuildStatus.Success => 3,
            _ => 0
        };
    }
}