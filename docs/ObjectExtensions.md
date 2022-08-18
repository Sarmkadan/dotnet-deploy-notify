# ObjectExtensions

`ObjectExtensions` is a static utility class providing a collection of extension methods for `System.Object` and generic types in C#. These methods facilitate common object operations such as safe casting, null checks, property reflection, value mapping, and dictionary conversion. The extensions are designed to reduce boilerplate code and improve readability in scenarios involving type safety, null handling, and object inspection.

## API

### `SafeCast<T>(this object? source)`
Converts an object to the specified type `T` if the conversion is valid; otherwise, returns `null`.

**Parameters:**
- `source` (`object?`): The object to cast.

**Returns:**
- `T?`: The cast object if successful; otherwise, `null`.

**Throws:**
- Does not throw exceptions.

---

### `IsNull(this object? obj)`
Determines whether the specified object is `null`.

**Parameters:**
- `obj` (`object?`): The object to check.

**Returns:**
- `bool`: `true` if the object is `null`; otherwise, `false`.

**Throws:**
- Does not throw exceptions.

---

### `IsNotNull(this object? obj)`
Determines whether the specified object is not `null`.

**Parameters:**
- `obj` (`object?`): The object to check.

**Returns:**
- `bool`: `true` if the object is not `null`; otherwise, `false`.

**Throws:**
- Does not throw exceptions.

---

### `IfNotNull<T>(this T? source, Action<T> action)`
Executes the specified action if the object is not `null`.

**Parameters:**
- `source` (`T?`): The object to check.
- `action` (`Action<T>`): The action to execute if the object is not `null`.

**Returns:**
- `T?`: The original object, allowing method chaining.

**Throws:**
- Does not throw exceptions.

---

### `Map<T, TResult>(this T? source, Func<T, TResult> mapper)`
Applies the specified mapping function to the object if it is not `null`; otherwise, returns `null`.

**Parameters:**
- `source` (`T?`): The object to map.
- `mapper` (`Func<T, TResult>`): The mapping function to apply.

**Returns:**
- `TResult?`: The result of the mapping function if the object is not `null`; otherwise, `null`.

**Throws:**
- Does not throw exceptions.

---

### `ShallowCopy<T>(this T source)`
Creates a shallow copy of the specified object by serializing and deserializing it using `System.Text.Json`. The object must be serializable.

**Parameters:**
- `source` (`T`): The object to copy.

**Returns:**
- `T?`: A shallow copy of the object, or `null` if the copy fails.

**Throws:**
- `ArgumentNullException`: If `source` is `null`.
- `JsonException`: If serialization or deserialization fails.

---

### `GetPropertyValue(this object? obj, string propertyName)`
Retrieves the value of the specified property via reflection.

**Parameters:**
- `obj` (`object?`): The object containing the property.
- `propertyName` (`string`): The name of the property.

**Returns:**
- `object?`: The value of the property if found; otherwise, `null`.

**Throws:**
- `ArgumentNullException`: If `obj` or `propertyName` is `null`.
- `ArgumentException`: If the property does not exist on the object.

---

### `SetPropertyValue(this object? obj, string propertyName, object? value)`
Sets the value of the specified property via reflection.

**Parameters:**
- `obj` (`object?`): The object containing the property.
- `propertyName` (`string`): The name of the property.
- `value` (`object?`): The value to assign to the property.

**Throws:**
- `ArgumentNullException`: If `obj` or `propertyName` is `null`.
- `ArgumentException`: If the property does not exist or is read-only.

---

### `ToDictionary(this object? obj)`
Converts the public readable properties of an object into a dictionary with property names as keys and property values as values.

**Parameters:**
- `obj` (`object?`): The object to convert.

**Returns:**
- `Dictionary<string, object?>`: A dictionary representing the object's properties, or an empty dictionary if `obj` is `null`.

**Throws:**
- Does not throw exceptions.

---

### `EqualsAny<T>(this T? obj, params T[] values)`
Determines whether the object is equal to any of the provided values.

**Parameters:**
- `obj` (`T?`): The object to compare.
- `values` (`params T[]`): The values to compare against.

**Returns:**
- `bool`: `true` if the object equals any of the provided values; otherwise, `false`.

**Throws:**
- Does not throw exceptions.

---

### `IsDefault<T>(this T? obj)`
Determines whether the object is equal to its default value (e.g., `null` for reference types, `0` for numeric types, `false` for `bool`).

**Parameters:**
- `obj` (`T?`): The object to check.

**Returns:**
- `bool`: `true` if the object is equal to its default value; otherwise, `false`.

**Throws:**
- Does not throw exceptions.

---

### `GetValueOrDefault<T>(this T? obj, T defaultValue)`
Returns the object if it is not `null`; otherwise, returns the specified default value.

**Parameters:**
- `obj` (`T?`): The object to check.
- `defaultValue` (`T`): The value to return if the object is `null`.

**Returns:**
- `T`: The object if not `null`; otherwise, `defaultValue`.

**Throws:**
- Does not throw exceptions.

---

### `ToStringSafe(this object? obj)`
Returns the string representation of the object, or `string.Empty` if the object is `null`.

**Parameters:**
- `obj` (`object?`): The object to convert to a string.

**Returns:**
- `string`: The string representation of the object, or `string.Empty` if `null`.

**Throws:**
- Does not throw exceptions.

---

### `GetTypeName(this object? obj)`
Returns the short type name of the object (e.g., `"String"` for `System.String`).

**Parameters:**
- `obj` (`object?`): The object to inspect.

**Returns:**
- `string`: The short type name, or `"null"` if the object is `null`.

**Throws:**
- Does not throw exceptions.

---

### `GetFullTypeName(this object? obj)`
Returns the fully qualified type name of the object (e.g., `"System.String"` for `System.String`).

**Parameters:**
- `obj` (`object?`): The object to inspect.

**Returns:**
- `string`: The fully qualified type name, or `"null"` if the object is `null`.

**Throws:**
- Does not throw exceptions.

---

### `Chain<T>(this T obj, Action<T> action)`
Executes the specified action on the object and returns the object, enabling method chaining.

**Parameters:**
- `obj` (`T`): The object to act upon.
- `action` (`Action<T>`): The action to execute.

**Returns:**
- `T`: The original object.

**Throws:**
- `ArgumentNullException`: If `obj` or `action` is `null`.

---

### `Validate<T>(this T? obj, Func<T, bool> predicate, string? errorMessage = null)`
Validates the object against the specified predicate. Throws an exception if the predicate returns `false`.

**Parameters:**
- `obj` (`T?`): The object to validate.
- `predicate` (`Func<T, bool>`): The validation predicate.
- `errorMessage` (`string?`): Optional error message to include in the exception.

**Returns:**
- `T`: The original object if validation succeeds.

**Throws:**
- `ArgumentNullException`: If `obj` or `predicate` is `null`.
- `ArgumentException`: If the predicate returns `false`.

## Usage

### Example 1: Safe Casting and Null Handling
