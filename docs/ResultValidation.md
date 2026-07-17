# ResultValidation

Provides static helper members for validating objects or validation state and exposing the results as read‑only lists of error messages, boolean validity checks, and helpers that throw when validation fails.

## API

### Validate
- **Purpose:** Returns a read-only list of validation error messages for the current validation state.  
- **Parameters:** None.  
- **Return value:** `IReadOnlyList<string>` containing error messages; an empty list indicates success.  
- **Exceptions:** None.

### Validate<T>
- **Purpose:** Returns a read-only list of validation error messages for a value of type `T` (or its default instance) according to the validation rules defined for `T`.  
- **Parameters:** None (generic type argument `T`).  
- **Return value:** `IReadOnlyList<string>` of error messages; empty if the value is valid.  
- **Exceptions:** None.

### IsValid
- **Purpose:** Determines whether the current validation state contains any errors.  
- **Parameters:** None.  
- **Return value:** `true` if no validation errors are present; otherwise `false`.  
- **Exceptions:** None.

### IsValid<T>
- **Purpose:** Determines whether a value of type `T` (or its default instance) passes validation.  
- **Parameters:** None (generic type argument `T`).  
- **Return value:** `true` if the value is valid; otherwise `false`.  
- **Exceptions:** None.

### EnsureValid
- **Purpose:** Throws an exception if the current validation state is invalid.  
- **Parameters:** None.  
- **Return value:** `void`.  
- **Exceptions:** Throws `InvalidOperationException` with a message that concatenates all validation errors when `IsValid` returns `false`.

### EnsureValid<T>
- **Purpose:** Throws an exception if a value of type `T` (or its default instance) fails validation.  
- **Parameters:** None (generic type argument `T`).  
- **Return value:** `void`.  
- **Exceptions:** Throws `InvalidOperationException` containing the validation errors when `IsValid<T>` returns `false`.

## Usage

```csharp
using DotNetDeployNotify.Validation;

// Example 1: Non‑generic validation check
var errors = ResultValidation.Validate();
if (ResultValidation.IsValid())
{
    // Proceed with operation
}
else
{
    // Handle validation failures
    foreach (var err in errors)
    {
        Console.WriteLine(err);
    }
}
```

```csharp
using DotNetDeployNotify.Validation;

public class DeploymentOptions
{
    public string Environment { get; set; }
    public int RetryCount { get; set; }
}

// Example 2: Generic validation and ensuring validity
var opts = new DeploymentOptions { Environment = "prod", RetryCount = 3 };

var optErrors = ResultValidation.Validate<DeploymentOptions>();
if (!ResultValidation.IsValid<DeploymentOptions>())
{
    // Deal with errors
}
else
{
    // Safe to use; EnsureValid will not throw
    ResultValidation.EnsureValid<DeploymentOptions>();
}

// If validation fails, EnsureValid throws
try
{
    ResultValidation.EnsureValid<DeploymentOptions>();
}
catch (InvalidOperationException ex)
{
    // ex.Message contains all validation errors
    Console.WriteLine(ex.Message);
}
```

## Notes

- All members are stateless and do not modify internal data, making them thread‑safe for concurrent invocation.  
- If the underlying validation logic relies on external mutable state (e.g., static configuration), callers must synchronize access to that state to avoid inconsistent results.  
- `Validate<T>` and `IsValid<T>` operate on the default instance of `T` unless the implementation provides a mechanism to supply an instance; refer to the source code for exact behavior.  
- The methods never return `null`; an empty list signifies a successful validation.  
- `EnsureValid` and `EnsureValid<T>` throw `InvalidOperationException`; the exception message combines all error messages, typically separated by newlines.  
- These helpers are intended for lightweight validation scenarios; for more complex validation pipelines consider using a dedicated validation framework.
