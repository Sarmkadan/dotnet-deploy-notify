# NotificationPipeline

The `NotificationPipeline` class orchestrates the processing of deployment notifications within the `dotnet-deploy-notify` library. It provides a fluent API to configure validation and filtering steps, execute the pipeline, and inspect the resulting notification, validation state, and any collected data or errors.

## API

### Constructor: `NotificationPipeline()`
Initializes a new instance of the `NotificationPipeline` with default internal state. No parameters are required.

### Method: `NotificationPipeline Use()`
Adds a processing step to the pipeline and returns the same instance to allow chaining. The method does not take parameters; the specific step type is inferred from the context in which `Use` is called. Throws `InvalidOperationException` if called after `ExecuteAsync` has already been invoked.

### Method: `async Task<PipelineResult> ExecuteAsync()`
Executes the configured pipeline steps asynchronously. Returns a `PipelineResult` indicating the outcome of the processing. Throws `InvalidOperationException` if the pipeline is not in a valid state (e.g., missing required steps) or if an internal step throws an exception during execution.

### Property: `DeploymentNotification Notification`
Gets the original deployment notification supplied to the pipeline. The property is read‑only after construction. No exceptions are thrown by the getter.

### Property: `bool IsValid`
Indicates whether the notification has passed all validation steps configured in the pipeline. Returns `true` if no validation errors were recorded; otherwise `false`. No exceptions are thrown by the getter.

### Property: `List<string> Errors`
Gets a collection of error messages produced during validation or processing. The list is empty when `IsValid` is `true`. The property is read‑only; modifications to the returned list do not affect internal state. No exceptions are thrown by the getter.

### Property: `Dictionary<string, object> Data`
Gets a dictionary of arbitrary data items that have been attached to the notification during pipeline processing. Keys are strings; values are objects. The dictionary is read‑only; attempts to modify it result in a `NotSupportedException`. No exceptions are thrown by the getter for read access.

### Property: `bool Success`
Gets a flag indicating whether the pipeline completed without encountering fatal errors. This is distinct from `IsValid`; `Success` reflects overall execution status, while `IsValid` reflects validation outcome. No exceptions are thrown by the getter.

### Property: `DeploymentNotification? ProcessedNotification`
Gets the notification after it has been potentially modified by pipeline steps. If the pipeline does not alter the notification, this property returns the same instance as `Notification`. The property can be `null` if a step explicitly sets it to null. No exceptions are thrown by the getter.

### Property: `ValidationProcessor ValidationProcessor`
Gets the validation processor instance associated with the pipeline. This property provides access to the component responsible for evaluating the notification against validation rules. The property is read‑only. No exceptions are thrown by the getter.

### Method: `Task ProcessAsync()`
Executes the pipeline’s processing steps without returning a detailed result. The method returns a `Task` that completes when all steps have finished. Throws `InvalidOperationException` if the pipeline has already been executed or is misconfigured.

### Method: `async Task ProcessAsync()`
An asynchronous overload of `ProcessAsync` that allows the caller to `await` completion. Functionally equivalent to the non‑async `Task ProcessAsync()` overload; the async version is provided for convenience when additional asynchronous work is needed after awaiting. Throws the same exceptions as the non‑async version.

### Property: `FilterProcessor FilterProcessor`
Gets the filter processor instance associated with the pipeline. This property provides access to the component responsible for determining whether the notification should proceed through subsequent steps based on filter criteria. The property is read‑only. No exceptions are thrown by the getter.

## Usage

### Basic pipeline creation and execution
```csharp
var pipeline = new NotificationPipeline()
    .Use()          // add validation step
    .Use();         // add filtering step

var result = await pipeline.ExecuteAsync();

if (result.Success && pipeline.IsValid)
{
    // notification is ready for further use
    var processed = pipeline.ProcessedNotification;
}
else
{
    foreach (var error in pipeline.Errors)
    {
        Console.Error.WriteLine(error);
    }
}
```

### Using the pipeline for side‑effect processing only
```csharp
var pipeline = new NotificationPipeline()
    .Use(); // configure a single processing step

// Fire‑and‑forget style; we only care that processing completes
await pipeline.ProcessAsync();

// Inspect any collected data or errors after execution
var data = pipeline.Data;
if (!pipeline.IsValid)
{
    // handle validation failures
}
```

## Notes

- The pipeline is **not thread‑safe**. Concurrent calls to `ExecuteAsync` or `ProcessAsync` on the same instance may lead to undefined behavior. Create a separate `NotificationPipeline` instance per thread or per invocation.
- Calling `Use` after the pipeline has already been executed (i.e., after `ExecuteAsync` or `ProcessAsync` has completed) will throw an `InvalidOperationException`.
- The `Errors` list is populated only during validation steps; processing steps do not modify it unless they explicitly add validation failures.
- The `Data` dictionary is mutable only by internal pipeline steps; external code receives a read‑only view. Attempts to cast the returned dictionary to a mutable type and modify it will result in a `NotSupportedException`.
- If a processing step sets `ProcessedNotification` to `null`, subsequent steps will receive a `null` reference; it is the responsibility of step authors to handle this case appropriately.
