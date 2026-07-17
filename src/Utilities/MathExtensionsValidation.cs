#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation extension methods for <see cref="MathExtensions"/>
/// </summary>
public static class MathExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="MathExtensions"/> class for common issues
    /// </summary>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate() => Array.Empty<string>();

    /// <summary>
    /// Checks if the <see cref="MathExtensions"/> class is valid
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid() => true;

    /// <summary>
    /// Ensures the <see cref="MathExtensions"/> class is valid, throwing if not
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if validation fails</exception>
    public static void EnsureValid() { }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.Clamp{T}"/> method
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to clamp</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateClamp<T>(
        this T value,
        T min,
        T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        var problems = new List<string>();

        if (min.CompareTo(max) > 0)
        {
            problems.Add("Minimum value cannot be greater than maximum value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.IsBetween{T}"/> method
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to check</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateIsBetween<T>(
        this T value,
        T min,
        T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        var problems = new List<string>();

        if (min.CompareTo(max) > 0)
        {
            problems.Add("Minimum value cannot be greater than maximum value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.ToPercentage(int, int)"/> method
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="total"/> is negative</exception>
    public static IReadOnlyList<string> ValidateToPercentage(this int value, int total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.ToPercentage(double, double)"/> method
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="total"/> is negative</exception>
    public static IReadOnlyList<string> ValidateToPercentage(this double value, double total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.RoundTo(decimal, int)"/> method
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static IReadOnlyList<string> ValidateRoundTo(this decimal value, int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.RoundTo(double, int)"/> method
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static IReadOnlyList<string> ValidateRoundTo(this double value, int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.Average(IEnumerable{int})"/> method
    /// </summary>
    /// <param name="values">The values to average</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateAverage(this IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.Median(IEnumerable{int})"/> method
    /// </summary>
    /// <param name="values">The values to calculate median for</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateMedian(this IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.SafeSum(IEnumerable{int}, int)"/> method
    /// </summary>
    /// <param name="values">The values to sum</param>
    /// <param name="defaultValue">The default value to return if validation fails</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateSafeSum(this IEnumerable<int> values, int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.SafeAverage(IEnumerable{double}, double)"/> method
    /// </summary>
    /// <param name="values">The values to average</param>
    /// <param name="defaultValue">The default value to return if validation fails</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateSafeAverage(this IEnumerable<double> values, double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.ToHumanReadableSize(long)"/> method
    /// </summary>
    /// <param name="bytes">The number of bytes to convert</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateToHumanReadableSize(this long bytes) => Array.Empty<string>();

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.ToHumanReadableDuration(int)"/> method
    /// </summary>
    /// <param name="milliseconds">The duration in milliseconds</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="milliseconds"/> is negative</exception>
    public static IReadOnlyList<string> ValidateToHumanReadableDuration(this int milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.ToHumanReadableDuration(TimeSpan)"/> method
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to convert</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateToHumanReadableDuration(this TimeSpan timeSpan) => Array.Empty<string>();

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.CalculateCompoundInterest(decimal, decimal, int)"/> method
    /// </summary>
    /// <param name="principal">The principal amount</param>
    /// <param name="rate">The interest rate per period</param>
    /// <param name="periods">The number of periods</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rate"/> is negative</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="periods"/> is negative</exception>
    public static IReadOnlyList<string> ValidateCalculateCompoundInterest(
        this decimal principal,
        decimal rate,
        int periods)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfNegative(periods);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="MathExtensions.RandomBetween(Random, int, int)"/> method
    /// </summary>
    /// <param name="random">The Random instance</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> ValidateRandomBetween(
        this Random random,
        int min,
        int max)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, min);
        return Array.Empty<string>();
    }

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.Clamp{T}"/> are valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to clamp</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static void EnsureValidClamp<T>(
        this T value,
        T min,
        T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        if (min.CompareTo(max) > 0)
        {
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        }
    }

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.IsBetween{T}"/> are valid, throwing if not
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to check</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static void EnsureValidIsBetween<T>(
        this T value,
        T min,
        T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        if (min.CompareTo(max) > 0)
        {
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
        }
    }

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.ToPercentage(int, int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="total"/> is negative</exception>
    public static void EnsureValidToPercentage(this int value, int total)
        => ArgumentOutOfRangeException.ThrowIfNegative(total);

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.ToPercentage(double, double)"/> are valid, throwing if not
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="total"/> is negative</exception>
    public static void EnsureValidToPercentage(this double value, double total)
        => ArgumentOutOfRangeException.ThrowIfNegative(total);

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.RoundTo(decimal, int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static void EnsureValidRoundTo(this decimal value, int decimalPlaces)
        => ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.RoundTo(double, int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static void EnsureValidRoundTo(this double value, int decimalPlaces)
        => ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.ToHumanReadableDuration(int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="milliseconds">The duration in milliseconds</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="milliseconds"/> is negative</exception>
    public static void EnsureValidToHumanReadableDuration(this int milliseconds)
        => ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.CalculateCompoundInterest(decimal, decimal, int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="principal">The principal amount</param>
    /// <param name="rate">The interest rate per period</param>
    /// <param name="periods">The number of periods</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rate"/> is negative</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="periods"/> is negative</exception>
    public static void EnsureValidCalculateCompoundInterest(
        this decimal principal,
        decimal rate,
        int periods)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfNegative(periods);
    }

    /// <summary>
    /// Ensures parameters for <see cref="MathExtensions.RandomBetween(Random, int, int)"/> are valid, throwing if not
    /// </summary>
    /// <param name="random">The Random instance</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static void EnsureValidRandomBetween(
        this Random random,
        int min,
        int max)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, min);
    }
}