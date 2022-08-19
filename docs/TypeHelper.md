# TypeHelper

A static helper class that encapsulates common reflection‑based type checks and operations used throughout the `dotnet-deploy-notify` project. It provides a concise API for determining numeric, nullable, collection, enum, and interface relationships, as well as for instantiating types, converting values, and retrieving members or attributes.

## API

### IsNumeric(Type type)
- **Purpose**: Determines whether the supplied type represents a numeric value (integral or floating‑point).
- **Parameters**: `type` – The `System.Type` to evaluate.
- **Return value**: `true` if `type` is a primitive numeric type (`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`) or a nullable version thereof; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### IsNumeric<T>()
- **Purpose**: Determines whether the generic type argument `T` represents a numeric value.
- **Parameters**: None.
- **Return value**: `true` if `typeof(T)` is a primitive numeric type or its nullable counterpart; otherwise `false`.
- **Exceptions**: None.

### IsNullable(Type type)
- **Purpose**: Checks whether the supplied type is a nullable value type (`Nullable<T>`).
- **Parameters**: `type` – The `System.Type` to evaluate.
- **Return value**: `true` if `type` is a generic instantiation of `System.Nullable<>`; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### GetUnderlyingType(Type type)
- **Purpose**: Returns the underlying type of a nullable type; for non‑nullable types returns `null`.
- **Parameters**: `type` – The `System.Type` to inspect.
- **Return value**: The underlying type if `type` is `Nullable<T>`; otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### ImplementsInterface<T>(Type type)
- **Purpose**: Determines whether the supplied type implements the interface specified by the generic argument `T`.
- **Parameters**: `type` – The `System.Type` to test.
- **Return value**: `true` if `type` implements `T`; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### IsEnum<T>()
- **Purpose**: Determines whether the generic type argument `T` is an enumeration.
- **Parameters**: None.
- **Return value**: `true` if `typeof(T)` is an `System.Enum`; otherwise `false`.
- **Exceptions**: None.

### IsCollection(Type type)
- **Purpose**: Determines whether the supplied type represents a collection (i.e., implements `System.Collections.IEnumerable`).
- **Parameters**: `type` – The `System.Type` to evaluate.
- **Return value**: `true` if `type` implements `IEnumerable`; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### GetGenericArguments(Type type)
- **Purpose**: Retrieves the generic type arguments of a constructed generic type.
- **Parameters**: `type` – The `System.Type` to inspect.
- **Return value**: An array of `System.Type` objects representing the generic arguments, or `null` if `type` is not a generic type.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### IsGeneric(Type type)
- **Purpose**: Checks whether the supplied type is a generic type definition or a constructed generic type.
- **Parameters**: `type` – The `System.Type` to evaluate.
- **Return value**: `true` if `type.IsGenericType` is `true`; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### GetMethodBySignature(Type type, string name, Type[] parameterTypes)
- **Purpose**: Looks for an instance or static method with the exact name and parameter types.
- **Parameters**: 
  - `type` – The `System.Type` containing the method.
  - `name` – The method name to match.
  - `parameterTypes` – An array of `System.Type` objects representing the method’s parameter types in order.
- **Return value**: A `System.Reflection.MethodInfo` if a matching method is found; otherwise `null`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `type`, `name`, or `parameterTypes` is `null`.
  - Throws `ArgumentException` if `parameterTypes` contains a `null` element.

### GetAllProperties(Type type)
- **Purpose**: Returns all public properties declared on the type and its base types.
- **Parameters**: `type` – The `System.Type` to inspect.
- **Return value**: A `List<System.Reflection.PropertyInfo>` containing the properties; empty list if none are found.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### GetAllFields(Type type)
- **Purpose**: Returns all public fields declared on the type and its base types.
- **Parameters**: `type` – The `System.Type` to inspect.
- **Return value**: A `List<System.Reflection.FieldInfo>` containing the fields; empty list if none are found.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### GetAllMethods(Type type)
- **Purpose**: Returns all public methods declared on the type and its base types.
- **Parameters**: `type` – The `System.Type` to inspect.
- **Return value**: A `List<System.Reflection.MethodInfo>` containing the methods; empty list if none are found.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### HasParameterlessConstructor(Type type)
- **Purpose**: Determines whether the type has a public constructor that takes no parameters.
- **Parameters**: `type` – The `System.Type` to evaluate.
- **Return value**: `true` if a public parameterless constructor exists; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `type` is `null`.

### CreateInstance(Type type)
- **Purpose**: Attempts to create an instance of the type using its public parameterless constructor.
- **Parameters**: `type` – The `System.Type` to instantiate.
- **Return value**: A new object instance if the constructor exists; otherwise `null`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `type` is `null`.
  - May propagate `TargetInvocationException` if the constructor throws; the helper does not catch it.

### ConvertTo(object value, Type targetType)
- **Purpose**: Attempts to convert the supplied value to the target type using `System.Convert` or type‑specific converters.
- **Parameters**: 
  - `value` – The object to convert; may be `null`.
  - `targetType` – The `System.Type` to convert to.
- **Return value**: The converted value if conversion succeeds; otherwise `null`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `targetType` is `null`.
  - Propagates any exception thrown by underlying conversion APIs (e.g., `InvalidCastException`, `FormatException`).

### ConvertTo<T>(object value)
- **Purpose**: Generic overload that attempts to convert the supplied value to type `T`.
- **Parameters**: `value` – The object to convert; may be `null`.
- **Return value**: The converted value as `T?` if conversion succeeds; otherwise `null`.
- **Exceptions**: Same as the non‑generic overload, with `targetType` inferred as `typeof(T)`.

### FindTypesThatInherit(Type baseType)
- **Purpose**: Scopes the currently loaded assemblies and returns all non‑abstract types that inherit from `baseType`.
- **Parameters**: `baseType` – The `System.Type` to match as a base class or interface.
- **Return value**: A `List<Type>` of matching types; empty list if none are found.
- **Exceptions**: Throws `ArgumentNullException` if `baseType` is `null`.

### GetAttribute<T>(MemberInfo member)
- **Purpose**: Retrieves the first attribute of type `T` applied to the supplied member.
- **Parameters**: `member` – A `System.Reflection.MemberInfo` (e.g., `Type`, `MethodInfo`, `PropertyInfo`) to inspect.
- **Return value**: The attribute instance if present; otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `member` is `null`.

### GetAttributes<T>(MemberInfo member)
- **Purpose**: Retrieves all attributes of type `T` applied to the supplied member.
- **Parameters**: `member` – A `System.Reflection.MemberInfo` to inspect.
- **Return value**: A `List<T>` containing the attributes; empty list if none are found.
- **Exceptions**: Throws `ArgumentNullException` if `member` is `null`.

## Usage

```csharp
using System;
using System.Collections.Generic;
using dotnet_deploy_notify.Helpers; // namespace containing TypeHelper

// Example 1: Determine if a type is numeric and create an instance if it is.
Type t = typeof(int?);
if (TypeHelper.IsNumeric(t))
{
    // Get the underlying type (int) and create a default instance.
    Type underlying = TypeHelper.GetUnderlyingType(t)!;
    object instance = TypeHelper.CreateInstance(underlying);
    Console.WriteLine($"Created default {underlying.Name}: {instance}");
}

// Example 2: Find all services that inherit from a base interface and read a custom attribute.
Type serviceBase = typeof(IService);
List<Type> services = TypeHelper.FindTypesThatInherit(serviceBase);
foreach (Type service in services)
{
    var attr = TypeHelper.GetAttribute<ServiceNameAttribute>(service);
    if (attr != null)
    {
        Console.WriteLine($"Service {service.Name} has name '{attr.Name}'");
    }
}
```

## Notes

- All methods are **thread‑safe**; they only read metadata and do not modify any shared state.
- Passing `null` for any reference‑type parameter results in an `ArgumentNullException`.
- `IsNumeric` and `IsNumeric<T>` consider only the built‑in numeric primitives and their nullable wrappers; custom structs that overload arithmetic operators are **not** treated as numeric.
- `GetUnderlyingType` returns `null` for non‑nullable types; callers should check for `null` before using the result.
- `CreateInstance` will return `null` if the type lacks a public parameterless constructor; it does **not** invoke non‑default or private constructors.
- Conversion methods rely on `System.Convert` and type‑specific parsers; they may throw exceptions from those APIs (e.g., `FormatException` when parsing a string to a number). The helper does not swallow those exceptions.
- `FindTypesThatInherit` examines only the assemblies currently loaded in the default reflection context; types in assemblies that have not yet been loaded will not be returned.
- Attribute retrieval methods return the first match (`GetAttribute<T>`) or all matches (`GetAttributes<T>`) based on the attribute’s `AllowMultiple` property; they do not walk inheritance chains for attributes unless the attribute itself is inherited.
