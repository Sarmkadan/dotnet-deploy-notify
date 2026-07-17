# BuildStatusConverterExtensions

Provides a collection of static extension methods and utility functions for interpreting, converting, and classifying build status values. These members are designed to work with the `BuildStatus` enumeration (or a string representation thereof) and offer common operations such as checking success/failure, retrieving a human-readable name, parsing, priority mapping, CSS class assignment, and severity scoring. The class is intended to be used in notification pipelines and UI rendering where build statuses need to be consistently evaluated and displayed.

## API

### `public static bool IsSuccessful(BuildStatus status)`

Returns `true` if the given `status` represents a successful build outcome (e.g., `Succeeded`, `Passed`). Returns `false` otherwise.

- **Parameters**: `status` – the `BuildStatus` value to evaluate.
- **Returns**: `true` if the status is considered successful; otherwise `false`.
- **Throws**: Nothing.

### `public static bool IsFailed(BuildStatus status)`

Returns `true` if the given `status` represents a failed build outcome (e.g., `Failed`, `Error`). Returns `false` otherwise.

- **Parameters**: `status` – the `BuildStatus` value to evaluate.
- **Returns**: `true` if the status is considered failed; otherwise `false`.
- **Throws**: Nothing.

### `public static bool IsInProgress(BuildStatus status)`

Returns `true` if the given `status` represents an in-progress or pending build state (e.g., `Running`, `Queued`). Returns `false` otherwise.

- **Parameters**: `status` – the `BuildStatus` value to evaluate.
- **Returns**: `true` if the status is considered in progress; otherwise `false`.
- **Throws**: Nothing.

### `public static string GetDisplayName(BuildStatus status)`

Returns a human-readable display name for the given build status. For example, `BuildStatus.Succeeded` might return `"Succeeded"` or a localized variant.

- **Parameters**: `status` – the `BuildStatus` value.
- **Returns**: A non-null, non-empty string representing the display name.
- **Throws**: Nothing.

### `public static BuildStatus ParseStatus(string value)`

Parses the specified string representation of a build status into its corresponding `BuildStatus` enumeration value.

- **Parameters**: `value` – the string to parse. Case-insensitive matching is typically used.
- **Returns**: The `BuildStatus` value that corresponds to the input string.
- **Throws**: `ArgumentException` if `value` is `null`, empty, or does not match any known build status name.

### `public static bool TryParseStatus(string value, out BuildStatus result)`

Attempts to parse the specified string into a `BuildStatus` value without throwing an exception.

- **Parameters**: `value` – the string to parse; `result` – when this method returns, contains the parsed `BuildStatus` value if parsing succeeded, or the default value otherwise.
- **Returns**: `true` if parsing succeeded; `false` otherwise.
- **Throws**: Nothing.

### `public static NotificationPriority GetPriority(BuildStatus status)`

Returns the notification priority level associated with the given build status. For example, a failed status might return `High`, while a successful one returns `Low`.

- **Parameters**: `status` – the `BuildStatus` value.
- **Returns**: A `NotificationPriority` value (e.g., `Low`, `Normal`, `High`, `Critical`).
- **Throws**: Nothing.

### `public static string GetCssClass(BuildStatus status)`

Returns a CSS class name suitable for styling UI elements that display the given build status. The returned string is typically a simple, lowercase identifier (e.g., `"success"`, `"failure"`, `"in-progress"`).

- **Parameters**: `status` – the `BuildStatus` value.
- **Returns**: A non-null string representing the CSS class.
- **Throws**: Nothing.

### `public static bool IsSameAs(BuildStatus status, BuildStatus other)`

Compares two `BuildStatus` values for semantic equality. This may treat different enumeration members that represent the same logical state (e.g., `Succeeded` and `Passed`) as equal.

- **Parameters**: `status` – the first status; `other` – the second status.
- **Returns**: `true` if the two statuses are semantically equivalent; otherwise `false`.
- **Throws**: Nothing.

### `public static int GetSeverity(BuildStatus status)`

Returns a numeric severity score for the given build status. Higher values typically indicate more critical states (e.g., failed builds score higher than successful ones). The exact range is implementation-defined.

- **Parameters**: `status` – the `BuildStatus` value.
- **Returns**: An integer representing the severity.
- **Throws**: Nothing.

## Usage

### Example 1: Parsing and classifying a build status from a CI webhook payload

```csharp
using DotNetDeployNotify.BuildStatus;

string rawStatus = "Failed";
if (BuildStatusConverterExtensions.TryParseStatus(rawStatus, out BuildStatus status))
{
    bool isFailed = BuildStatusConverterExtensions.IsFailed(status);
    string displayName = BuildStatusConverterExtensions.GetDisplayName(status);
    NotificationPriority priority = BuildStatusConverterExtensions.GetPriority(status);
    string cssClass = BuildStatusConverterExtensions.GetCssClass(status);

    Console.WriteLine($"Status: {displayName}, Failed: {isFailed}, Priority: {priority}, CSS: {cssClass}");
}
else
{
    Console.WriteLine($"Unable to parse status: {rawStatus}");
}
```

### Example 2: Using severity and semantic equality in a notification filter

```csharp
using DotNetDeployNotify.BuildStatus;

BuildStatus currentStatus = BuildStatus.Succeeded;
BuildStatus thresholdStatus = BuildStatus.Passed;

// Check if the current status is semantically the same as the threshold
if (BuildStatusConverterExtensions.IsSameAs(currentStatus, thresholdStatus))
{
    Console.WriteLine("Statuses are equivalent.");
}

// Only send notification if severity exceeds a certain level
int severity = BuildStatusConverterExtensions.GetSeverity(currentStatus);
if (severity >= 5)
{
    SendNotification(currentStatus);
}
```

## Notes

- All methods are static and thread-safe. They do not modify any shared state and can be called concurrently from multiple threads without synchronization.
- `ParseStatus` throws `ArgumentException` for invalid input; use `TryParseStatus` when the input may be unreliable (e.g., from external sources).
- The `IsSameAs` method may treat different enumeration values as equal if they represent the same logical outcome (e.g., `Succeeded` and `Passed`). This behavior is implementation-specific and should not be assumed to follow ordinal or numeric equality.
- `GetDisplayName` and `GetCssClass` always return non-null strings. The exact strings may vary by locale or configuration.
- The severity scale returned by `GetSeverity` is not guaranteed to be consistent across different versions of the library. Avoid relying on absolute numeric thresholds unless documented otherwise.
- `IsSuccessful`, `IsFailed`, and `IsInProgress` are mutually exclusive for most status values, but some statuses may not fall into any of these categories (e.g., `Cancelled`). In such cases all three methods return `false`.
