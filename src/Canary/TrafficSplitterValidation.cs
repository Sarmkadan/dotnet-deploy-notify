#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using System.Globalization;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides validation helpers for <see cref="TrafficSplit"/> instances to ensure traffic split values
/// are within valid ranges and meet business invariants before use in canary deployments.
/// </summary>
public static class TrafficSplitterValidation
{
    /// <summary>
    /// Validates that a <see cref="TrafficSplit"/> instance contains only valid traffic percentages.
    /// </summary>
    /// <param name="value">The traffic split to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable validation errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this TrafficSplit value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate CanaryPercent
        if (value.CanaryPercent < 0 || value.CanaryPercent > 100)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryPercent must be between 0 and 100 (inclusive), but was {0:F2}.",
                value.CanaryPercent));
        }

        // Validate StablePercent
        if (value.StablePercent < 0 || value.StablePercent > 100)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "StablePercent must be between 0 and 100 (inclusive), but was {0:F2}.",
                value.StablePercent));
        }

        // Validate that percentages sum to 100 (within floating-point tolerance)
        const double tolerance = 0.01;
        if (Math.Abs(value.CanaryPercent + value.StablePercent - 100) > tolerance)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryPercent ({0:F2}) and StablePercent ({1:F2}) must sum to 100 (within tolerance {2}), but sum was {3:F2}.",
                value.CanaryPercent,
                value.StablePercent,
                tolerance,
                value.CanaryPercent + value.StablePercent));
        }

        // Validate that both percentages are non-negative
        if (value.CanaryPercent < 0)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryPercent cannot be negative, but was {0:F2}.",
                value.CanaryPercent));
        }

        if (value.StablePercent < 0)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "StablePercent cannot be negative, but was {0:F2}.",
                value.StablePercent));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TrafficSplit"/> instance is valid.
    /// </summary>
    /// <param name="value">The traffic split to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this TrafficSplit value)
    {
        ArgumentNullException.ThrowIfNull(value);

        const double tolerance = 0.01;
        return value.CanaryPercent >= 0
            && value.CanaryPercent <= 100
            && value.StablePercent >= 0
            && value.StablePercent <= 100
            && Math.Abs(value.CanaryPercent + value.StablePercent - 100) <= tolerance;
    }

    /// <summary>
    /// Ensures that the specified <see cref="TrafficSplit"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation errors if it is not.
    /// </summary>
    /// <param name="value">The traffic split to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is invalid; the exception message lists all validation errors.</exception>
    public static void EnsureValid(this TrafficSplit value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            string.Join(" ", errors),
            nameof(value));
    }
}
