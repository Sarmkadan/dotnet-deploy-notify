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
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }

    /// <summary>
    /// Checks if a value is between min and max (inclusive)
    /// </summary>
    public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
    {
        return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
    }

    /// <summary>
    /// Calculates percentage
    /// </summary>
    public static double ToPercentage(this int value, int total)
    {
        return total == 0 ? 0 : (double)value / total * 100;
    }

    /// <summary>
    /// Calculates percentage
    /// </summary>
    public static double ToPercentage(this double value, double total)
    {
        return total == 0 ? 0 : value / total * 100;
    }

    /// <summary>
    /// Rounds to specified decimal places
    /// </summary>
    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Rounds to specified decimal places
    /// </summary>
    public static double RoundTo(this double value, int decimalPlaces)
    {
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Calculates average of values
    /// </summary>
    public static double Average(this IEnumerable<int> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0 : list.Average();
    }

    /// <summary>
    /// Calculates median of values
    /// </summary>
    public static double Median(this IEnumerable<int> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        if (sorted.Count == 0) return 0;
        if (sorted.Count % 2 == 1) return sorted[sorted.Count / 2];
        return (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2.0;
    }

    /// <summary>
    /// Calculates sum with default for empty collection
    /// </summary>
    public static int SafeSum(this IEnumerable<int> values, int defaultValue = 0)
    {
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
    public static double SafeAverage(this IEnumerable<double> values, double defaultValue = 0)
    {
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
    public static string ToHumanReadableDuration(this int milliseconds)
    {
        var ts = TimeSpan.FromMilliseconds(milliseconds);

        if (ts.TotalSeconds < 1)
            return $"{milliseconds}ms";
        if (ts.TotalMinutes < 1)
            return $"{ts.TotalSeconds:F2}s";
        if (ts.TotalHours < 1)
            return $"{ts.TotalMinutes:F2}m";
        return $"{ts.TotalHours:F2}h";
    }

    /// <summary>
    /// Converts TimeSpan to human-readable format
    /// </summary>
    public static string ToHumanReadableDuration(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 1)
            return $"{(int)timeSpan.TotalMilliseconds}ms";
        if (timeSpan.TotalMinutes < 1)
            return $"{timeSpan.TotalSeconds:F2}s";
        if (timeSpan.TotalHours < 1)
            return $"{timeSpan.TotalMinutes:F2}m";
        if (timeSpan.TotalDays < 1)
            return $"{timeSpan.TotalHours:F2}h";
        return $"{timeSpan.TotalDays:F2}d";
    }

    /// <summary>
    /// Calculates compound interest
    /// </summary>
    public static decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods)
    {
        return principal * (decimal)Math.Pow((double)(1 + rate), periods);
    }

    /// <summary>
    /// Generates random number between min and max
    /// </summary>
    public static int RandomBetween(this Random random, int min, int max)
    {
        return random.Next(min, max + 1);
    }
}
