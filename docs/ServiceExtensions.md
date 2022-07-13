# ServiceExtensions

`ServiceExtensions` provides utility methods and properties for analyzing and manipulating `DeploymentNotification` instances within the `dotnet-deploy-notify` project. These extensions facilitate common operations such as determining deployment criticality, environment compatibility, and generating descriptive or compact string representations of notifications. The class is designed to centralize logic related to deployment status evaluation and metadata handling.

## API

### Properties

#### `IsCritical`
```csharp
public static bool IsCritical { get; }
```
Indicates whether the associated deployment notification represents a critical deployment. This property evaluates internal state or configuration to determine criticality, typically influencing retry behavior and notification urgency.

#### `IsProduction`
```csharp
public static bool IsProduction { get; }
```
Determines if the deployment notification targets a production environment. Used to differentiate between staging, testing, and production deployments for conditional logic.

#### `SupportsStatus`
```csharp
public static bool SupportsStatus { get; }
```
Checks whether the deployment notification includes status information. Returns `true` if the notification contains a valid status field that can be processed.

#### `SupportsEnvironment`
```csharp
public static bool SupportsEnvironment { get; }
```
Evaluates if the notification includes environment metadata. Returns `true` if environment details are present and parseable.

---

### Methods

#### `GetDescription(DeploymentNotification notification)`
```csharp
public static string GetDescription(DeploymentNotification notification)
```
Generates a human-readable description of the deployment notification. Uses default formatting rules to summarize deployment details.

- **Parameters**:  
  `notification` – The `DeploymentNotification` instance to describe.
- **Returns**:  
  A formatted string describing the deployment.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `GetDescription(DeploymentNotification notification, string format)`
```csharp
public static string GetDescription(DeploymentNotification notification, string format)
```
Produces a description using a custom format string. Allows callers to define how deployment fields are rendered.

- **Parameters**:  
  `notification` – The `DeploymentNotification` instance.  
  `format` – A format string specifying the output structure.
- **Returns**:  
  A formatted string based on `format`.
- **Exceptions**:  
  `ArgumentNullException` if `notification` or `format` is `null`.

---

#### `GetDescription(DeploymentStatus status)`
```csharp
public static string GetDescription(DeploymentStatus status)
```
Returns a description for a given `DeploymentStatus` enum value. Useful for logging or UI rendering.

- **Parameters**:  
  `status` – The `DeploymentStatus` to describe.
- **Returns**:  
  A string representation of `status`.
- **Exceptions**:  
  None.

---

#### `GetDescription(DeploymentStatus status, string format)`
```csharp
public static string GetDescription(DeploymentStatus status, string format)
```
Generates a custom-formatted description for a `DeploymentStatus`. Combines enum value with caller-defined formatting.

- **Parameters**:  
  `status` – The `DeploymentStatus` to describe.  
  `format` – A format string for customizing output.
- **Returns**:  
  A formatted string combining `status` and `format`.
- **Exceptions**:  
  `ArgumentNullException` if `format` is `null`.

---

#### `MergeMetadata(DeploymentNotification target, DeploymentMetadata source)`
```csharp
public static void MergeMetadata(DeploymentNotification target, DeploymentMetadata source)
```
Merges metadata from `source` into `target`, overwriting existing fields where applicable. Used to enrich notifications with additional context.

- **Parameters**:  
  `target` – The `DeploymentNotification` to update.  
  `source` – The `DeploymentMetadata` to merge into `target`.
- **Returns**:  
  `void`.
- **Exceptions**:  
  `ArgumentNullException` if either parameter is `null`.

---

#### `Clone(DeploymentNotification notification)`
```csharp
public static DeploymentNotification Clone(DeploymentNotification notification)
```
Creates a deep copy of the provided `DeploymentNotification`. Ensures modifications to the clone do not affect the original.

- **Parameters**:  
  `notification` – The instance to clone.
- **Returns**:  
  A new `DeploymentNotification` with identical field values.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `ToCompactString(DeploymentNotification notification)`
```csharp
public static string ToCompactString(DeploymentNotification notification)
```
Generates a minimal string representation of the notification, omitting verbose details. Suitable for logging or short identifiers.

- **Parameters**:  
  `notification` – The `DeploymentNotification` to convert.
- **Returns**:  
  A compact string summarizing key fields.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `ToCompactString(DeploymentNotification notification, bool includeEnvironment)`
```csharp
public static string ToCompactString(DeploymentNotification notification, bool includeEnvironment)
```
Produces a compact string with optional environment inclusion. Controls verbosity based on `includeEnvironment`.

- **Parameters**:  
  `notification` – The `DeploymentNotification` to convert.  
  `includeEnvironment` – Whether to include environment details in the output.
- **Returns**:  
  A compact string, conditionally including environment data.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `GetSeverityLevel(DeploymentNotification notification)`
```csharp
public static int GetSeverityLevel(DeploymentNotification notification)
```
Calculates a numerical severity level for the notification. Higher values indicate greater urgency or impact.

- **Parameters**:  
  `notification` – The `DeploymentNotification` to evaluate.
- **Returns**:  
  An integer representing severity (e.g., 0 for low, 3 for critical).
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `ShouldRetry(DeploymentNotification notification)`
```csharp
public static bool ShouldRetry(DeploymentNotification notification)
```
Determines if a failed deployment should be retried based on its status and configuration.

- **Parameters**:  
  `notification` – The `DeploymentNotification` to evaluate.
- **Returns**:  
  `true` if retry is recommended; otherwise `false`.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

#### `GetRetryDelay(DeploymentNotification notification)`
```csharp
public static TimeSpan GetRetryDelay(DeploymentNotification notification)
```
Calculates the delay before the next retry attempt. Uses exponential backoff or fixed intervals based on notification settings.

- **Parameters**:  
  `notification` – The `DeploymentNotification` to evaluate.
- **Returns**:  
  A `TimeSpan` indicating the recommended wait duration.
- **Exceptions**:  
  `ArgumentNullException` if `notification` is `null`.

---

## Usage

### Example 1: Evaluating Critical Deployments
```csharp
var notification = new DeploymentNotification
{
    Status = DeploymentStatus.Failed,
    Environment = "Production",
    IsCritical = true
};

if (ServiceExtensions.IsCritical && ServiceExtensions.ShouldRetry(notification))
{
    var delay = ServiceExtensions.GetRetryDelay(notification);
    Console.WriteLine($"Retrying critical deployment in {delay.TotalSeconds} seconds...");
}
```

### Example 2: Generating Descriptions and Merging Metadata
```csharp
var notification = new DeploymentNotification { Status = DeploymentStatus.Started };
var metadata = new DeploymentMetadata { Region = "us-east-1", Version = "2.1.0" };

ServiceExtensions.MergeMetadata(notification, metadata);
string description = ServiceExtensions.GetDescription(notification, "Deployed {Version} to {Region} ({Status})");
Console.WriteLine(description); // Output: "Deployed 2.1.0 to us-east-1 (Started)"
```

---

## Notes

- **Thread Safety**: All methods are thread-safe provided that input parameters (e.g., `DeploymentNotification`, `DeploymentMetadata`) are not modified concurrently. Since these are static methods with no shared state, thread safety depends entirely on the immutability or synchronization of the passed objects.
- **Null Handling**: All methods throw `ArgumentNullException` if required parameters are `null`. Callers must validate inputs before invocation.
- **Overloads**: Multiple overloads of `GetDescription` and `ToCompactString` allow flexibility in formatting. The `includeEnvironment` parameter in `ToCompactString` enables dynamic verbosity control.
- **Severity Levels**: `GetSeverityLevel` returns integers that may correspond to predefined thresholds (e.g., 0=Informational, 1=Warning, 2=Error, 3=Critical). Exact mappings depend on implementation details not exposed in the public API.
