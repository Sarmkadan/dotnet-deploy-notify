# MetricsServiceTestsValidation

`MetricsServiceTestsValidation` is a static utility class that provides validation methods for metrics service test configurations. It is used to ensure that test metrics adhere to expected formats and constraints before execution, preventing runtime failures due to invalid configurations.

## API

### `Validate`

```csharp
public static IReadOnlyList<string> Validate(/* parameters */);
```

Validates the provided metrics test configuration and returns a list of validation error messages. If the configuration is valid, the list will be empty.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.

**Return value:**
- `IReadOnlyList<string>`: A read-only list of error messages. Each message describes a validation failure. If no errors are found, the list is empty.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.

---

```csharp
public static IReadOnlyList<string> Validate(/* parameters */);
```

Validates the provided metrics test configuration using a specific validation rule set and returns a list of validation error messages. If the configuration is valid, the list will be empty.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.
- `ruleSet`: The name of the validation rule set to apply.

**Return value:**
- `IReadOnlyList<string>`: A read-only list of error messages. Each message describes a validation failure. If no errors are found, the list is empty.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.
- Throws `ArgumentException` if `ruleSet` is `null` or empty.

### `IsValid`

```csharp
public static bool IsValid(/* parameters */);
```

Determines whether the provided metrics test configuration is valid according to default validation rules.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.

**Return value:**
- `bool`: `true` if the configuration is valid; otherwise, `false`.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.

---

```csharp
public static bool IsValid(/* parameters */);
```

Determines whether the provided metrics test configuration is valid according to a specific validation rule set.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.
- `ruleSet`: The name of the validation rule set to apply.

**Return value:**
- `bool`: `true` if the configuration is valid; otherwise, `false`.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.
- Throws `ArgumentException` if `ruleSet` is `null` or empty.

### `EnsureValid`

```csharp
public static void EnsureValid(/* parameters */);
```

Validates the provided metrics test configuration and throws an exception if the configuration is invalid according to default validation rules.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.
- Throws `InvalidOperationException` if the configuration is invalid, containing the validation error messages.

---

```csharp
public static void EnsureValid(/* parameters */);
```

Validates the provided metrics test configuration using a specific validation rule set and throws an exception if the configuration is invalid.

**Parameters:**
- `configuration`: The metrics test configuration object to validate.
- `ruleSet`: The name of the validation rule set to apply.

**Exceptions:**
- Throws `ArgumentNullException` if `configuration` is `null`.
- Throws `ArgumentException` if `ruleSet` is `null` or empty.
- Throws `InvalidOperationException` if the configuration is invalid, containing the validation error messages.

## Usage

```csharp
// Example 1: Basic validation with default rules
var config = new MetricsTestConfiguration
{
    MetricName = "ResponseTime",
    Threshold = 100,
    Aggregation = "Average"
};

var errors = MetricsServiceTestsValidation.Validate(config);
if (errors.Any())
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Using EnsureValid to enforce validation
try
{
    MetricsServiceTestsValidation.EnsureValid(config, "StrictRules");
    Console.WriteLine("Configuration is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Notes

- **Thread safety**: All methods in `MetricsServiceTestsValidation` are thread-safe and can be called concurrently from multiple threads without additional synchronization.
- **Performance**: The `IsValid` methods offer a lightweight check for validity without generating error messages, making them suitable for high-frequency validation scenarios.
- **Error handling**: The `Validate` methods return error messages rather than throwing exceptions, allowing callers to aggregate and process multiple validation failures at once. Use `EnsureValid` when immediate failure on the first error is desired.
- **Rule sets**: The `ruleSet` parameter allows for different validation strictness levels. Ensure that rule sets are predefined and validated elsewhere to avoid runtime errors from invalid rule set names.
