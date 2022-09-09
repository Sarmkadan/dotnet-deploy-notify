#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Extension methods for mathematical operations
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Clamps a value between min and max
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to clamp</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>The clamped value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));

        return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }

    /// <summary>
    /// Checks if a value is between min and max (inclusive)
    /// </summary>
    /// <typeparam name="T">The type that implements <see cref="IComparable{T}"/></typeparam>
    /// <param name="value">The value to check</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns><see langword="true"/> if the value is between min and max (inclusive); otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="min"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="max"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(min);
        ArgumentNullException.ThrowIfNull(max);

        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));

        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Calculates percentage
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <returns>The percentage value (0-100)</returns>
    /// <exception cref="ArgumentException"><paramref name="total"/> is negative</exception>
    public static double ToPercentage(this int value, int total)
    {
        if (total < 0)
            throw new ArgumentException("Total cannot be negative.", nameof(total));

        return total == 0 ? 0 : (double)value / total * 100;
    }

    /// <summary>
    /// Calculates percentage
    /// </summary>
    /// <param name="value">The numerator value</param>
    /// <param name="total">The denominator value</param>
    /// <returns>The percentage value (0-100)</returns>
    /// <exception cref="ArgumentException"><paramref name="total"/> is negative</exception>
    public static double ToPercentage(this double value, double total)
    {
        if (total < 0)
            throw new ArgumentException("Total cannot be negative.", nameof(total));

        return total == 0 ? 0 : value / total * 100;
    }

    /// <summary>
    /// Rounds to specified decimal places
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <returns>The rounded value</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Rounds to specified decimal places
    /// </summary>
    /// <param name="value">The value to round</param>
    /// <param name="decimalPlaces">The number of decimal places to round to</param>
    /// <returns>The rounded value</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="decimalPlaces"/> is negative</exception>
    public static double RoundTo(this double value, int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Calculates average of values
    /// </summary>
    /// <param name="values">The values to average</param>
    /// <returns>The average of the values, or 0 if the collection is empty</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static double Average(this IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var list = values.ToList();
        return list.Count == 0 ? 0 : list.Average();
    }

    /// <summary>
    /// Calculates median of values
    /// </summary>
    /// <param name="values">The values to calculate median for</param>
    /// <returns>The median value, or 0 if the collection is empty</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static double Median(this IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var sorted = values.OrderBy(x => x).ToList();
        if (sorted.Count == 0) return 0;
        if (sorted.Count % 2 == 1) return sorted[sorted.Count / 2];
        return (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2.0;
    }

    /// <summary>
    /// Calculates sum with default for empty collection
    /// </summary>
    /// <param name="values">The values to sum</param>
    /// <param name="defaultValue">The value to return if the collection is empty or an error occurs</param>
    /// <returns>The sum of the values, or <paramref name="defaultValue"/> if the collection is empty or an error occurs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static int SafeSum(this IEnumerable<int> values, int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(values);
        try
        {
            return values.Sum();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Calculates average with default for empty collection
    /// </summary>
    /// <param name="values">The values to average</param>
    /// <param name="defaultValue">The value to return if the collection is empty or an error occurs</param>
    /// <returns>The average of the values, or <paramref name="defaultValue"/> if the collection is empty or an error occurs</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/></exception>
    public static double SafeAverage(this IEnumerable<double> values, double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(values);
        try
        {
            return values.Average();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts bytes to human-readable format
    /// </summary>
    /// <param name="bytes">The number of bytes to convert</param>
    /// <returns>A string representing the human-readable size</returns>
    public static string ToHumanReadableSize(this long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Converts milliseconds to human-readable format
    /// </summary>
    /// <param name="milliseconds">The duration in milliseconds</param>
    /// <returns>A human-readable duration string</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="milliseconds"/> is negative</exception>
    public static string ToHumanReadableDuration(this int milliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(milliseconds);
        var ts = TimeSpan.FromMilliseconds(milliseconds);

        return ts.TotalSeconds < 1 ? $"{milliseconds}ms" :
               ts.TotalMinutes < 1 ? $"{ts.TotalSeconds:F2}s" :
               ts.TotalHours < 1 ? $"{ts.TotalMinutes:F2}m" :
               $"{ts.TotalHours:F2}h";
    }

    /// <summary>
    /// Converts TimeSpan to human-readable format
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to convert</param>
    /// <returns>A human-readable duration string</returns>
    public static string ToHumanReadableDuration(this TimeSpan timeSpan)
    {
        return timeSpan.TotalSeconds < 1 ? $"{(int)timeSpan.TotalMilliseconds}ms" :
               timeSpan.TotalMinutes < 1 ? $"{timeSpan.TotalSeconds:F2}s" :
               timeSpan.TotalHours < 1 ? $"{timeSpan.TotalMinutes:F2}m" :
               timeSpan.TotalDays < 1 ? $"{timeSpan.TotalHours:F2}h" :
               $"{timeSpan.TotalDays:F2}d";
    }

    /// <summary>
    /// Calculates compound interest
    /// </summary>
    /// <param name="principal">The principal amount</param>
    /// <param name="rate">The interest rate per period</param>
    /// <param name="periods">The number of periods</param>
    /// <returns>The compounded amount</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rate"/> is negative</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="periods"/> is negative</exception>
    public static decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rate);
        ArgumentOutOfRangeException.ThrowIfNegative(periods);
        return principal * (decimal)Math.Pow((double)(1 + rate), periods);
    }

    /// <summary>
    /// Generates random number between min and max
    /// </summary>
    /// <param name="random">The Random instance</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>A random number between min and max</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="min"/> is greater than <paramref name="max"/></exception>
    public static int RandomBetween(this Random random, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, min);
        return random.Next(min, max + 1);
    }
}