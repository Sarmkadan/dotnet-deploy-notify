#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Provides validation helpers for <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> instances.
/// </summary>
public static class ServiceExtensionsJsonExtensionsValidation
{
    /// <summary>
    /// Validates the provided <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> instance.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns>A list of human-readable validation errors, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(metadata.Type))
        {
            errors.Add("Type must not be null or empty.");
        }

        if (string.IsNullOrEmpty(metadata.Namespace))
        {
            errors.Add("Namespace must not be null or empty.");
        }

        if (string.IsNullOrEmpty(metadata.Assembly))
        {
            errors.Add("Assembly must not be null or empty.");
        }

        if (metadata.Methods is null)
        {
            errors.Add("Methods must not be null.");
        }
        else if (metadata.Methods.Length == 0)
        {
            errors.Add("Methods must not be empty.");
        }
        else if (metadata.Methods.Any(method => string.IsNullOrEmpty(method)))
        {
            errors.Add("Methods must not contain null or empty strings.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the provided <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is valid.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata metadata)
    {
        return !Validate(metadata).Any();
    }

    /// <summary>
    /// Ensures the provided <see cref="ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata"/> is valid.
    /// </summary>
    /// <param name="metadata">The metadata to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="metadata"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all error messages joined with semicolons.</exception>
    public static void EnsureValid(this ServiceExtensionsJsonExtensions.ServiceExtensionsMetadata metadata)
    {
        var errors = Validate(metadata);
        if (errors.Any())
        {
            throw new ArgumentException(string.Join("; ", errors), nameof(metadata));
        }
    }
}
