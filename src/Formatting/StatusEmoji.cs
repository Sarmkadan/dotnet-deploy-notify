#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Formatting;

/// <summary>
/// Provides emoji representations for deployment build statuses
/// </summary>
public static class StatusEmoji
{
    /// <summary>
    /// Returns the emoji indicator for the given build status
    /// </summary>
    public static string Get(BuildStatus status) => status switch
    {
        BuildStatus.Success => "✅",
        BuildStatus.SuccessWithWarnings => "⚠️",
        BuildStatus.Failed => "❌",
        BuildStatus.DeploymentSuccess => "🚀",
        BuildStatus.DeploymentFailed => "💥",
        BuildStatus.Deploying => "🔄",
        BuildStatus.InProgress => "⏳",
        BuildStatus.Cancelled => "🛑",
        BuildStatus.Started => "▶️",
        _ => "ℹ️"
    };

    /// <summary>
    /// Returns the emoji indicator, or an empty string when emojis are disabled
    /// </summary>
    public static string Get(BuildStatus status, bool enableEmojis) =>
        enableEmojis ? Get(status) : string.Empty;

    /// <summary>
    /// Returns the status label with a leading emoji when enabled
    /// </summary>
    public static string Format(BuildStatus status, bool enableEmojis = true)
    {
        var label = status.ToString();
        if (!enableEmojis) return label;

        return $"{Get(status)} {label}";
    }
}
