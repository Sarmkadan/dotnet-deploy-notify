#nullable enable
using System.Globalization;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="DeploymentHistoryServiceTests"/> instances.
/// </summary>
public static class DeploymentHistoryServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DeploymentHistoryServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DeploymentHistoryServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // DeploymentHistoryServiceTests is a test class with no instance state to validate
        // All validation is performed at the service level via the methods being tested
        // This method exists for consistency with the validation helper pattern

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DeploymentHistoryServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DeploymentHistoryServiceTests? value)
    {
        return value?.Validate() is { Count: 0 };
    }

    /// <summary>
    /// Ensures that the specified <see cref="DeploymentHistoryServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this DeploymentHistoryServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "The DeploymentHistoryServiceTests instance is not valid. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}
