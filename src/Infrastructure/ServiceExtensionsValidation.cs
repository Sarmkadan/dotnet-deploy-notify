#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Provides validation helpers for ServiceExtensions extension methods and their parameters
/// </summary>
public static class ServiceExtensionsValidation
{
    /// <summary>
    /// Validates a DeploymentNotification object for common issues
    /// </summary>
    /// <param name="value">The notification to validate</param>
    /// <returns>List of human-readable validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this DeploymentNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            problems.Add("ProjectName is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("Version is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(value.BranchName))
        {
            problems.Add("BranchName is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(value.CommitHash))
        {
            problems.Add("CommitHash is required and cannot be empty");
        }

        // Validate enum values
        if (value.Status == default)
        {
            problems.Add("Status must be a valid BuildStatus value");
        }

        if (value.TargetEnvironment == default)
        {
            problems.Add("TargetEnvironment must be a valid Environment value");
        }

        // Validate collection properties
        if (value.Channels == null || value.Channels.Count == 0)
        {
            problems.Add("Channels collection must contain at least one channel");
        }

        // Validate duration
        if (value.DurationSeconds.HasValue && value.DurationSeconds.Value < 0)
        {
            problems.Add("DurationSeconds cannot be negative");
        }

        // Validate dates
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be a valid DateTime");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("CreatedAt cannot be in the future");
        }

        // Validate priority
        if (value.Priority == default)
        {
            problems.Add("Priority must be a valid NotificationPriority value");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a NotificationResult object for common issues
    /// </summary>
    /// <param name="value">The result to validate</param>
    /// <returns>List of human-readable validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.NotificationId))
        {
            problems.Add("NotificationId is required and cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(value.ConfigurationId))
        {
            problems.Add("ConfigurationId is required and cannot be empty");
        }

        // Validate enum values
        if (value.Channel == default)
        {
            problems.Add("Channel must be a valid NotificationChannel value");
        }

        if (value.Status == default)
        {
            problems.Add("Status must be a valid DeliveryStatus value");
        }

        // Validate duration
        if (value.DurationMs < 0)
        {
            problems.Add("DurationMs cannot be negative");
        }

        // Validate attempt number
        if (value.AttemptNumber < 1)
        {
            problems.Add("AttemptNumber must be at least 1");
        }

        // Validate dates
        if (value.AttemptedAt == default)
        {
            problems.Add("AttemptedAt must be a valid DateTime");
        }
        else if (value.AttemptedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("AttemptedAt cannot be in the future");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DeploymentNotification is valid
    /// </summary>
    /// <param name="value">The notification to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this DeploymentNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a NotificationResult is valid
    /// </summary>
    /// <param name="value">The result to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a DeploymentNotification is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The notification to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with all problems listed</exception>
    public static void EnsureValid(this DeploymentNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "DeploymentNotification validation failed: " + string.Join("; ", problems),
                nameof(value));
        }
    }

    /// <summary>
    /// Ensures a NotificationResult is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The result to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails with all problems listed</exception>
    public static void EnsureValid(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "NotificationResult validation failed: " + string.Join("; ", problems),
                nameof(value));
        }
    }
}