#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Core.Models.Validation;

/// <summary>
/// Provides validation extension methods for canary deployment state through CanaryDeploymentExtensions
/// </summary>
public static class CanaryDeploymentExtensionsValidation
{
    /// <summary>
    /// Validates the canary deployment state by exercising the CanaryDeploymentExtensions methods
    /// </summary>
    /// <param name="deployment">The canary deployment instance to validate</param>
    /// <returns>An enumerable of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        var problems = new List<string>();

        // Validate traffic split display
        try
        {
            var trafficDisplay = deployment.GetTrafficSplitDisplay();
            if (string.IsNullOrWhiteSpace(trafficDisplay))
            {
                problems.Add("Traffic split display is null or empty");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to get traffic split display: {ex.Message}");
        }

        // Validate health score
        try
        {
            var healthScore = deployment.CalculateHealthScore();
            if (double.IsNaN(healthScore) || double.IsInfinity(healthScore))
            {
                problems.Add("Health score is not a valid number");
            }
            else if (healthScore < 0 || healthScore > 100)
            {
                problems.Add($"Health score {healthScore} is out of range [0, 100]");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to calculate health score: {ex.Message}");
        }

        // Validate status summary
        try
        {
            var statusSummary = deployment.GetStatusSummary();
            if (string.IsNullOrWhiteSpace(statusSummary))
            {
                problems.Add("Status summary is null or empty");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to get status summary: {ex.Message}");
        }

        // Validate promotion capability
        try
        {
            var canPromote = deployment.CanPromote();
            // No specific validation needed beyond the method call itself
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to check promotion capability: {ex.Message}");
        }

        // Validate next traffic percentage
        try
        {
            var nextTraffic = deployment.GetNextTrafficPercentage();
            if (nextTraffic.HasValue)
            {
                if (nextTraffic.Value < 0 || nextTraffic.Value > 100)
                {
                    problems.Add($"Next traffic percentage {nextTraffic.Value} is out of range [0, 100]");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to get next traffic percentage: {ex.Message}");
        }

        // Validate soak remaining time
        try
        {
            var soakRemaining = deployment.GetCurrentSoakRemaining();
            if (soakRemaining.HasValue)
            {
                if (soakRemaining.Value < TimeSpan.Zero)
                {
                    problems.Add("Soak remaining time is negative");
                }
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Failed to get current soak remaining: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the canary deployment state is valid
    /// </summary>
    /// <param name="deployment">The canary deployment instance to validate</param>
    /// <returns><see langword="true"/> if the deployment state is valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool IsValid(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return deployment.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the canary deployment state is valid, throwing an exception if not
    /// </summary>
    /// <param name="deployment">The canary deployment instance to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">The deployment state is invalid with details of the problems</exception>
    public static void EnsureValid(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        var problems = deployment.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Canary deployment state is invalid:{System.Environment.NewLine}- {string.Join($"{System.Environment.NewLine}- ", problems)}");
        }
    }
}
