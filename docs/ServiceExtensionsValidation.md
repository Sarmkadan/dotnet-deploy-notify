# ServiceExtensionsValidation

ServiceExtensionsValidation provides static methods for validating service extensions in the IServiceCollection during application startup. It ensures that required services are properly configured and throws exceptions or returns validation results when misconfigurations are detected.

## API

### Validate

#### `Validate(IServiceCollection services)`
Validates the provided service collection for common misconfigurations.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `IReadOnlyList<string>`: A list of error messages describing validation failures.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  

#### `Validate<T>(IServiceCollection services)`
Validates the provided service collection for misconfigurations specific to the generic type `T`.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `IReadOnlyList<string>`: A list of error messages describing validation failures.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  

---

### IsValid

#### `IsValid(IServiceCollection services)`
Determines whether the provided service collection passes all validation checks.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `bool`: `true` if valid; `false` otherwise.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  

#### `IsValid<T>(IServiceCollection services)`
Determines whether the provided service collection passes validation checks specific to the generic type `T`.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `bool`: `true` if valid; `false` otherwise.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  

---

### EnsureValid

#### `EnsureValid(IServiceCollection services)`
Validates the provided service collection and throws an exception if invalid.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `void`.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  
- `InvalidOperationException` if validation fails.  

#### `EnsureValid<T>(IServiceCollection services)`
Validates the provided service collection for type-specific misconfigurations and throws an exception if invalid.  
**Parameters:**  
- `services`: The IServiceCollection to validate.  
**Returns:**  
- `void`.  
**Throws:**  
- `ArgumentNullException` if `services` is null.  
- `InvalidOperationException` if validation fails.  

---

## Usage

### Example 1: Validate and Log Errors
```csharp
var services = new ServiceCollection();
// Configure services...
var errors = ServiceExtensionsValidation.Validate(services);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Ensure Valid Configuration
```csharp
var services = new ServiceCollection();
// Configure services...
try
{
    ServiceExtensionsValidation.EnsureValid<DeploymentOptions>(services);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Deployment configuration invalid: {ex.Message}");
}
```

---

## Notes

- **Thread Safety**: These methods are static and do not maintain state. However, if the underlying validation logic accesses shared mutable state (e.g., static caches), concurrent calls may lead to race conditions. Ensure thread-safe access to such resources if used in multi-threaded contexts.
- **Edge Cases**:  
  - Passing a `null` `IServiceCollection` to any method will throw `ArgumentNullException`.  
  - `EnsureValid` methods throw `InvalidOperationException` with aggregated error messages when validation fails, which may include multiple issues.  
  - Generic type-specific overloads (`Validate<T>`, `IsValid<T>`, `EnsureValid<T>`) may impose additional constraints on the service collection beyond the non-generic versions.
