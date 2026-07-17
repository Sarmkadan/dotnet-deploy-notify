#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation helpers for DateTime values
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates a DateTime value for common issues
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        var problems = new List<string>();

        // Check for default DateTime (uninitialized)
        if (dateTime == default)
            problems.Add("DateTime cannot be default (uninitialized).");

        // Check for MinValue which is often used as a sentinel
        if (dateTime == DateTime.MinValue)
            problems.Add("DateTime cannot be DateTime.MinValue.");

        // Check for MaxValue which is often used as a sentinel
        if (dateTime == DateTime.MaxValue)
            problems.Add("DateTime cannot be DateTime.MaxValue.");

        // Check if the date is in the future beyond reasonable bounds
        // (more than 100 years in the future is likely an error)
        if (dateTime > DateTime.UtcNow.AddYears(100))
            problems.Add("DateTime is unreasonably far in the future (more than 100 years).");

        // Check if the date is in the past beyond reasonable bounds
        // (more than 100 years in the past is likely an error)
        if (dateTime < DateTime.UtcNow.AddYears(-100))
            problems.Add("DateTime is unreasonably far in the past (more than 100 years).");

        // Check for invalid DateTime values (those that would throw exceptions)
        try
        {
            // Try to format the date to catch any invalid values
            _ = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            problems.Add($"DateTime formatting failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DateTime value is valid
    /// </summary>
    /// <param name="dateTime">The DateTime value to check</param>
    /// <returns>true if the DateTime is valid; otherwise, false</returns>
    public static bool IsValid(this DateTime dateTime) => dateTime.Validate().Count == 0;

    /// <summary>
    /// Ensures that a DateTime value is valid, throwing an exception if not
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <returns>The validated DateTime</returns>
    /// <exception cref="ArgumentException">Thrown when dateTime is invalid with detailed error message</exception>
    public static DateTime EnsureValid(this DateTime dateTime)
    {
        var problems = dateTime.Validate();

        if (problems.Count > 0)
        {
            var errorMessage = $"DateTime validation failed:{System.Environment.NewLine}- {string.Join($"{System.Environment.NewLine}- ", problems)}";
            throw new ArgumentException(errorMessage);
        }

        return dateTime;
    }
}