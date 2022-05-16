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
    /// <summary>
    /// Gets description attribute value from enum
    /// </summary>
    public static string GetDescription<T>(this T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Checks if enum has a specific flag
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        if (typeof(T).GetCustomAttributes(typeof(System.FlagsAttribute), false).Length == 0)
            return false;

        long num = Convert.ToInt64(value);
        long flagNum = Convert.ToInt64(flag);

        return (num & flagNum) == flagNum;
    }

    /// <summary>
    /// Gets all values of an enum
    /// </summary>
    public static List<T> GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Converts enum to human-readable string
    /// </summary>
    public static string ToHumanReadable<T>(this T value) where T : Enum
    {
        var name = value.ToString();
        return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>
    /// Safely parses string to enum
    /// </summary>
    public static T? TryParse<T>(string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : null;
    }

    /// <summary>
    /// Gets random enum value
    /// </summary>
    public static T GetRandomValue<T>() where T : Enum
    {
        var values = GetAllValues<T>();
        return values[new Random().Next(values.Count)];
    }

    /// <summary>
    /// Checks if enum value is in a list of values
    /// </summary>
    public static bool IsIn<T>(this T value, params T[] values) where T : Enum
    {
        return values.Contains(value);
    }
}
