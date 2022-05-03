// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Helper utilities for type operations and conversions
/// </summary>
public static class TypeHelper
{
    /// <summary>
    /// Checks if type is numeric
    /// </summary>
    public static bool IsNumeric(this Type type)
    {
        return type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) ||
               type == typeof(decimal);
    }

    /// <summary>
    /// Checks if type is numeric
    /// </summary>
    public static bool IsNumeric<T>() => typeof(T).IsNumeric();

    /// <summary>
    /// Checks if type is nullable
    /// </summary>
    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Gets underlying type from nullable
    /// </summary>
    public static Type GetUnderlyingType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Checks if type implements interface
    /// </summary>
    public static bool ImplementsInterface<T>(this Type type) where T : class
    {
        return typeof(T).IsAssignableFrom(type);
    }

    /// <summary>
    /// Checks if type is enum
    /// </summary>
    public static bool IsEnum<T>() => typeof(T).IsEnum;

    /// <summary>
    /// Checks if type is collection
    /// </summary>
    public static bool IsCollection(this Type type)
    {
        if (type == typeof(string))
            return false;

        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets generic arguments from type
    /// </summary>
    public static Type[]? GetGenericArguments(this Type type)
    {
        return type.IsGenericType ? type.GetGenericArguments() : null;
    }

    /// <summary>
    /// Checks if type is generic
    /// </summary>
    public static bool IsGeneric(this Type type)
    {
        return type.IsGenericType;
    }

    /// <summary>
    /// Gets method by name and parameter types
    /// </summary>
    public static System.Reflection.MethodInfo? GetMethodBySignature(
        this Type type,
        string methodName,
        params Type[] parameterTypes)
    {
        return type.GetMethod(methodName, parameterTypes);
    }

    /// <summary>
    /// Gets all properties of a type
    /// </summary>
    public static List<System.Reflection.PropertyInfo> GetAllProperties(this Type type)
    {
        return type.GetProperties().ToList();
    }

    /// <summary>
    /// Gets all fields of a type
    /// </summary>
    public static List<System.Reflection.FieldInfo> GetAllFields(this Type type)
    {
        return type.GetFields().ToList();
    }

    /// <summary>
    /// Gets all methods of a type
    /// </summary>
    public static List<System.Reflection.MethodInfo> GetAllMethods(this Type type)
    {
        return type.GetMethods().ToList();
    }

    /// <summary>
    /// Checks if type has parameter-less constructor
    /// </summary>
    public static bool HasParameterlessConstructor(this Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null;
    }

    /// <summary>
    /// Creates instance of type with parameter-less constructor
    /// </summary>
    public static object? CreateInstance(this Type type)
    {
        if (!type.HasParameterlessConstructor())
            return null;

        return Activator.CreateInstance(type);
    }

    /// <summary>
    /// Converts value to target type
    /// </summary>
    public static object? ConvertTo(this object? value, Type targetType)
    {
        if (value == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        try
        {
            if (targetType == typeof(string))
                return value.ToString();

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString() ?? "");

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts value to target type
    /// </summary>
    public static T? ConvertTo<T>(this object? value)
    {
        var result = value.ConvertTo(typeof(T));
        return result != null ? (T)result : default;
    }

    /// <summary>
    /// Finds types in assembly that inherit from base type
    /// </summary>
    public static List<Type> FindTypesThatInherit(this System.Reflection.Assembly assembly, Type baseType)
    {
        return assembly.GetTypes()
            .Where(t => t != baseType && baseType.IsAssignableFrom(t))
            .ToList();
    }

    /// <summary>
    /// Gets attribute of type from type
    /// </summary>
    public static T? GetAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
    }

    /// <summary>
    /// Gets all attributes of type from type
    /// </summary>
    public static List<T> GetAttributes<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes(typeof(T), false).Cast<T>().ToList();
    }

    /// <summary>
    /// Checks if type has attribute
    /// </summary>
    public static bool HasAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetAttribute<T>() != null;
    }
}
