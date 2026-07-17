#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotNetDeployNotify.Results;

/// <summary>
/// Provides validation helpers for Result types
/// </summary>
public static class ResultValidation
{
    /// <summary>
    /// Validates a Result instance and returns a list of validation problems
    /// </summary>
    /// <param name="value">The Result to validate</param>
    /// <returns>List of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this Result value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.IsSuccess)
        {
            // For successful results, Error should be null and Errors should be empty
            if (value.Error is not null)
            {
                problems.Add("Successful Result should have null Error, but it has a value");
            }

            if (value.Errors.Count > 0)
            {
                problems.Add("Successful Result should have empty Errors collection, but it contains items");
            }
        }
        else
        {
            // For failed results, Error should not be null/empty and Errors should not be empty
            if (string.IsNullOrEmpty(value.Error))
            {
                problems.Add("Failed Result should have non-null, non-empty Error, but it is null or empty");
            }

            if (value.Errors.Count == 0)
            {
                problems.Add("Failed Result should have non-empty Errors collection, but it is empty");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a Result&lt;T&gt; instance and returns a list of validation problems
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The Result&lt;T&gt; to validate</param>
    /// <returns>List of human-readable validation problems, or empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate<T>(this Result<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base Result properties first
        problems.AddRange(((Result)value).Validate());

        if (value.IsSuccess)
        {
            // For successful results with value, Value should not be default
            if (EqualityComparer<T>.Default.Equals(value.Value, default))
            {
                problems.Add("Successful Result<T> should have non-default Value, but it is default");
            }
        }
        else
        {
            // For failed results, Value should be default
            if (!EqualityComparer<T>.Default.Equals(value.Value, default))
            {
                problems.Add("Failed Result<T> should have default Value, but it has a value");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a Result instance is valid
    /// </summary>
    /// <param name="value">The Result to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this Result value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if a Result&lt;T&gt; instance is valid
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The Result&lt;T&gt; to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid<T>([NotNullWhen(true)] this Result<T>? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a Result instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The Result to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems</exception>
    public static void EnsureValid(this Result value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            var message = string.Join("\n- ", problems);
            throw new ArgumentException("Result validation failed:\n- " + message);
        }
    }

    /// <summary>
    /// Ensures that a Result&lt;T&gt; instance is valid, throwing ArgumentException if not
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The Result&lt;T&gt; to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems</exception>
    public static void EnsureValid<T>(this Result<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            var message = string.Join("\n- ", problems);
            throw new ArgumentException("Result<T> validation failed:\n- " + message);
        }
    }
}