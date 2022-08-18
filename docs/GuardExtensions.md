# GuardExtensions

Provides a set of static helper methods for validating method arguments and throwing descriptive exceptions when preconditions are not met. The extensions are designed to be fluent and concise, allowing developers to guard against invalid input with a single line of code.

## API

### ThrowIfNull
```csharp
public static void ThrowIfNull(object value, string paramName)
```
- **Purpose**: Throws an `ArgumentNullException` if `value` is `null`.
- **Parameters**:
  - `value`: The object to test.
  - `paramName`: The name of the parameter being validated (used in the exception message).
- **Return value**: None.
- **Throws**: `ArgumentNullException` when `value` is `null`.

### ThrowIfNullOrEmpty
```csharp
public static void ThrowIfNullOrEmpty(string value, string paramName)
```
- **Purpose**: Throws an `ArgumentException` if `value` is `null` or an empty string.
- **Parameters**:
  - `value`: The string to test.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentException` when `value` is `null` or `string.Empty`.

### ThrowIfNullOrEmpty<T>
```csharp
public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> value, string paramName)
```
- **Purpose**: Throws an `ArgumentException` if `value` is `null` or contains no elements.
- **Parameters**:
  - `value`: The enumerable to test.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentException` when `value` is `null` or `!value.Any()`.

### ThrowIfFalse
```csharp
public static void ThrowIfFalse(bool condition, string paramName)
```
- **Purpose**: Throws an `ArgumentException` if `condition` is `false`.
- **Parameters**:
  - `condition`: The Boolean condition to test.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentException` when `condition` evaluates to `false`.

### ThrowIfLessThan
```csharp
public static void ThrowIfLessThan<T>(T value, T minimum, string paramName) where T : IComparable<T>
```
- **Purpose**: Throws an `ArgumentOutOfRangeException` if `value` is less than `minimum`.
- **Parameters**:
  - `value`: The value to compare.
  - `minimum`: The lower inclusive bound.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentOutOfRangeException` when `value.CompareTo(minimum) < 0`.

### ThrowIfLongerThan
```csharp
public static void ThrowIfLongerThan(string value, int maxLength, string paramName)
```
- **Purpose**: Throws an `ArgumentException` if the length of `value` exceeds `maxLength`.
- **Parameters**:
  - `value`: The string to test.
  - `maxLength`: The maximum allowed length.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentException` when `value.Length > maxLength`.

### ThrowIfInvalidUrl
```csharp
public static void ThrowIfInvalidUrl(string url, string paramName)
```
- **Purpose**: Throws an `ArgumentException` if `url` is not a well-formed absolute URI.
- **Parameters**:
  - `url`: The string to test.
  - `paramName`: The name of the parameter being validated.
- **Return value**: None.
- **Throws**: `ArgumentException` when `Uri.TryCreate(url, UriKind.Absolute, out _)` returns `false`.

### GetValueOrThrow<T>
```csharp
public static T GetValueOrThrow<T>(this T? value, string paramName) where T : struct
```
- **Purpose**: Returns the underlying value if `value` has a value; otherwise throws an `ArgumentNullException`.
- **Parameters**:
  - `value`: The nullable value type to test.
  - `paramName`: The name of the parameter being validated.
- **Return value**: The typed value (`T`) when `value.HasValue` is `true`.
- **Throws**: `ArgumentNullException` when `value.HasValue` is `false`.

### IsInRange
```csharp
public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
```
- **Purpose**: Determines whether `value` falls within the inclusive range `[min, max]`.
- **Parameters**:
  - `value`: The value to test.
  - `min`: The lower bound.
  - `max`: The upper bound.
- **Return value**: `true` if `value` is greater than or equal to `min` and less than or equal to `max`; otherwise `false`.
- **Throws**: None.

### MatchesPattern
```csharp
public static bool MatchesPattern(string input, string pattern)
```
- **Purpose**: Indicates whether `input` matches the regular expression supplied by `pattern`.
- **Parameters**:
  - `input`: The string to test.
  - `pattern`: The regular expression pattern.
- **Return value**: `true` if `Regex.IsMatch(input, pattern)` succeeds; otherwise `false`.
- **Throws**: None (exceptions from `Regex` construction or matching are allowed to propagate).

## Usage

```csharp
public void ProcessItem(string name, int count, Uri source)
{
    GuardExtensions.ThrowIfNullOrEmpty(name, nameof(name));
    GuardExtensions.ThrowIfLessThan(count, 1, nameof(count));
    GuardExtensions.ThrowIfInvalidUrl(source?.ToString(), nameof(source));

    // Continue with valid arguments…
}
```

```csharp
public TResult Compute<T, TResult>(T? value, Func<T, TResult> selector) where T : struct
{
    T val = GuardExtensions.GetValueOrThrow(value, nameof(value));
    if (!GuardExtensions.IsInRange(val, default(T), (T)(object)100))
        throw new ArgumentOutOfRangeException(nameof(value));

    return selector(val);
}
```

## Notes

- All guard methods are stateless and rely only on their input arguments; therefore they are thread‑safe and can be invoked concurrently from multiple threads without external synchronization.
- `ThrowIfNullOrEmpty<T>` enumerates the source only enough to determine emptiness (`Any()`); it does not consume the entire sequence, preserving lazy evaluation for LINQ sequences.
- `MatchesPattern` does not pre‑compile the regular expression; if the same pattern is used repeatedly, consider caching a `Regex` instance for performance.
- `GetValueOrThrow<T>` is constrained to nullable value types (`struct?`). Passing a reference type will not compile; for reference‑type null checks use `ThrowIfNull`.
- The methods do not perform culture‑specific comparisons; string‑based checks (`ThrowIfNullOrEmpty`, `ThrowIfLongerThan`) use ordinal semantics implicitly via `string.Length` and `string.IsNullOrEmpty`. If culture‑aware validation is required, perform it before calling these guards.
