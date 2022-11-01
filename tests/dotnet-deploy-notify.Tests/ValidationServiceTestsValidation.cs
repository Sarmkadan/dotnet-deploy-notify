#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Validation helpers for the ValidationServiceTests class.
/// Validates the test class structure and its dependencies.
/// </summary>
public static class ValidationServiceTestsValidation
{
    /// <summary>
    /// Validates a ValidationServiceTests instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private _validationService field
        var validationServiceField = value.GetType().GetField("_validationService",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (validationServiceField?.GetValue(value) is not ValidationService validationService)
        {
            problems.Add("ValidationServiceTests._validationService must not be null.");
        }
        else
        {
            // Validate ValidationService instance
            if (validationService is null)
            {
                problems.Add("ValidationService instance must not be null.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a ValidationServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this ValidationServiceTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a ValidationServiceTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the test instance is invalid.</exception>
    public static void EnsureValid(this ValidationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationServiceTests is invalid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }
}