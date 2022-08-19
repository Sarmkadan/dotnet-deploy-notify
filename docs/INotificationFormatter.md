# INotificationFormatter
The `INotificationFormatter` interface defines how a deployment notification is serialized into a string payload, specifies the resulting content type, and controls whether emoji characters are included in the output. Implementations are used by the notification pipeline to produce the final message sent to chat or messaging services.

## API
### Format
```csharp
public string Format { get; }
```
Returns the formatted notification content as a string. The implementation uses any internal state (such as template data or configuration) to produce the final message.  
- **Parameters:** None.  
- **Return value:** A string containing the ready‑to‑send notification; may be empty or null if no content can be generated.  
- **Exceptions:**  
  - `InvalidOperationException` – thrown when the formatter has not been properly initialized or required data is missing.

### GetContentType
```csharp
public string GetContentType()
```
Provides the MIME type that describes the format of the string returned by `Format`.  
- **Parameters:** None.  
- **Return value:** A string such as `"application/json"` or `"text/plain"` indicating the content type.  
- **Exceptions:**  
  - `NotSupportedException` – thrown if the formatter cannot determine a suitable content type for its current configuration.

### EnableEmojis
```csharp
public bool EnableEmojis { get; set; }
```
Gets or sets a flag indicating whether emoji characters should be included in the formatted output. When `true`, implementations may insert emojis; when `false`, they must strip or replace them with plain‑text equivalents.  
- **Parameters:** None.  
- **Return value:** `true` if emojis are enabled, `false` otherwise.  
- **Exceptions:** Setting the property does not throw exceptions for valid boolean values.

### CreateFormatter
```csharp
public static INotificationFormatter CreateFormatter()
```
Factory method that produces a default implementation of `INotificationFormatter`.  
- **Parameters:** None.  
- **Return value:** A new instance ready for use.  
- **Exceptions:**  
  - `InvalidOperationException` – thrown if the required dependencies for the default formatter are unavailable or misconfigured.

## Usage
```csharp
// Example 1: Basic formatting without emojis
INotificationFormatter formatter = NotificationFormatter.CreateFormatter();
// Assume formatter is configured elsewhere with template data
string payload = formatter.Format;
string contentType = formatter.GetContentType();
// Send payload to the webhook using the indicated content type
```
```csharp
// Example 2: Enabling emojis and handling potential errors
try {
    INotificationFormatter formatter = NotificationFormatter.CreateFormatter();
    formatter.EnableEmojis = true; // Ask for emoji‑rich output
    string message = formatter.Format;
    string type = formatter.GetContentType();
    // Proceed with sending...
} catch (InvalidOperationException ex) {
    // Log configuration problems and fall back to a plain text notification
    Console.Error.WriteLine($"Formatter initialization failed: {ex.Message}");
}
```

## Notes
- The `Format` property may return `null` or an empty string if the underlying template yields no output; callers should treat this as a “no‑op” notification.  
- `GetContentType` is expected to return a non‑null, valid MIME type; returning `null` indicates a misconfiguration and will result in a `NotSupportedException`.  
- Changing `EnableEmojis` after a call to `Format` does not affect the already‑generated string; the flag only influences subsequent calls.  
- The interface itself imposes no thread‑safety guarantees. Implementations that hold mutable state should either be immutable after construction or provide their own synchronization. Instances returned by `CreateFormatter` are stateless and safe to use concurrently across threads.
