#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

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
        if (string.IsNullOrWhiteSpace(value.ProjectName))
            errors.Add("ProjectName must be a non-empty string");

        if (string.IsNullOrWhiteSpace(value.StableVersion))
            errors.Add("StableVersion must be a non-empty string");

        if (string.IsNullOrWhiteSpace(value.CanaryVersion))
            errors.Add("CanaryVersion must be a non-empty string");

        if (value.TargetEnvironment == default)
            errors.Add("TargetEnvironment must be specified");

        // Validate string properties that can be empty but not null
        if (value.InitiatedBy is null)
            errors.Add("InitiatedBy must not be null");

        if (value.BranchName is null)
            errors.Add("BranchName must not be null");

        if (value.CommitHash is null)
            errors.Add("CommitHash must not be null");

        if (value.BuildUrl is null)
            errors.Add("BuildUrl must not be null");

        // Validate TrafficSplit ranges
        if (value.CurrentSplit.StablePercent < 0 || value.CurrentSplit.StablePercent > 100)
            errors.Add("CurrentSplit.StablePercent must be between 0 and 100");

        if (value.CurrentSplit.CanaryPercent < 0 || value.CurrentSplit.CanaryPercent > 100)
            errors.Add("CurrentSplit.CanaryPercent must be between 0 and 100");

        if (Math.Abs(value.CurrentSplit.StablePercent + value.CurrentSplit.CanaryPercent - 100) > 0.01)
            errors.Add("CurrentSplit must sum to 100% (StablePercent + CanaryPercent = 100)");

        // Validate RolloutPlan
        if (value.RolloutPlan is null)
            errors.Add("RolloutPlan must not be null");
        else if (value.RolloutPlan.Count == 0)
            errors.Add("RolloutPlan must contain at least one step");
        else
        {
            // Validate each rollout step
            foreach (var step in value.RolloutPlan)
            {
                if (step is null)
                {
                    errors.Add("RolloutPlan contains a null step");
                    continue;
                }

                if (step.StepNumber < 1)
                    errors.Add($"RolloutPlan step {step.StepNumber} has invalid StepNumber (must be >= 1)");

                if (step.CanaryPercent < 0 || step.CanaryPercent > 100)
                    errors.Add($"RolloutPlan step {step.StepNumber} has invalid CanaryPercent ({step.CanaryPercent}) - must be between 0 and 100");

                if (step.SoakDuration < TimeSpan.Zero)
                    errors.Add($"RolloutPlan step {step.StepNumber} has negative SoakDuration");
            }
        }

        // Validate metrics
        if (value.StableMetrics is null)
            errors.Add("StableMetrics must not be null");
        else
            ValidateMetrics(value.StableMetrics, "StableMetrics", errors);

        if (value.CanaryMetrics is null)
            errors.Add("CanaryMetrics must not be null");
        else
            ValidateMetrics(value.CanaryMetrics, "CanaryMetrics", errors);

        // Validate CreatedAt (must not be default)
        if (value.CreatedAt == default)
            errors.Add("CreatedAt must be set to a non-default DateTime");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("CreatedAt cannot be in the future");

        // Validate PromotedAt
        if (value.PromotedAt.HasValue)
        {
            if (value.PromotedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("PromotedAt cannot be in the future");

            if (value.PromotedAt.Value < value.CreatedAt)
                errors.Add("PromotedAt cannot be before CreatedAt");
        }

        // Validate AbortedAt
        if (value.AbortedAt.HasValue)
        {
            if (value.AbortedAt.Value > DateTime.UtcNow.AddMinutes(5))
                errors.Add("AbortedAt cannot be in the future");

            if (value.AbortedAt.Value < value.CreatedAt)
                errors.Add("AbortedAt cannot be before CreatedAt");
        }

        // Validate Status consistency with timestamps
        if (value.Status == CanaryStatus.Promoted && !value.PromotedAt.HasValue)
            errors.Add("Status is Promoted but PromotedAt is not set");

        if (value.Status == CanaryStatus.Aborted && string.IsNullOrWhiteSpace(value.AbortReason))
            errors.Add("Status is Aborted but AbortReason is not provided");

        // Validate NotificationChannels
        if (value.NotificationChannels is null)
            errors.Add("NotificationChannels must not be null");
        else
        {
            foreach (var channel in value.NotificationChannels)
            {
                if (channel == default)
                    errors.Add("NotificationChannels contains a default/invalid channel");
            }
        }

        if (value.Priority == default)
            errors.Add("Priority must be specified");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the health metrics for a canary deployment
    /// </summary>
    /// <param name="metrics">The metrics to validate</param>
    /// <param name="metricsName">Name of the metrics property for error messages</param>
    /// <param name="errors">List to accumulate validation errors</param>
    private static void ValidateMetrics(CanaryMetrics metrics, string metricsName, List<string> errors)
    {
        if (metrics.ErrorRatePercent < 0 || metrics.ErrorRatePercent > 100)
            errors.Add($"{metricsName}.ErrorRatePercent must be between 0 and 100 (actual: {metrics.ErrorRatePercent:F2}%)");

        if (metrics.P95LatencyMs < 0)
            errors.Add($"{metricsName}.P95LatencyMs must be non-negative (actual: {metrics.P95LatencyMs:F2}ms)");

        if (metrics.P99LatencyMs < 0)
            errors.Add($"{metricsName}.P99LatencyMs must be non-negative (actual: {metrics.P99LatencyMs:F2}ms)");

        if (metrics.P99LatencyMs < metrics.P95LatencyMs)
            errors.Add($"{metricsName}.P99LatencyMs ({metrics.P99LatencyMs:F2}ms) cannot be less than P95LatencyMs ({metrics.P95LatencyMs:F2}ms)");

        if (metrics.RequestCount < 0)
            errors.Add($"{metricsName}.RequestCount must be non-negative (actual: {metrics.RequestCount:N0}) - no traffic observed");

        if (metrics.ErrorCount < 0)
            errors.Add($"{metricsName}.ErrorCount must be non-negative (actual: {metrics.ErrorCount:N0})");

        if (metrics.ErrorCount > metrics.RequestCount)
            errors.Add($"{metricsName}.ErrorCount ({metrics.ErrorCount:N0}) cannot exceed RequestCount ({metrics.RequestCount:N0})");
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
    /// Ensures that the specified <see cref="CanaryDeployment"/> is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The canary deployment to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, listing all problems</exception>
    public static void EnsureValid(this CanaryDeployment value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CanaryDeployment validation failed:{System.Environment.NewLine}- {
                    string.Join($"{System.Environment.NewLine}- ", errors)
                }");
        }
    }
}