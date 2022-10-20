#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetDeployNotify.Caching;

/// <summary>
/// Provides validation helpers for cache-related types.
/// </summary>
public static class CacheEntryValidation
{
    /// <summary>
    /// Validates the provided <see cref="CacheStatistics"/> instance.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CacheStatistics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate TotalItems: should be non-negative
        if (value.TotalItems < 0)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CacheStatistics.TotalItems must be non-negative, but was {0}.",
                value.TotalItems));
        }

        // Validate Hits: should be non-negative
        if (value.Hits < 0)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CacheStatistics.Hits must be non-negative, but was {0}.",
                value.Hits));
        }

        // Validate Misses: should be non-negative
        if (value.Misses < 0)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CacheStatistics.Misses must be non-negative, but was {0}.",
                value.Misses));
        }

        // Validate LastCleanup: should not be default(DateTime)
        // Default DateTime is DateTime.MinValue which is 0001-01-01, clearly invalid
        if (value.LastCleanup == default)
        {
            problems.Add("CacheStatistics.LastCleanup must be set to a valid DateTime, but was default(DateTime).");
        }
        // Also check if it's in the future (invalid state)
        else if (value.LastCleanup > DateTime.UtcNow)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CacheStatistics.LastCleanup must not be in the future, but was {0}.",
                value.LastCleanup));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="CacheStatistics"/> instance is valid.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CacheStatistics value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="CacheStatistics"/> instance is valid.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the statistics contain validation problems.</exception>
    public static void EnsureValid(this CacheStatistics value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "CacheStatistics validation failed:{0}{1}",
                    "\n",
                    string.Join("\n", problems)),
                nameof(value));
        }
    }
}
