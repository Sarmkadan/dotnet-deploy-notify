# DotnetDeployNotifyOptionsValidation

The `DotnetDeployNotifyOptionsValidation` class provides a static utility surface for validating configuration options related to deployment notifications within the `dotnet-deploy-notify` ecosystem. It encapsulates the logic required to verify the structural integrity and semantic correctness of option objects, offering methods to retrieve detailed error messages, perform boolean checks, or enforce validity through exceptions. This type serves as a central guardrail to ensure that notification pipelines are initialized with compliant configurations before execution begins.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate(...)
```
*Note: Multiple overloads of this method exist to support different validation contexts or input signatures.*

Executes a comprehensive validation routine against the provided options or specific configuration parameters. Depending on the overload invoked, it inspects the input for missing required fields, invalid formats, or contradictory settings.
*   **Parameters**: Varies by overload; typically accepts the options object or specific primitive values to check.
*   **Return Value**: Returns an `IReadOnlyList<string>` containing descriptive error messages for each validation failure detected. If the input is valid, the returned list is empty.
*   **Throws**: This method does not throw exceptions for validation failures; it aggregates them into the return list.

### IsValid
```csharp
public static bool IsValid(...)
```
Performs a quick check to determine if the provided options or parameters meet all validation criteria. This method is optimized for scenarios where only a pass/fail status is required without the overhead of generating error message strings.
*   **Parameters**: Varies by overload; mirrors the input signature of the corresponding `Validate` methods.
*   **Return Value**: Returns `true` if the input is fully valid; otherwise, returns `false`.
*   **Throws**: Does not throw exceptions for validation failures.

### EnsureValid
```csharp
public static void EnsureValid(...)
```
Enforces strict validity on the provided options or parameters. This method acts as a guard clause, typically used at the entry point of a service or method to halt execution immediately if the configuration is flawed.
*   **Parameters**: Varies by overload; mirrors the input signature of the corresponding `Validate` methods.
*   **Return Value**: Returns `void` if the input is valid.
*   **Throws**: Throws an exception (typically `ArgumentException` or a custom validation exception) if any validation rules are violated. The exception message usually aggregates the errors found during validation.

## Usage

### Example 1: Pre-flight Check with Error Reporting
This pattern is useful during application startup or configuration reloading, where you need to log specific reasons why a configuration block was rejected without crashing the entire process immediately.

```csharp
var options = new DotnetDeployNotifyOptions 
{ 
    WebhookUrl = "", // Invalid: empty URL
    TimeoutSeconds = -5 // Invalid: negative timeout
};

var errors = DotnetDeployNotifyOptionsValidation.Validate(options);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration Error: {error}");
    }
    // Fallback to default configuration or abort initialization gracefully
}
else
{
    // Proceed with valid options
    InitializeNotificationService(options);
}
```

### Example 2: Strict Guard Clause in Method Entry
Use `EnsureValid` when a method cannot proceed safely with invalid data, ensuring that any downstream logic assumes a verified state.

```csharp
public void SendDeploymentNotification(DotnetDeployNotifyOptions options)
{
    // Throws immediately if options are invalid, preventing downstream null references 
    // or logic errors caused by bad state.
    DotnetDeployNotifyOptionsValidation.EnsureValid(options);

    // Execution continues only if validation passes
    var client = new NotificationClient(options.WebhookUrl);
    client.PostAsync(options.Payload);
}
```

## Notes

*   **Thread Safety**: As `DotnetDeployNotifyOptionsValidation` exposes only static methods and operates purely on input parameters without maintaining internal mutable state, it is inherently thread-safe. Multiple threads may invoke `Validate`, `IsValid`, or `EnsureValid` concurrently without risk of race conditions.
*   **Overload Resolution**: The presence of multiple `Validate` overloads requires careful attention to argument types during compilation. Ensure the correct overload is selected based on whether you are validating a full options object or individual primitive properties.
*   **Exception Handling**: When using `EnsureValid`, callers must be prepared to catch validation exceptions. It is recommended to wrap calls to `EnsureValid` in try-catch blocks if the invalid state is a recoverable scenario, rather than a critical failure.
*   **Empty Results**: An empty `IReadOnlyList<string>` returned by `Validate` guarantees that `IsValid` would return `true` for the same input, and `EnsureValid` would complete without throwing.
