#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Generic extension methods for objects
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Safely casts an object to a type
    /// </summary>
    public static T? SafeCast<T>(this object? obj) where T : class
    {
        return obj as T;
    }

    /// <summary>
    /// Checks if object is null
    /// </summary>
    public static bool IsNull(this object? obj)
    {
        return obj is null;
    }

    /// <summary>
    /// Checks if object is not null
    /// </summary>
    public static bool IsNotNull(this object? obj)
    {
        return obj is not null;
    }

    /// <summary>
    /// Executes an action if object is not null
    /// </summary>
    public static T IfNotNull<T>(this T? obj, Action<T> action) where T : class
    {
        if (obj is not null)
            action(obj);
        return obj;
    }

    /// <summary>
    /// Maps object to another type
    /// </summary>
    public static TResult? Map<T, TResult>(this T? obj, Func<T, TResult> mapper) where T : class
    {
        return obj is not null ? mapper(obj) : default;
    }

    /// <summary>
    /// Creates a shallow copy of an object
    /// </summary>
    public static T? ShallowCopy<T>(this T obj) where T : class
    {
        if (obj is null)
            return null;

        var type = obj.GetType();
        if (type.IsValueType || obj is string)
            return obj;

        var objCopy = Activator.CreateInstance(type);
        if (objCopy is null)
            return null;

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(obj);
                property.SetValue(objCopy, value);
            }
        }

        return (T)objCopy;
    }

    /// <summary>
    /// Gets property value by name
    /// </summary>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        return property?.GetValue(obj);
    }

    /// <summary>
    /// Sets property value by name
    /// </summary>
    public static void SetPropertyValue(this object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
            property.SetValue(obj, value);
    }

    /// <summary>
    /// Converts object to dictionary of properties
    /// </summary>
    public static Dictionary<string, object?> ToDictionary(this object obj)
    {
        var dictionary = new Dictionary<string, object?>();
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                dictionary[property.Name] = property.GetValue(obj);
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Checks if object equals any in the list
    /// </summary>
    public static bool EqualsAny<T>(this T obj, params T[] values)
    {
        return values.Contains(obj);
    }

    /// <summary>
    /// Checks if object is default value
    /// </summary>
    public static bool IsDefault<T>(this T obj)
    {
        return Equals(obj, default(T));
    }

    /// <summary>
    /// Returns object or default if null
    /// </summary>
    public static T GetValueOrDefault<T>(this T? obj, T defaultValue) where T : struct
    {
        return obj ?? defaultValue;
    }

    /// <summary>
    /// Converts object to string safely
    /// </summary>
    public static string ToStringSafe(this object? obj, string defaultValue = "null")
    {
        return obj?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Gets type name without namespace
    /// </summary>
    public static string GetTypeName(this object obj)
    {
        return obj.GetType().Name;
    }

    /// <summary>
    /// Gets fully qualified type name
    /// </summary>
    public static string GetFullTypeName(this object obj)
    {
        return obj.GetType().FullName ?? obj.GetType().Name;
    }

    /// <summary>
    /// Chains multiple operations
    /// </summary>
    public static T Chain<T>(this T obj, Action<T> action) where T : class
    {
        action(obj);
        return obj;
    }

    /// <summary>
    /// Validates object against a condition
    /// </summary>
    public static bool Validate<T>(this T obj, Func<T, bool> validator) where T : class
    {
        return obj is not null && validator(obj);
    }
}
