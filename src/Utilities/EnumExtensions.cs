#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Extension methods for enum operations
/// </summary>
public static class EnumExtensions
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Gets description attribute value from enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The enum value</param>
    /// <returns>The description attribute value if present, otherwise the enum name</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string GetDescription<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Checks if enum has a specific flag
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The enum value to check</param>
    /// <param name="flag">The flag to check for</param>
    /// <returns>True if the enum has the specified flag; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or flag is null</exception>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(flag);

        if (typeof(T).GetCustomAttributes(typeof(System.FlagsAttribute), false).Length == 0)
            return false;

        long num = Convert.ToInt64(value);
        long flagNum = Convert.ToInt64(flag);

        return (num & flagNum) == flagNum;
    }

    /// <summary>
    /// Gets all values of an enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>A list containing all enum values</returns>
    /// <exception cref="ArgumentException">Thrown when T is not an enum type</exception>
    public static List<T> GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Converts enum to human-readable string
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The enum value to convert</param>
    /// <returns>A human-readable string representation of the enum value</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToHumanReadable<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var name = value.ToString();
        return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>
    /// Safely parses string to enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The string value to parse</param>
    /// <returns>The parsed enum value if successful; otherwise null</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static T? TryParse<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;
    }

    /// <summary>
    /// Gets random enum value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <returns>A randomly selected enum value</returns>
    /// <exception cref="ArgumentException">Thrown when the enum type has no values</exception>
    public static T GetRandomValue<T>() where T : Enum
    {
        var values = GetAllValues<T>();
        if (values.Count == 0)
        {
            throw new ArgumentException($"Enum type {typeof(T).Name} has no defined values.");
        }

        return values[_random.Next(values.Count)];
    }

    /// <summary>
    /// Checks if enum value is in a list of values
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The enum value to check</param>
    /// <param name="values">The values to check against</param>
    /// <returns>True if the enum value is in the list; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsIn<T>(this T value, params T[] values) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(values);

        return values.Contains(value);
    }
}
