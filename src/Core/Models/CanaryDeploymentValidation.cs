#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Validation;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="CanaryDeployment"/> instances
/// </summary>
public static class CanaryDeploymentValidation
{
    /// <summary>
    /// Validates a <see cref="CanaryDeployment"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The canary deployment to validate</param>
    /// <returns>An immutable list of validation errors; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this CanaryDeployment value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        ValidationGuard.CheckRequired(errors, value.ProjectName, nameof(value.ProjectName));
        ValidationGuard.CheckRequired(errors, value.StableVersion, nameof(value.StableVersion));
        ValidationGuard.CheckRequired(errors, value.CanaryVersion, nameof(value.CanaryVersion));

        if (!string.IsNullOrWhiteSpace(value.StableVersion) && value.StableVersion == value.CanaryVersion)
        {
            errors.Add("CanaryVersion must differ from StableVersion.");
        }

        // Validate enum properties against their defined members
        ValidationGuard.CheckDefinedEnum(errors, value.TargetEnvironment, nameof(value.TargetEnvironment));
        ValidationGuard.CheckDefinedEnum(errors, value.Strategy, nameof(value.Strategy));
        ValidationGuard.CheckDefinedEnum(errors, value.Status, nameof(value.Status));
        ValidationGuard.CheckDefinedEnum(errors, value.Priority, nameof(value.Priority));

        // Validate string properties that can be empty but not null
        ValidationGuard.CheckNotNull(errors, value.InitiatedBy, nameof(value.InitiatedBy));
        ValidationGuard.CheckNotNull(errors, value.BranchName, nameof(value.BranchName));
        ValidationGuard.CheckNotNull(errors, value.CommitHash, nameof(value.CommitHash));
        ValidationGuard.CheckNotNull(errors, value.BuildUrl, nameof(value.BuildUrl));

        // Validate TrafficSplit ranges
        ValidationGuard.CheckInRange(errors, value.CurrentSplit.StablePercent, 0, 100, "CurrentSplit.StablePercent");
        ValidationGuard.CheckInRange(errors, value.CurrentSplit.CanaryPercent, 0, 100, "CurrentSplit.CanaryPercent");

        if (Math.Abs(value.CurrentSplit.StablePercent + value.CurrentSplit.CanaryPercent - 100) > 0.01)
        {
            errors.Add("CurrentSplit must sum to 100% (StablePercent + CanaryPercent = 100)");
        }

        // Validate RolloutPlan
        ValidationGuard.CheckNotNull(errors, value.RolloutPlan, nameof(value.RolloutPlan));

        if (value.RolloutPlan is { Count: 0 })
        {
            errors.Add("RolloutPlan must contain at least one step.");
        }
        else if (value.RolloutPlan is { } rolloutPlan)
        {
            // Validate each rollout step
            foreach (var step in rolloutPlan)
            {
                if (step is null)
                {
                    errors.Add("RolloutPlan contains a null step.");
                    continue;
                }

                var stepPrefix = $"RolloutPlan step {step.StepNumber}";

                ValidationGuard.CheckPositive(errors, step.StepNumber, $"{stepPrefix}: StepNumber");
                ValidationGuard.CheckInRange(errors, step.CanaryPercent, 0, 100, $"{stepPrefix}: CanaryPercent");
                ValidationGuard.CheckPositive(errors, step.SoakDuration, $"{stepPrefix}: SoakDuration");
                ValidationGuard.CheckDefinedEnum(errors, step.Status, $"{stepPrefix}: Status");
            }
        }

        // Validate metrics
        ValidationGuard.CheckNotNull(errors, value.StableMetrics, nameof(value.StableMetrics));
        ValidationGuard.CheckNotNull(errors, value.CanaryMetrics, nameof(value.CanaryMetrics));

        if (value.StableMetrics is { } stableMetrics)
        {
            ValidateMetrics(stableMetrics, nameof(value.StableMetrics), errors);
        }

        if (value.CanaryMetrics is { } canaryMetrics)
        {
            ValidateMetrics(canaryMetrics, nameof(value.CanaryMetrics), errors);
        }

        // Validate CreatedAt (must not be default)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a non-default DateTime");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future");
        }

        // Validate PromotedAt
        if (value.PromotedAt.HasValue)
        {
            if (value.PromotedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("PromotedAt cannot be in the future");
            }

            if (value.PromotedAt.Value < value.CreatedAt)
            {
                errors.Add("PromotedAt cannot be before CreatedAt");
            }
        }

        // Validate AbortedAt
        if (value.AbortedAt.HasValue)
        {
            if (value.AbortedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("AbortedAt cannot be in the future");
            }

            if (value.AbortedAt.Value < value.CreatedAt)
            {
                errors.Add("AbortedAt cannot be before CreatedAt");
            }
        }

        // Validate Status consistency with timestamps
        if (value.Status == CanaryStatus.Promoted && !value.PromotedAt.HasValue)
        {
            errors.Add("Status is Promoted but PromotedAt is not set");
        }

        if (value.Status == CanaryStatus.Aborted && !value.AbortedAt.HasValue)
        {
            errors.Add("Status is Aborted but AbortedAt is not set");
        }

        if (value.Status == CanaryStatus.Aborted && string.IsNullOrWhiteSpace(value.AbortReason))
        {
            errors.Add("Status is Aborted but AbortReason is not provided");
        }

        // Validate NotificationChannels
        ValidationGuard.CheckNotNull(errors, value.NotificationChannels, nameof(value.NotificationChannels));

        if (value.NotificationChannels is { } channels)
        {
            foreach (var (index, channel) in channels.Index())
            {
                ValidationGuard.CheckDefinedEnum(errors, channel, $"NotificationChannels[{index}]");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the health metrics for a canary deployment
    /// </summary>
    /// <param name="metrics">The metrics to validate</param>
    /// <param name="metricsName">Name of the metrics property for error messages</param>
    /// <param name="errors">List to accumulate validation errors</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/>, <paramref name="metricsName"/> or <paramref name="errors"/> is <see langword="null"/></exception>
    private static void ValidateMetrics(CanaryMetrics metrics, string metricsName, List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(metricsName);
        ArgumentNullException.ThrowIfNull(errors);

        if (metrics.ErrorRatePercent is < 0 or > 100)
        {
            errors.Add($"{metricsName}.ErrorRatePercent must be between 0 and 100 (actual: {metrics.ErrorRatePercent:F2}%)");
        }

        if (metrics.P95LatencyMs < 0)
        {
            errors.Add($"{metricsName}.P95LatencyMs must be non-negative (actual: {metrics.P95LatencyMs:F2}ms)");
        }

        if (metrics.P99LatencyMs < 0)
        {
            errors.Add($"{metricsName}.P99LatencyMs must be non-negative (actual: {metrics.P99LatencyMs:F2}ms)");
        }

        if (metrics.P99LatencyMs < metrics.P95LatencyMs)
        {
            errors.Add($"{metricsName}.P99LatencyMs ({metrics.P99LatencyMs:F2}ms) cannot be less than P95LatencyMs ({metrics.P95LatencyMs:F2}ms)");
        }

        if (metrics.RequestCount < 0)
        {
            errors.Add($"{metricsName}.RequestCount must be non-negative (actual: {metrics.RequestCount:N0}) - no traffic observed");
        }

        if (metrics.ErrorCount < 0)
        {
            errors.Add($"{metricsName}.ErrorCount must be non-negative (actual: {metrics.ErrorCount:N0})");
        }

        if (metrics.ErrorCount > metrics.RequestCount)
        {
            errors.Add($"{metricsName}.ErrorCount ({metrics.ErrorCount:N0}) cannot exceed RequestCount ({metrics.RequestCount:N0})");
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="CanaryDeployment"/> is valid.
    /// </summary>
    /// <param name="value">The canary deployment to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this CanaryDeployment value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CanaryDeployment"/> is valid, throwing a <see cref="ValidationFailedException"/> if it is not.
    /// </summary>
    /// <param name="value">The canary deployment to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ValidationFailedException">Thrown when validation fails, listing all problems</exception>
    public static void EnsureValid(this CanaryDeployment value)
    {
        ArgumentNullException.ThrowIfNull(value);

        ValidationGuard.ThrowIfAny(nameof(value), nameof(CanaryDeployment), value.Validate());
    }
}
