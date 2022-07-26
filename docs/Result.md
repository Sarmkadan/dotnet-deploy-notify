# Result

The `Result` type provides a functional approach to error handling in C#, encapsulating the outcome of an operation as either a success or a failure without relying on exceptions for control flow. It supports both non-generic results for void-returning operations and generic `Result<T>` variants for operations returning data, offering fluent methods for chaining, transformation, and error aggregation to ensure robust and readable deployment notification logic.

## API

### Properties

*   **`public bool IsSuccess`**
    Indicates whether the operation completed successfully. Returns `true` if the result represents success; otherwise, `false`.

*   **`public string? Error`**
    Contains the primary error message if the operation failed. Returns `null` if the operation was successful or if no specific primary error was set.

*   **`public List<string> Errors`**
    A collection of all error messages associated with a failed result. For successful results, this list is typically empty.

*   **`public T? Value`** (Generic `Result<T>` only)
    Retrieves the underlying value if the operation was successful. Returns `default(T)` (or `null` for reference types) if the operation failed. Accessing this property on a failure does not throw but returns the default value.

### Static Factory Methods (Non-Generic)

*   **`public static Result Ok`**
    Creates a new `Result` instance representing a successful operation with no data payload.

*   **`public static Result Fail`**
    Creates a new `Result` instance representing a failed operation. Overloads exist to accept no arguments (creating a generic failure) or specific error details depending on the concrete implementation context.

*   **`public static Result Fail`**
    Overloaded variant to create a failed `Result`, typically accepting parameters such as an error message or exception to populate the `Error` and `Errors` collections.

### Static Factory Methods (Generic `Result<T>`)

*   **`public static Result<T> Ok`**
    Creates a new `Result<T>` instance representing a successful operation containing the specified value of type `T`.

*   **`public new static Result<T> Fail`**
    Creates a new `Result<T>` instance representing a failed operation. The `new` keyword indicates this hides the base non-generic `Fail` method to ensure the correct generic type is returned.

*   **`public new static Result<T> Fail`**
    Overloaded variant for creating a failed `Result<T>`, allowing the attachment of specific error messages or exceptions while maintaining the generic type definition.

### Transformation and Chaining

*   **`public Result<TNew> Map<TNew>`**
    Transforms the success value of type `T` into a new type `TNew` using a provided selector function. If the current result is a failure, the function is not executed, and the failure is propagated unchanged.

*   **`public Result<TNew> Bind<TNew>`**
    Chains a function that returns a `Result<TNew>`. If the current result is successful, the function is executed; if it fails, the function is skipped, and the original failure is returned. This prevents nested `Result` structures.

*   **`public Result<T> OnSuccess`**
    Executes an action (side effect) if the result is successful. The original `Result<T>` is returned unchanged, allowing further chaining.

*   **`public Result<T> OnFailure`**
    Executes an action (side effect) if the result is a failure. The original `Result<T>` is returned unchanged, allowing further chaining.

### Value Extraction

*   **`public T GetValueOrDefault`**
    Returns the contained value if successful; otherwise, returns `default(T)`. This method never throws an exception.

*   **`public T GetValueOrThrow`**
    Returns the contained value if successful. If the result is a failure, this method throws an exception (typically an `InvalidOperationException` or a custom exception containing the error details).

### Builder Pattern

*   **`public ResultBuilder Success`**
    Initiates or configures the success state via a `ResultBuilder`. Used to fluently construct complex success scenarios.

*   **`public ResultBuilder Error`**
    Initiates or configures the error state via a `ResultBuilder`. Used to set the primary error message.

*   **`public ResultBuilder AddError`**
    Appends an additional error message to the `Errors` collection via the `ResultBuilder`, supporting scenarios where multiple issues occur during a single operation.

### Overrides

*   **`public override string ToString`**
    Returns a string representation of the result. Typically includes the status (Success/Failure) and, if applicable, the value or the primary error message.

## Usage

### Example 1: Handling a Deployment Step with Error Aggregation

This example demonstrates creating a failure result with multiple errors and using `OnFailure` to log issues before returning.

```csharp
using DotNetDeployNotify;

public Result ValidateDeploymentConfig(string configPath)
{
    if (!File.Exists(configPath))
    {
        return Result.Fail
            .Error("Configuration file not found")
            .AddError($"Path '{configPath}' is invalid")
            .Build(); // Assuming Build() finalizes the builder pattern based on the provided members
    }

    var content = File.ReadAllText(configPath);
    if (string.IsNullOrWhiteSpace(content))
    {
        return Result.Fail
            .Error("Configuration is empty")
            .AddError("File contains no valid JSON")
            .Build();
    }

    return Result.Ok;
}

// Usage
var result = ValidateDeploymentConfig("./appsettings.json");
result.OnFailure(r => 
{
    foreach (var err in r.Errors)
    {
        Console.WriteLine($"Validation Error: {err}");
    }
});

if (!result.IsSuccess)
{
    // Handle failure logic
}
```

### Example 2: Transforming Data with Map and Bind

This example shows a successful retrieval of a deployment ID, transforming it into a URL, and then binding it to a notification service call.

```csharp
using DotNetDeployNotify;

public Result<string> GetDeploymentUrl(int deploymentId)
{
    return Result.Ok(deploymentId)
        .Map(id => $"https://deploy.example.com/status/{id}")
        .Bind(url => SendNotificationAsync(url)); 
}

private Result<string> SendNotificationAsync(string url)
{
    // Simulate API call
    if (url.Contains("invalid"))
    {
        return Result<string>.Fail("Notification service unreachable");
    }
    return Result<string>.Ok($"Sent to {url}");
}

// Usage
var finalResult = GetDeploymentUrl(101);

if (finalResult.IsSuccess)
{
    var message = finalResult.GetValueOrThrow();
    Console.WriteLine(message);
}
else
{
    Console.WriteLine($"Operation failed: {finalResult.Error}");
}
```

## Notes

*   **Thread Safety**: The `Result` type itself is immutable once constructed, making read operations on `IsSuccess`, `Error`, and `Value` thread-safe. However, the `Errors` property exposes a `List<string>`. While the list reference is stable, concurrent modification of the list contents (e.g., during builder construction before the object is finalized) by multiple threads is not inherently synchronized. Consumers should treat the `Errors` list as read-only after the `Result` instance is returned from a factory method.
*   **Exception Handling**: The `GetValueOrThrow` method is the only member in this API surface guaranteed to throw an exception, and only when `IsSuccess` is `false`. All other accessors (`Value`, `Error`) return `null` or default values on failure, adhering to the pattern of avoiding unexpected exceptions during inspection.
*   **Builder Fluency**: The presence of `ResultBuilder` properties (`Success`, `Error`, `AddError`) suggests a fluent construction pattern. Ensure that the builder is fully configured and the final `Result` object is instantiated before sharing the instance across threads or contexts, as the intermediate builder state may be mutable.
*   **Generic Hiding**: The use of `new` on the static `Fail` methods for `Result<T>` indicates that care must be taken when calling these methods from a context where the type is inferred as the non-generic base. Always explicitly specify the generic type argument (e.g., `Result<T>.Fail(...)`) to ensure the correct overload is invoked and the return type matches expectations.
