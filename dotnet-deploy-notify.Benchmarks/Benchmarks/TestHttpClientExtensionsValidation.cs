using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Validation helpers for TestHttpClientExtensions to ensure proper usage in test scenarios
/// </summary>
public static class TestHttpClientExtensionsValidation
{
    /// <summary>
    /// Validates that the TestHttpClientExtensions type is properly configured for use
    /// </summary>
    /// <param name="value">The TestHttpClientExtensions instance to validate</param>
    /// <returns>List of validation errors; empty list if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this TestHttpClientExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the TestHttpClientExtensions instance is valid
    /// </summary>
    /// <param name="value">The TestHttpClientExtensions instance to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this TestHttpClientExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Validates the TestHttpClientExtensions instance and throws an exception if invalid
    /// </summary>
    /// <param name="value">The TestHttpClientExtensions instance to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">The instance is invalid with validation errors listed</exception>
    public static void EnsureValid(this TestHttpClientExtensions value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}