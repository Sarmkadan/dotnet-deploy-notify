#nullable enable

using System.Globalization;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="IntegrationTests"/> to ensure test data integrity and configuration validity.
/// </summary>
public static class IntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="IntegrationTests"/> instance for common issues.
    /// </summary>
    /// <param name="value">The integration tests instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IntegrationTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // IntegrationTests is a test class with async methods - no data members to validate
        // All validation is structural (compiler ensures the class exists and has the expected methods)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="IntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The integration tests instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this IntegrationTests? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="IntegrationTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The integration tests instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this IntegrationTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"IntegrationTests validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}