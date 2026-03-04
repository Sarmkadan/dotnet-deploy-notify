#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Guard extension methods for argument validation and null checking
/// </summary>
public static class GuardExtensions
{
    /// <summary>
    /// Throws ArgumentNullException if value is null
    /// </summary>
    public static void ThrowIfNull(this object? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
    }

    /// <summary>
    /// Throws ArgumentException if string is null or empty
    /// </summary>
    public static void ThrowIfNullOrEmpty(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if collection is null or empty
    /// </summary>
    public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T>? value, string paramName)
    {
        if (value is null || !value.Any())
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if value is false
    /// </summary>
    public static void ThrowIfFalse(this bool condition, string paramName, string message)
    {
        if (!condition)
            throw new ArgumentException(message, paramName);
    }

    /// <summary>
    /// Throws ArgumentException if value is less than minimum
    /// </summary>
    public static void ThrowIfLessThan(this int value, int minimum, string paramName)
    {
        if (value < minimum)
            throw new ArgumentException($"{paramName} must be at least {minimum}, but was {value}", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if string length exceeds maximum
    /// </summary>
    public static void ThrowIfLongerThan(this string? value, int maxLength, string paramName)
    {
        if (value?.Length > maxLength)
            throw new ArgumentException(
                $"{paramName} cannot be longer than {maxLength} characters, but was {value.Length}",
                paramName);
    }

    /// <summary>
    /// Throws ArgumentException if URL is invalid
    /// </summary>
    public static void ThrowIfInvalidUrl(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException($"{paramName} is not a valid URL", paramName);

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException($"{paramName} must be an HTTP or HTTPS URL", paramName);
    }

    /// <summary>
    /// Returns the value or throws if null
    /// </summary>
    public static T GetValueOrThrow<T>(this T? value, string paramName) where T : class
    {
        return value ?? throw new ArgumentNullException(paramName);
    }

    /// <summary>
    /// Checks if value is within a range
    /// </summary>
    public static bool IsInRange(this int value, int min, int max) => value >= min && value <= max;

    /// <summary>
    /// Checks if string matches a pattern
    /// </summary>
    public static bool MatchesPattern(this string? value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
        }
        catch
        {
            return false;
        }
    }
}
