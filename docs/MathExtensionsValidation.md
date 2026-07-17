# MathExtensionsValidation

Provides centralized validation logic for the mathematical extension methods in the `dotnet-deploy-notify` project. This static class exposes both boolean checks and exception-throwing guards that verify preconditions—such as range constraints, numeric types, and argument validity—before executing operations like clamping, rounding, averaging, or human-readable formatting. It ensures that invalid inputs are caught early with clear error messages.

## API

### `Validate`

```csharp
public static IReadOnlyList<string> Validate { get; }
```

Returns a read-only list of all accumulated validation error messages across the application. This property is populated by calling the various `Validate*` methods; it does not throw exceptions itself. Useful for collecting multiple failures before reporting them in bulk.

### `IsValid`

```csharp
public static bool IsValid { get; }
```

Indicates whether the current validation state is clean (i.e., no errors have been recorded). Returns `true` when the internal error list is empty; otherwise `false`.

### `EnsureValid`

```csharp
public static void EnsureValid()
```

Throws an `InvalidOperationException` if any validation errors are present. Call this after one or more `Validate*` methods to fail fast when invalid state is detected. If the error list is empty, the method returns silently.

### `ValidateClamp<T>`

```csharp
public static IReadOnlyList<string> ValidateClamp<T>(T value, T min, T max, [CallerArgumentExpression("value")] string paramName = null)
```

Validates that `value` is within the inclusive range `[min, max]` and that `min` is not greater than `max`. The generic parameter `T` must be a numeric type. Appends error messages to the internal list and returns the updated list. The `paramName` is automatically captured for better error reporting.

### `ValidateIsBetween<T>`

```csharp
public static IReadOnlyList<string> ValidateIsBetween<T>(T value, T lower, T upper, [CallerArgumentExpression("value")] string paramName = null)
```

Validates that `value` lies strictly or inclusively between `lower` and `upper` (depending on the overload semantics). Ensures `lower` is not greater than `upper`. Appends errors and returns the list.

### `ValidateToPercentage` (two overloads)

```csharp
public static IReadOnlyList<string> ValidateToPercentage(double value, [CallerArgumentExpression("value")] string paramName = null)
public static IReadOnlyList<string> ValidateToPercentage(decimal value, [CallerArgumentExpression("value")] string paramName = null)
```

Validates that the given `value` is a valid percentage (typically between 0 and 100, or 0 and 1 depending on internal convention). Both overloads append errors for out-of-range values and return the updated error list.

### `ValidateRoundTo` (two overloads)

```csharp
public static IReadOnlyList<string> ValidateRoundTo(decimal value, int decimals, [CallerArgumentExpression("value")] string paramName = null)
public static IReadOnlyList<string> ValidateRoundTo(double value, int decimals, [CallerArgumentExpression("value")] string paramName = null)
```

Validates that `decimals` is non-negative and that `value` is a finite number (not `NaN` or infinity). Appends errors and returns the updated list.

### `ValidateAverage`

```csharp
public static IReadOnlyList<string> ValidateAverage<T>(IEnumerable<T> source, [CallerArgumentExpression("source")] string paramName = null)
```

Validates that `source` is not null and contains at least one element before computing an average. Appends errors and returns the updated list.

### `ValidateMedian`

```csharp
public static IReadOnlyList<string> ValidateMedian<T>(IEnumerable<T> source, [CallerArgumentExpression("source")] string paramName = null)
```

Validates that `source` is not null and contains at least one element before computing a median. Appends errors and returns the updated list.

### `ValidateSafeSum`

```csharp
public static IReadOnlyList<string> ValidateSafeSum<T>(IEnumerable<T> source, [CallerArgumentExpression("source")] string paramName = null)
```

Validates that `source` is not null and that summing its elements will not overflow the underlying numeric type. Appends errors and returns the updated list.

### `ValidateSafeAverage`

```csharp
public static IReadOnlyList<string> ValidateSafeAverage<T>(IEnumerable<T> source, [CallerArgumentExpression("source")] string paramName = null)
```

Validates that `source` is not null, contains at least one element, and that computing the average will not overflow. Appends errors and returns the updated list.

### `ValidateToHumanReadableSize`

```csharp
public static IReadOnlyList<string> ValidateToHumanReadableSize(long bytes, [CallerArgumentExpression("bytes")] string paramName = null)
```

Validates that `bytes` is non-negative before converting to a human-readable file size string (e.g., "1.5 GB"). Appends errors and returns the updated list.

### `ValidateToHumanReadableDuration` (two overloads)

```csharp
public static IReadOnlyList<string> ValidateToHumanReadableDuration(TimeSpan duration, [CallerArgumentExpression("duration")] string paramName = null)
public static IReadOnlyList<string> ValidateToHumanReadableDuration(double milliseconds, [CallerArgumentExpression("milliseconds")] string paramName = null)
```

Validates that the duration is non-negative and, for the `double` overload, not `NaN` or infinity. Appends errors and returns the updated list.

### `ValidateCalculateCompoundInterest`

```csharp
public static IReadOnlyList<string> ValidateCalculateCompoundInterest(decimal principal, decimal rate, int periods, [CallerArgumentExpression("principal")] string paramName = null)
```

Validates that `principal` is non-negative, `rate` is a valid decimal percentage, and `periods` is positive. Appends errors and returns the updated list.

### `ValidateRandomBetween`

```csharp
public static IReadOnlyList<string> ValidateRandomBetween(int min, int max, [CallerArgumentExpression("min")] string paramName = null)
```

Validates that `min` is not greater than `max`. Appends errors and returns the updated list.

### `EnsureValidClamp<T>`

```csharp
public static void EnsureValidClamp<T>(T value, T min, T max, [CallerArgumentExpression("value")] string paramName = null)
```

Combines `ValidateClamp<T>` and `EnsureValid`. Validates the clamp parameters and immediately throws an `AggregateException` if any errors are found. Returns silently on success.

### `EnsureValidIsBetween<T>`

```csharp
public static void EnsureValidIsBetween<T>(T value, T lower, T upper, [CallerArgumentExpression("value")] string paramName = null)
```

Combines `ValidateIsBetween<T>` and `EnsureValid`. Validates the between parameters and immediately throws an `AggregateException` if any errors are found. Returns silently on success.

## Usage

### Example 1: Batch validation with deferred throw

```csharp
// Collect multiple validation errors before deciding to throw
var errors = MathExtensionsValidation.ValidateClamp(150, 0, 100, nameof(inputValue));
errors = MathExtensionsValidation.ValidateAverage(Enumerable.Empty<int>(), nameof(emptyList));
errors = MathExtensionsValidation.ValidateToHumanReadableSize(-5, nameof(negativeBytes));

if (!MathExtensionsValidation.IsValid)
{
    // Log all errors, then throw
    foreach (var error in errors)
        Console.WriteLine(error);
    MathExtensionsValidation.EnsureValid();
}
```

### Example 2: Immediate throw on invalid input

```csharp
// Fail fast when processing user input
public void ProcessUserInput(int value, int lower, int upper)
{
    MathExtensionsValidation.EnsureValidIsBetween(value, lower, upper);
    // Proceed with the operation knowing the range is valid
    var result = value.Clamp(lower, upper);
}
```

## Notes

- All `Validate*` methods are **append-only**; they add errors to a shared internal list and never clear it automatically. Callers must manage the lifecycle of the error list, typically by resetting it before a new validation cycle.
- The `EnsureValid*` methods throw `AggregateException` (or a custom exception type wrapping multiple errors) when validation fails, allowing callers to catch and inspect all failures at once.
- **Thread safety**: The static validation state is not thread-safe by default. Concurrent calls to `Validate*` methods from multiple threads may corrupt the error list. Use external synchronization if validation is performed in parallel contexts.
- The generic methods `ValidateClamp<T>` and `ValidateIsBetween<T>` are constrained to numeric types at runtime; passing a non-numeric type will result in an error message appended to the list rather than a compile-time error.
- `ValidateToPercentage` overloads may enforce different valid ranges (0–100 or 0–1) depending on internal conventions; consult the specific overload's implementation for exact bounds.
- `ValidateSafeSum` and `ValidateSafeAverage` use `checked` arithmetic internally to detect overflow conditions before the actual computation occurs.
