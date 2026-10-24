#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Validation;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides validation helpers for <see cref="CanaryDeploymentEngine"/> instances.
/// Validates required fields, ranges, and business rules for canary deployments.
/// </summary>
public static class CanaryDeploymentEngineValidation
{
    /// <summary>
    /// Validates the specified <see cref="CanaryDeploymentEngine"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to validate</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>An immutable list of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static async Task<IReadOnlyList<string>> ValidateAsync(this CanaryDeploymentEngine value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Get active deployments to validate
        var activeDeployments = await value.GetActiveDeploymentsAsync(cancellationToken).ConfigureAwait(false);
        var deploymentHistory = await value.GetDeploymentHistoryAsync(string.Empty, 100, cancellationToken).ConfigureAwait(false);

        // Validate active deployments
        foreach (var deployment in activeDeployments)
        {
            ValidateDeployment(deployment, errors);
        }

        // Validate deployment history for any obvious issues
        foreach (var deployment in deploymentHistory)
        {
            ValidateDeployment(deployment, errors);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a single canary deployment instance.
    /// </summary>
    /// <param name="deployment">The deployment to validate</param>
    /// <param name="errors">List to accumulate validation errors</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is <see langword="null"/></exception>
    private static void ValidateDeployment(CanaryDeployment deployment, List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var prefix = $"Deployment {deployment.Id}";

        ValidationGuard.CheckRequired(errors, deployment.ProjectName, $"{prefix}: ProjectName");
        ValidationGuard.CheckRequired(errors, deployment.StableVersion, $"{prefix}: StableVersion");
        ValidationGuard.CheckRequired(errors, deployment.CanaryVersion, $"{prefix}: CanaryVersion");

        if (!string.IsNullOrWhiteSpace(deployment.StableVersion) && deployment.StableVersion == deployment.CanaryVersion)
            errors.Add($"{prefix}: CanaryVersion must differ from StableVersion.");

        ValidationGuard.CheckDefinedEnum(errors, deployment.TargetEnvironment, $"{prefix}: TargetEnvironment");
        ValidationGuard.CheckDefinedEnum(errors, deployment.Status, $"{prefix}: Status");
        ValidationGuard.CheckDefinedEnum(errors, deployment.Strategy, $"{prefix}: Strategy");

        // Validate rollout plan
        ValidationGuard.CheckNotNull(errors, deployment.RolloutPlan, $"{prefix}: RolloutPlan");

        if (deployment.RolloutPlan is { Count: 0 })
            errors.Add($"{prefix}: RolloutPlan must contain at least one step.");
        else if (deployment.RolloutPlan is { } rolloutPlan)
        {
            foreach (var step in rolloutPlan)
            {
                var stepPrefix = $"{prefix}, Step {step.StepNumber}";

                ValidationGuard.CheckPositive(errors, step.StepNumber, $"{stepPrefix}: StepNumber");
                ValidationGuard.CheckInRange(errors, step.CanaryPercent, 0, 100, $"{stepPrefix}: CanaryPercent");
                ValidationGuard.CheckPositive(errors, step.SoakDuration, $"{stepPrefix}: SoakDuration");
                ValidationGuard.CheckDefinedEnum(errors, step.Status, $"{stepPrefix}: Status");
            }
        }

        // Validate traffic split
        ValidationGuard.CheckInRange(errors, deployment.CurrentSplit.StablePercent, 0, 100, $"{prefix}: CurrentSplit.StablePercent");
        ValidationGuard.CheckInRange(errors, deployment.CurrentSplit.CanaryPercent, 0, 100, $"{prefix}: CurrentSplit.CanaryPercent");

        if (Math.Abs(deployment.CurrentSplit.StablePercent + deployment.CurrentSplit.CanaryPercent - 100) > 0.01)
            errors.Add($"{prefix}: CurrentSplit percentages must sum to 100 (Stable: {deployment.CurrentSplit.StablePercent:F2}%, Canary: {deployment.CurrentSplit.CanaryPercent:F2}% vs expected 100%).");

        // Validate timestamps
        if (deployment.CreatedAt == default)
            errors.Add($"{prefix}: CreatedAt must be set.");

        if (deployment.Status == CanaryStatus.Promoted && deployment.PromotedAt == default)
            errors.Add($"{prefix}: PromotedAt must be set when status is Promoted.");

        if (deployment.Status == CanaryStatus.Aborted && deployment.AbortedAt == default)
            errors.Add($"{prefix}: AbortedAt must be set when status is Aborted.");

        if (deployment.Status == CanaryStatus.Aborted && string.IsNullOrWhiteSpace(deployment.AbortReason))
            errors.Add($"{prefix}: AbortReason must be set when status is Aborted.");

        if (deployment.ActiveStep?.StartedAt > DateTime.UtcNow)
            errors.Add($"{prefix}: ActiveStep.StartedAt cannot be in the future.");

        if (deployment.ActiveStep?.CompletedAt > DateTime.UtcNow)
            errors.Add($"{prefix}: ActiveStep.CompletedAt cannot be in the future.");

        if (deployment.ActiveStep?.CompletedAt < deployment.ActiveStep?.StartedAt)
            errors.Add($"{prefix}: ActiveStep.CompletedAt cannot be before StartedAt.");
    }

    /// <summary>
    /// Determines whether the specified <see cref="CanaryDeploymentEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to check</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static async Task<bool> IsValidAsync(this CanaryDeploymentEngine value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        return (await value.ValidateAsync(cancellationToken).ConfigureAwait(false)).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CanaryDeploymentEngine"/> instance is valid, throwing a <see cref="ValidationFailedException"/>
    /// that lists every problem if validation fails.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to validate</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ValidationFailedException">Thrown when validation fails, containing a list of all problems</exception>
    public static async Task EnsureValidAsync(this CanaryDeploymentEngine value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = await value.ValidateAsync(cancellationToken).ConfigureAwait(false);

        ValidationGuard.ThrowIfAny(nameof(value), nameof(CanaryDeploymentEngine), errors);
    }
}
