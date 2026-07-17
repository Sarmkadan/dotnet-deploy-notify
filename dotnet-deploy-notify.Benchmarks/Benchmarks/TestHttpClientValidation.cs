using System.Net;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="TestHttpClient"/> instances to ensure they are properly configured for benchmarking scenarios.
/// </summary>
public static class TestHttpClientValidation
{
    /// <summary>
    /// Validates that a <see cref="TestHttpClient"/> instance is properly configured and ready for use.
    /// </summary>
    /// <param name="value">The TestHttpClient instance to validate</param>
    /// <returns>A list of human-readable validation problems; empty if the instance is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this TestHttpClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate BaseAddress - should be set for webhook testing
        if (value.BaseAddress == null)
        {
            problems.Add("BaseAddress is null");
        }
        else if (!value.BaseAddress.IsAbsoluteUri)
        {
            problems.Add("BaseAddress is not an absolute URI");
        }
        else if (value.BaseAddress.AbsoluteUri == "https://localhost" || value.BaseAddress.AbsoluteUri == "http://localhost")
        {
            problems.Add("BaseAddress appears to be a default localhost address");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="TestHttpClient"/> instance is valid and ready for use.
    /// </summary>
    /// <param name="value">The TestHttpClient instance to check</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this TestHttpClient? value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="TestHttpClient"/> instance is valid and throws an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The TestHttpClient instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the instance has validation problems</exception>
    public static void EnsureValid(this TestHttpClient? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TestHttpClient is not valid. Problems: {string.Join("; ", problems)}");
        }
    }
}
