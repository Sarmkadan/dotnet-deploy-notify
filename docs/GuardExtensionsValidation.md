# GuardExtensionsValidation

Provides a set of static validation methods that return a read‑only list of error messages when a condition fails, and corresponding boolean checks that indicate whether the validation passes. Designed for input precondition enforcement and data integrity checks in a consistent, fluent style.

## API

### ValidateObject
- **Description**: Validates that an object is not null and meets any additional object‑level constraints.
- **Parameters**: (inferred) `object value`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValid (object overload)
- **Description**: Checks whether the object passes the same validation as `ValidateObject`.
- **Parameters**: (inferred) `object value`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateString
- **Description**: Validates that a string is not null, not empty, and does not exceed a maximum length (if specified).
- **Parameters**: (inferred) `string value`, `string parameterName`, `int? maxLength`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValid (string overload)
- **Description**: Checks whether the string passes the same validation as `ValidateString`.
- **Parameters**: (inferred) `string value`, `string parameterName`, `int? maxLength`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateCollection\<T\>
- **Description**: Validates that a collection is not null and contains at least one element.
- **Type parameters**: `T` – element type of the collection.
- **Parameters**: (inferred) `IEnumerable<T> value`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValid\<T\> (collection overload)
- **Description**: Checks whether the collection passes the same validation as `ValidateCollection<T>`.
- **Type parameters**: `T` – element type.
- **Parameters**: (inferred) `IEnumerable<T> value`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateCondition
- **Description**: Validates that a boolean condition is true.
- **Parameters**: (inferred) `bool condition`, `string message`.
- **Returns**: `IReadOnlyList<string>` – empty list if condition is true; otherwise list containing the provided message.
- **Throws**: `ArgumentNullException` if `message` is null.

### IsValid (condition overload)
- **Description**: Checks whether the condition passes the same validation as `ValidateCondition`.
- **Parameters**: (inferred) `bool condition`, `string message`.
- **Returns**: `true` if condition is true; otherwise `false`.

### ValidateMinimum
- **Description**: Validates that a comparable value is greater than or equal to a specified minimum.
- **Parameters**: (inferred) `IComparable value`, `IComparable minimum`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValidMinimum
- **Description**: Checks whether the value passes the same validation as `ValidateMinimum`.
- **Parameters**: (inferred) `IComparable value`, `IComparable minimum`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateMaxLength
- **Description**: Validates that a string does not exceed a specified maximum length.
- **Parameters**: (inferred) `string value`, `int maxLength`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValidMaxLength
- **Description**: Checks whether the string passes the same validation as `ValidateMaxLength`.
- **Parameters**: (inferred) `string value`, `int maxLength`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateUrl
- **Description**: Validates that a string is a well‑formed absolute URL.
- **Parameters**: (inferred) `string value`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValidUrl
- **Description**: Checks whether the string passes the same validation as `ValidateUrl`.
- **Parameters**: (inferred) `string value`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidateNotNull\<T\>
- **Description**: Validates that a value of a reference type is not null.
- **Type parameters**: `T` – the type of the value (must be a reference type).
- **Parameters**: (inferred) `T value`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if value is not null; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValidNotNull\<T\>
- **Description**: Checks whether the value passes the same validation as `ValidateNotNull<T>`.
- **Type parameters**: `T` – reference type.
- **Parameters**: (inferred) `T value`, `string parameterName`.
- **Returns**: `true` if value is not null; otherwise `false`.

### ValidateRange
- **Description**: Validates that a comparable value falls within a specified inclusive range.
- **Parameters**: (inferred) `IComparable value`, `IComparable minimum`, `IComparable maximum`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` is null.

### IsValidRange
- **Description**: Checks whether the value passes the same validation as `ValidateRange`.
- **Parameters**: (inferred) `IComparable value`, `IComparable minimum`, `IComparable maximum`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

### ValidatePattern
- **Description**: Validates that a string matches a specified regular expression pattern.
- **Parameters**: (inferred) `string value`, `string pattern`, `string parameterName`.
- **Returns**: `IReadOnlyList<string>` – empty list if valid; otherwise list of error messages.
- **Throws**: `ArgumentNullException` if `parameterName` or `pattern` is null; `ArgumentException` if `pattern` is not a valid regex.

### IsValidPattern
- **Description**: Checks whether the string passes the same validation as `ValidatePattern`.
- **Parameters**: (inferred) `string value`, `string pattern`, `string parameterName`.
- **Returns**: `true` if valid; otherwise `false`.

## Usage

```csharp
using DotnetDeployNotify.Validation;

public class DeploymentService
{
    public void Deploy(string projectPath, int maxRetries, IEnumerable<string> targetEnvironments)
    {
        // Validate string – returns list of errors
        var errors = GuardExtensionsValidation.ValidateString(projectPath, nameof(projectPath), maxLength: 260);
        if (errors.Count > 0)
            throw new ArgumentException(string.Join("; ", errors));

        // Validate collection – returns list of errors
        errors = GuardExtensionsValidation.ValidateCollection(targetEnvironments, nameof(targetEnvironments));
        if (errors.Count > 0)
            throw new ArgumentException(string.Join("; ", errors));

        // Validate range using boolean check
        if (!GuardExtensionsValidation.IsValidRange(maxRetries, 1, 10, nameof(maxRetries)))
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "Must be between 1 and 10.");

        // Proceed with deployment...
    }
}
```

```csharp
using DotnetDeployNotify.Validation;

public class NotificationService
{
    public void SendAlert(string webhookUrl, string message)
    {
        // Validate URL – returns list of errors
        var urlErrors = GuardExtensionsValidation.ValidateUrl(webhookUrl, nameof(webhookUrl));
        if (urlErrors.Any())
            throw new ArgumentException("Invalid webhook URL: " + urlErrors[0]);

        // Validate string with max length using boolean check
        if (!GuardExtensionsValidation.IsValidMaxLength(message, 500, nameof(message)))
            throw new ArgumentException("Message exceeds 500 characters.");

        // Validate not null for a custom object
        var payload = new AlertPayload(message);
        var notNullErrors = GuardExtensionsValidation.ValidateNotNull(payload, nameof(payload));
        if (notNullErrors.Any())
            throw new ArgumentNullException(nameof(payload));

        // Send notification...
    }
}
```

## Notes

- All methods are **static** and **thread‑safe** – they do not maintain any shared mutable state.
- The returned `IReadOnlyList<string>` is a snapshot of errors at the time of validation; it is safe to iterate over even if the underlying validation state changes (which it does not).
- **Edge cases**:
  - `ValidateString` treats `null` and `string.Empty` as invalid.
  - `ValidateCollection<T>` treats `null` and any empty collection as invalid.
  - `ValidateCondition` with a `false` condition always returns a single error message; the `message` parameter must not be null.
  - `ValidatePattern` throws `ArgumentException` if the `pattern` is not a valid regular expression.
  - `ValidateNotNull<T>` is intended for reference types; using it with a value type will always pass (the value can never be null).
- The `IsValid` overloads are provided for convenience when only a boolean result is needed, avoiding the overhead of allocating a list.
- Parameter names (`parameterName`) are used in error messages to identify the offending argument; they should match the actual variable name for clarity.
