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
    /// Validates a <see cref="ValidationRule{T}"/> instance and returns a list of human-readable problems.
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

    /// <summary>
    /// Validates a collection of validation rules and returns a dictionary mapping each rule to its validation problems.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="rules">The collection of <see cref="ValidationRule{T}"/> instances to validate.</param>
    /// <returns>A dictionary where keys are validation rules and values are lists of problems.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rules"/> is <c>null</c>.
    /// </exception>
    public static IReadOnlyDictionary<ValidationRule<T>, IReadOnlyList<string>> ValidateAll<T>(
        this IEnumerable<ValidationRule<T>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var results = new Dictionary<ValidationRule<T>, IReadOnlyList<string>>();

        foreach (var rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);
            results[rule] = rule.Validate();
        }

        return results;
    }

    /// <summary>
    /// Checks if all validation rules in a collection are valid.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="rules">The collection of <see cref="ValidationRule{T}"/> instances to check.</param>
    /// <returns><c>true</c> if all rules are valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> is <c>null</c>.</exception>
    public static bool AllValid<T>(this IEnumerable<ValidationRule<T>> rules)
    {
        return rules?.All(r => r.IsValid()) != false;
    }

    /// <summary>
    /// Gets the first validation problem for a rule, or <c>null</c> if the rule is valid.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="value">The <see cref="ValidationRule{T}"/> to check.</param>
    /// <returns>The first validation problem message, or <c>null</c> if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string? GetFirstProblem<T>(this ValidationRule<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        return problems.Count > 0 ? problems[0] : null;
    }
}