# DryRunRenderResult

The `DryRunRenderResult` type encapsulates the outcome of a simulated notification rendering operation within the `dotnet-deploy-notify` pipeline. It provides a comprehensive snapshot of how a specific notification would be constructed for a given channel without actually transmitting data to external services. This structure is primarily used for validation, debugging, and previewing payloads to ensure configuration correctness before enabling live deployments.

## API

### `Channel`
```csharp
public NotificationChannel Channel { get; }
```
Retrieves the specific notification channel (e.g., Slack, Teams, Email) associated with this render result. This property identifies the target platform for which the payload was generated.

### `ConfigurationId`
```csharp
public string ConfigurationId { get; }
```
Gets the unique identifier of the channel configuration used during the rendering process. This allows tracing the result back to specific settings in the deployment configuration file.

### `DisplayName`
```csharp
public string DisplayName { get; }
```
Returns the human-readable name assigned to this channel configuration. This is useful for logging and UI display when presenting dry-run results to operators.

### `TargetUrl`
```csharp
public string TargetUrl { get; }
```
Provides the resolved endpoint URL where the notification would be sent. If the channel uses a dynamic webhook or a routed address, this property reflects the final computed destination.

### `RenderedPayload`
```csharp
public string RenderedPayload { get; }
```
Contains the fully serialized message payload (typically JSON or XML) ready for transmission. This string represents the exact body that would be included in the HTTP request if the operation were not a dry run.

### `WouldSend`
```csharp
public bool WouldSend { get; }
```
Indicates whether the notification pipeline determined that a request should be dispatched. A value of `false` implies that logic filters (such as event type matching or suppression rules) prevented the send action, even if rendering succeeded.

### `SkipReason`
```csharp
public string? SkipReason { get; }
```
If `WouldSend` is `false`, this property contains a descriptive message explaining why the notification was suppressed. If the notification would have been sent, this property returns `null`.

### `DryRunRenderer`
```csharp
public DryRunRenderer { get; }
```
Exposes the renderer instance responsible for generating this result. This allows access to the underlying logic or state used during the rendering phase if further introspection is required.

### `Render`
```csharp
public DryRunRenderResult Render { get; }
```
Returns a single `DryRunRenderResult` instance representing the primary render outcome. In contexts where this property is accessed on a collection or aggregate, it typically returns the current instance or the first valid result in a set.

### `RenderAll`
```csharp
public IReadOnlyList<DryRunRenderResult> RenderAll { get; }
```
Retrieves a read-only list of all render results generated across multiple channels or variations for the current context. This is useful when evaluating the collective outcome of a broadcast operation.

## Usage

### Example 1: Validating Payload Structure Before Deployment
This example demonstrates iterating through dry-run results to inspect the generated JSON payload and verify that dynamic variables were correctly substituted.

```csharp
using DotNetDeployNotify;

public void ValidatePayloads(IEnumerable<DryRunRenderResult> results)
{
    foreach (var result in results)
    {
        if (!result.WouldSend)
        {
            Console.WriteLine($"[{result.DisplayName}] Skipped: {result.SkipReason}");
            continue;
        }

        Console.WriteLine($"--- Channel: {result.DisplayName} ({result.Channel}) ---");
        Console.WriteLine($"Target: {result.TargetUrl}");
        Console.WriteLine($"Payload Preview: {result.RenderedPayload.Substring(0, Math.Min(100, result.RenderedPayload.Length))}...");
        
        // Assert specific content exists in the rendered payload
        if (!result.RenderedPayload.Contains("deployment_success"))
        {
            throw new InvalidOperationException($"Critical tag missing in {result.ConfigurationId}");
        }
    }
}
```

### Example 2: Auditing Suppression Logic
This example focuses on diagnosing why certain notifications were not scheduled to send by analyzing the `SkipReason` property.

```csharp
using System.Linq;
using DotNetDeployNotify;

public void AuditSuppressions(DryRunRenderResult aggregateResult)
{
    var allResults = aggregateResult.RenderAll;
    var suppressed = allResults.Where(r => !r.WouldSend).ToList();

    if (!suppressed.Any())
    {
        Console.WriteLine("All channels eligible for sending.");
        return;
    }

    foreach (var item in suppressed)
    {
        Console.WriteLine($"Configuration '{item.ConfigurationId}' ({item.DisplayName}) was suppressed.");
        Console.WriteLine($"Reason: {item.SkipReason}");
        
        // Log detailed diagnostic info based on the renderer state if necessary
        var rendererType = item.DryRunRenderer.GetType().Name;
        Console.WriteLine($"Renderer used: {rendererType}");
    }
}
```

## Notes

*   **Null Safety**: The `SkipReason` property is nullable (`string?`). Consumers must check `WouldSend` before accessing `SkipReason` to avoid logic errors, though accessing a `null` skip reason on a successful send is expected behavior.
*   **Immutability**: The `RenderAll` property returns an `IReadOnlyList<T>`, indicating that the collection of results cannot be modified through this reference. This ensures the integrity of the dry-run snapshot during analysis.
*   **Thread Safety**: As `DryRunRenderResult` acts as a data transfer object containing only getters and no internal mutable state, instances are inherently thread-safe for read operations. Multiple threads may safely access properties like `RenderedPayload` or `TargetUrl` concurrently.
*   **Empty Payloads**: A `WouldSend` value of `true` does not guarantee that `RenderedPayload` is non-empty; it only indicates that the pipeline logic permitted the send. Consumers should validate the content length if the downstream service requires a non-zero body.
*   **Renderer Context**: The `DryRunRenderer` property exposes the engine used. While useful for debugging, reliance on specific renderer implementations may couple consumer code to internal library details subject to change.
