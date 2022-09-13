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
    /// <param name="value">The value to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static void ThrowIfNull(this object? value, string paramName)
    {
        if (value is null)
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
    }

    /// <summary>
    /// Throws ArgumentException if string is null or empty
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty</exception>
    public static void ThrowIfNullOrEmpty(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if collection is null or empty
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="value">The collection to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty</exception>
    public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T>? value, string paramName)
    {
        if (value is null || !value.Any())
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if value is false
    /// </summary>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <param name="message">The exception message</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is false</exception>
    public static void ThrowIfFalse(this bool condition, string paramName, string message)
    {
        if (!condition)
            throw new ArgumentException(message, paramName);
    }

    /// <summary>
    /// Throws ArgumentException if value is less than minimum
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="minimum">The minimum allowed value</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is less than <paramref name="minimum"/></exception>
    public static void ThrowIfLessThan(this int value, int minimum, string paramName)
    {
        if (value < minimum)
            throw new ArgumentException($"{paramName} must be at least {minimum}, but was {value}", paramName);
    }

    /// <summary>
    /// Throws ArgumentException if string length exceeds maximum
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> length exceeds <paramref name="maxLength"/></exception>
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
    /// <param name="value">The URL string to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid HTTP/HTTPS URL</exception>
    public static void ThrowIfInvalidUrl(this string? value, string paramName)
    {
        value.ThrowIfNullOrEmpty(paramName);

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException($"{paramName} is not a valid URL", paramName);

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException($"{paramName} must be an HTTP or HTTPS URL", paramName);
    }

    /// <summary>
    /// Returns the value or throws if null
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The nullable value to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>The non-null value</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static T GetValueOrThrow<T>(this T? value, string paramName) where T : class
    {
        return value ?? throw new ArgumentNullException(paramName);
    }

    /// <summary>
    /// Checks if value is within a range [min, max]
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>True if value is within the range, false otherwise</returns>
    public static bool IsInRange(this int value, int min, int max) => value >= min && value <= max;

    /// <summary>
    /// Checks if string matches a regular expression pattern
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <returns>True if the string matches the pattern, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is not a valid regular expression</exception>
    public static bool MatchesPattern(this string? value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        try
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
        }
        catch (ArgumentException)
        {
            // Re-throw ArgumentException as it indicates an invalid pattern
            throw;
        }
        catch (Exception)
        {
            // All other exceptions (RegexParseException, etc.) indicate invalid pattern
            return false;
        }
    }
}