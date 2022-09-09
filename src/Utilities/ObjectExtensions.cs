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
    /// <param name="obj">The object to cast</param>
    /// <returns>The cast object or null if the cast fails</returns>
    public static T? SafeCast<T>(this object? obj) where T : class
    {
        return obj as T;
    }

    /// <summary>
    /// Checks if object is null
    /// </summary>
    /// <param name="obj">The object to check</param>
    /// <returns>True if object is null, false otherwise</returns>
    public static bool IsNull(this object? obj)
    {
        return obj is null;
    }

    /// <summary>
    /// Checks if object is not null
    /// </summary>
    /// <param name="obj">The object to check</param>
    /// <returns>True if object is not null, false otherwise</returns>
    public static bool IsNotNull(this object? obj)
    {
        return obj is not null;
    }

    /// <summary>
    /// Executes an action if object is not null
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to check</param>
    /// <param name="action">The action to execute if object is not null</param>
    /// <returns>The original object for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null</exception>
    public static T IfNotNull<T>(this T? obj, Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(action);

        if (obj is not null)
            action(obj);

        return obj!;
    }

    /// <summary>
    /// Maps object to another type
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <typeparam name="TResult">The result type</typeparam>
    /// <param name="obj">The object to map</param>
    /// <param name="mapper">The mapping function</param>
    /// <returns>The mapped result or null if source is null</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapper"/> is null</exception>
    public static TResult? Map<T, TResult>(this T? obj, Func<T, TResult> mapper) where T : class
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return obj is not null ? mapper(obj) : default;
    }

    /// <summary>
    /// Creates a shallow copy of an object
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to copy</param>
    /// <returns>A shallow copy of the object or null if copy fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null</exception>
    public static T? ShallowCopy<T>(this T obj) where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

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
    /// <param name="obj">The source object</param>
    /// <param name="propertyName">The name of the property to get</param>
    /// <returns>The property value or null if property doesn't exist</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null or <paramref name="propertyName"/> is null or empty</exception>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        var property = obj.GetType().GetProperty(propertyName);
        return property?.GetValue(obj);
    }

    /// <summary>
    /// Sets property value by name
    /// </summary>
    /// <param name="obj">The target object</param>
    /// <param name="propertyName">The name of the property to set</param>
    /// <param name="value">The value to set</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null or <paramref name="propertyName"/> is null or empty</exception>
    public static void SetPropertyValue(this object obj, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        var property = obj.GetType().GetProperty(propertyName);
        if (property?.CanWrite == true)
            property.SetValue(obj, value);
    }

    /// <summary>
    /// Converts object to dictionary of properties
    /// </summary>
    /// <param name="obj">The object to convert</param>
    /// <returns>Dictionary of property names and values</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null</exception>
    public static Dictionary<string, object?> ToDictionary(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

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
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to check</param>
    /// <param name="values">Values to compare against</param>
    /// <returns>True if object equals any value, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null</exception>
    public static bool EqualsAny<T>(this T obj, params T[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return values.Contains(obj);
    }

    /// <summary>
    /// Checks if object is default value
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to check</param>
    /// <returns>True if object equals default(T), false otherwise</returns>
    public static bool IsDefault<T>(this T obj)
    {
        return Equals(obj, default(T));
    }

    /// <summary>
    /// Returns object or default if null
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The nullable object</param>
    /// <param name="defaultValue">The default value to return if null</param>
    /// <returns>The object if not null, otherwise the default value</returns>
    public static T GetValueOrDefault<T>(this T? obj, T defaultValue) where T : struct
    {
        return obj ?? defaultValue;
    }

    /// <summary>
    /// Converts object to string safely
    /// </summary>
    /// <param name="obj">The object to convert</param>
    /// <param name="defaultValue">The default string to return if object is null</param>
    /// <returns>String representation of object or default value</returns>
    public static string ToStringSafe(this object? obj, string defaultValue = "null")
    {
        return obj?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Gets type name without namespace
    /// </summary>
    /// <param name="obj">The object to get type name from</param>
    /// <returns>Type name without namespace</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null</exception>
    public static string GetTypeName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.GetType().Name;
    }

    /// <summary>
    /// Gets fully qualified type name
    /// </summary>
    /// <param name="obj">The object to get full type name from</param>
    /// <returns>Fully qualified type name</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null</exception>
    public static string GetFullTypeName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj.GetType().FullName ?? obj.GetType().Name;
    }

    /// <summary>
    /// Chains multiple operations
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to chain operations on</param>
    /// <param name="action">The action to perform on the object</param>
    /// <returns>The original object for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null</exception>
    public static T Chain<T>(this T obj, Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(action);
        action(obj);
        return obj;
    }

    /// <summary>
    /// Validates object against a condition
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="obj">The object to validate</param>
    /// <param name="validator">The validation function</param>
    /// <returns>True if object is not null and validation passes, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is null</exception>
    public static bool Validate<T>(this T obj, Func<T, bool> validator) where T : class
    {
        ArgumentNullException.ThrowIfNull(validator);
        return obj is not null && validator(obj);
    }
}