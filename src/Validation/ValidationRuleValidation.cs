#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetDeployNotify.Validation;

/// <summary>
/// Validation helpers for <see cref="ValidationRule{T}"/>.
/// </summary>
public static class ValidationRuleValidation
{
    /// <summary>
    /// Validates a <see cref="ValidationRule{T}"/> and returns a list of human-readable problems.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="value">The <see cref="ValidationRule{T}"/> to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate<T>(this ValidationRule<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.GetErrorMessage()))
        {
            problems.Add("Error message cannot be empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="ValidationRule{T}"/> is valid.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="value">The <see cref="ValidationRule{T}"/> to validate.</param>
    /// <returns><c>true</c> if the <see cref="ValidationRule{T}"/> is valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid<T>(this ValidationRule<T> value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ValidationRule{T}"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="value">The <see cref="ValidationRule{T}"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid<T>(this ValidationRule<T> value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid validation rule: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
