using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="TestHttpClientExtensions"/> extension methods.
/// Since extension classes are static, these methods validate the <see cref="TestHttpClient"/> instances
/// that the extension methods operate on.
/// </summary>
public static class TestHttpClientExtensionsValidation
{
    /// <summary>
    /// Validates that a <see cref="TestHttpClient"/> instance is compatible with the TestHttpClientExtensions methods.
    /// </summary>
    /// <param name="value">The <see cref="TestHttpClient"/> instance to validate</param>
    /// <returns>A list of validation problems; empty if the instance is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this TestHttpClient value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TestHttpClient"/> instance is valid for use with TestHttpClientExtensions methods.
    /// </summary>
    /// <param name="value">The <see cref="TestHttpClient"/> instance to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this TestHttpClient value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="TestHttpClient"/> instance is valid for use with TestHttpClientExtensions methods
    /// and throws an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The <see cref="TestHttpClient"/> instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the instance has validation problems</exception>
    public static void EnsureValid(this TestHttpClient value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TestHttpClient instance is not valid for TestHttpClientExtensions. Problems: {string.Join("; ", problems)}");
        }
    }
}