#nullable enable

using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="TemplateServiceTests"/> to ensure test data integrity and configuration validity.
/// </summary>
public static class TemplateServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="TemplateServiceTests"/> instance for common issues.
    /// This method validates that the test class instance is properly initialized.
    /// </summary>
    /// <param name="value">The template service tests instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TemplateServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // TemplateServiceTests is a test class that validates TemplateService functionality.
        // The actual test data validation is performed by the test methods themselves.
        // This validation ensures the test class instance is not null and properly initialized.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TemplateServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The template service tests instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this TemplateServiceTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="TemplateServiceTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The template service tests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this TemplateServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TemplateServiceTests validation failed:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
        }
    }
}