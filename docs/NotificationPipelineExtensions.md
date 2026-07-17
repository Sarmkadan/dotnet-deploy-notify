# NotificationPipelineExtensions

The `NotificationPipelineExtensions` class provides a set of static extension methods and utility functions designed to streamline the execution, validation, and result handling of deployment notification pipelines within the `dotnet-deploy-notify` ecosystem. It facilitates the asynchronous processing of notification metadata, ensures robust error reporting through validation strings, and offers standardized mechanisms for constructing pipeline results and retrieving processed notification states.

## API

### ExecuteWithMetadataAsync
Executes the notification pipeline asynchronously, incorporating specific metadata into the processing context.
*   **Purpose**: Initiates the full pipeline execution flow with additional contextual data.
*   **Parameters**: Accepts the pipeline context and a metadata object (specific signature depends on the extended target type).
*   **Return Value**: Returns a `Task<PipelineResult>` representing the final outcome of the pipeline execution.
*   **Throws**: May throw exceptions related to pipeline configuration errors or critical failures during asynchronous metadata processing.

### ExecuteSuccessfullyAsync
Executes the pipeline specifically targeting successful deployment scenarios and returns the resulting notification entity.
*   **Purpose**: Runs the pipeline logic filtered for success cases, often used for post-deployment success callbacks.
*   **Parameters**: Accepts the relevant pipeline input context.
*   **Return Value**: Returns a `Task<DeploymentNotification?>`. If the execution yields a valid success notification, it is returned; otherwise, `null` is returned.
*   **Throws**: Throws if the underlying pipeline encounters an unrecoverable error before a result can be determined.

### GetValidationErrors
Retrieves a formatted string containing all validation errors associated with a specific notification or pipeline context.
*   **Purpose**: Aggregates validation failures into a human-readable format for logging or error reporting.
*   **Parameters**: Accepts the target object containing validation state.
*   **Return Value**: Returns a `string` listing the errors. Returns an empty string if no validation errors exist.
*   **Throws**: Does not typically throw; returns an empty string if the input is null or invalid depending on implementation safety.

### IsSuccessful
Evaluates the status of a pipeline result or notification to determine if the operation completed successfully.
*   **Purpose**: Provides a boolean check for success status without inspecting detailed result codes.
*   **Parameters**: Accepts the `PipelineResult` or `DeploymentNotification` instance.
*   **Return Value**: Returns a `bool` (`true` if successful, `false` otherwise).
*   **Throws**: None.

### GetChannelCount
Calculates the number of distinct channels configured or utilized within a specific notification payload.
*   **Purpose**: Determines the scope of distribution for a given notification.
*   **Parameters**: Accepts the `DeploymentNotification` or configuration object.
*   **Return Value**: Returns an `int` representing the total count of channels.
*   **Throws**: None.

### GetOriginalNotification
Retrieves the initial, unprocessed version of a notification from a pipeline result or processing context.
*   **Purpose**: Allows inspection of the raw input before any transformation or enrichment logic was applied.
*   **Parameters**: Accepts the `PipelineResult` or processing wrapper.
*   **Return Value**: Returns a `DeploymentNotification?`. Returns `null` if the original notification is not preserved or available.
*   **Throws**: None.

### GetProcessedNotification
Retrieves the final, transformed version of a notification after pipeline processing is complete.
*   **Purpose**: Accesses the enriched or modified notification ready for dispatch.
*   **Parameters**: Accepts the `PipelineResult` or processing wrapper.
*   **Return Value**: Returns a `DeploymentNotification?`. Returns `null` if processing failed or did not produce an output.
*   **Throws**: None.

### CreateResult (Overload 1)
Factory method to instantiate a new `PipelineResult` based on standard success/failure parameters.
*   **Purpose**: Constructs a standardized result object indicating completion status.
*   **Parameters**: Typically accepts status enums, error messages, and the associated notification entity.
*   **Return Value**: Returns a new `PipelineResult` instance.
*   **Throws**: None.

### CreateResult (Overload 2)
Factory method to instantiate a `PipelineResult` with extended metadata or specific exception handling context.
*   **Purpose**: Constructs a result object including detailed diagnostic information or inner exceptions.
*   **Parameters**: Accepts status indicators, exception objects, and detailed context data.
*   **Return Value**: Returns a new `PipelineResult` instance.
*   **Throws**: None.

## Usage

### Example 1: Executing a Pipeline and Handling Validation
This example demonstrates how to execute a pipeline with metadata, check for validation errors, and retrieve the processed notification if successful.

```csharp
using DotNetDeployNotify.Pipeline;

public async Task ProcessDeploymentAsync(DeploymentContext context)
{
    var metadata = new { Environment = "Production", Trigger = "CI/CD" };
    
    // Execute the pipeline with additional metadata
    var result = await NotificationPipelineExtensions.ExecuteWithMetadataAsync(context, metadata);

    // Check for validation errors immediately
    string errors = NotificationPipelineExtensions.GetValidationErrors(result);
    if (!string.IsNullOrEmpty(errors))
    {
        Console.WriteLine($"Pipeline validation failed: {errors}");
        return;
    }

    if (NotificationPipelineExtensions.IsSuccessful(result))
    {
        var processed = NotificationPipelineExtensions.GetProcessedNotification(result);
        int channelCount = NotificationPipelineExtensions.GetChannelCount(processed);
        
        Console.WriteLine($"Success! Notification sent to {channelCount} channels.");
    }
}
```

### Example 2: Creating Results and Retrieving Original State
This example illustrates creating a custom pipeline result manually and comparing the original notification against the processed one.

```csharp
using DotNetDeployNotify.Pipeline;

public PipelineResult AuditNotificationFlow(DeploymentNotification original, Exception ex)
{
    // Create a failure result using the overload that accepts an exception
    var failureResult = NotificationPipelineExtensions.CreateResult(
        PipelineStatus.Failed, 
        original, 
        ex, 
        "Critical failure during dispatch"
    );

    // Retrieve the original notification from the result for auditing
    var auditOriginal = NotificationPipelineExtensions.GetOriginalNotification(failureResult);
    
    if (auditOriginal != null)
    {
        // Log the state of the original payload before failure
        LogAuditEvent(auditOriginal.Id, "Processing failed");
    }

    return failureResult;
}
```

## Notes

*   **Null Safety**: Methods returning `DeploymentNotification?` (`ExecuteSuccessfullyAsync`, `GetOriginalNotification`, `GetProcessedNotification`) explicitly handle cases where no notification exists. Callers must perform null checks before accessing properties on the returned entities.
*   **Thread Safety**: As this class consists entirely of static methods operating on passed-in parameters without maintaining internal mutable static state, it is inherently thread-safe. However, the thread safety of the `PipelineResult` or `DeploymentNotification` objects passed into these methods depends on the immutability of those specific instances.
*   **Validation Logic**: `GetValidationErrors` aggregates errors but does not trigger re-validation; it reports the current state of the validation context. If the input object has not been validated prior to calling this method, the return value may be empty regardless of the object's actual validity.
*   **Asynchronous Execution**: `ExecuteWithMetadataAsync` and `ExecuteSuccessfullyAsync` are asynchronous operations. They should always be awaited to prevent blocking threads and to ensure exceptions are propagated correctly to the caller's context.
*   **Result Creation**: The two `CreateResult` overloads allow for flexible result construction. Ensure the correct overload is selected based on whether detailed exception context is required, as mixing parameters between overloads may lead to compilation errors or unintended default values.
