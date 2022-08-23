# ServiceExtensionsJsonExtensions

Provides static methods for serializing and deserializing `ServiceExtensionsMetadata` objects to and from JSON strings, enabling easy storage and transmission of service extension metadata.

## API

### `ToJson`
Serializes a `ServiceExtensionsMetadata` instance into a JSON string.

**Parameters**
- `metadata` (`ServiceExtensionsMetadata?`): The metadata to serialize. May be `null`.

**Returns**
- `string`: A JSON representation of the metadata, or `null` if the input is `null`.

**Throws**
- `System.Text.Json.JsonException`: Thrown if serialization fails.

---

### `FromJson`
Deserializes a JSON string into a `ServiceExtensionsMetadata` instance.

**Parameters**
- `json` (`string`): The JSON string to deserialize.

**Returns**
- `ServiceExtensionsMetadata?`: The deserialized metadata, or `null` if the input is `null`.

**Throws**
- `System.Text.Json.JsonException`: Thrown if the JSON is malformed or incompatible with the target type.

---
### `TryFromJson`
Safely deserializes a JSON string into a `ServiceExtensionsMetadata` instance without throwing exceptions.

**Parameters**
- `json` (`string`): The JSON string to deserialize.

**Returns**
- `bool`: `true` if deserialization succeeds; otherwise, `false`.
- `ServiceExtensionsMetadata?`: The deserialized metadata if successful, otherwise `null`.

---
### `Type` (instance property)
Gets the type name of the service extension.

**Returns**
- `string?`: The type name, or `null` if not set.

---
### `Namespace` (instance property)
Gets the namespace of the service extension.

**Returns**
- `string?`: The namespace, or `null` if not set.

---
### `Assembly` (instance property)
Gets the assembly name of the service extension.

**Returns**
- `string?`: The assembly name, or `null` if not set.

---
### `Methods` (instance property)
Gets the names of the methods exposed by the service extension.

**Returns**
- `string[]?`: An array of method names, or `null` if no methods are exposed.

## Usage

```csharp
// Example 1: Serializing metadata to JSON
var metadata = new ServiceExtensionsMetadata
{
    Type = "EmailNotificationService",
    Namespace = "Acme.Notifications",
    Assembly = "Acme.Notifications.dll",
    Methods = new[] { "SendEmail", "SendSms" }
};

string json = ServiceExtensionsJsonExtensions.ToJson(metadata);
Console.WriteLine(json);
// Output: {"Type":"EmailNotificationService","Namespace":"Acme.Notifications","Assembly":"Acme.Notifications.dll","Methods":["SendEmail","SendSms"]}

// Example 2: Deserializing JSON back to metadata
string inputJson = "{\"Type\":\"SmsNotificationService\",\"Namespace\":\"Acme.Notifications\",\"Assembly\":\"Acme.Notifications.dll\",\"Methods\":[\"SendSms\"]}";
if (ServiceExtensionsJsonExtensions.TryFromJson(inputJson, out var deserialized))
{
    Console.WriteLine($"Type: {deserialized.Type}");
    Console.WriteLine($"Methods: {string.Join(", ", deserialized.Methods)}");
}
// Output:
// Type: SmsNotificationService
// Methods: SendSms
```

## Notes

- **Null Handling**: All methods handle `null` inputs gracefully. `ToJson` returns `null` for `null` input, while `FromJson` and `TryFromJson` return `null` for invalid or `null` JSON.
- **Thread Safety**: The class is stateless and thread-safe. Concurrent calls to any method do not require synchronization.
- **Performance**: `TryFromJson` is preferred when handling untrusted input to avoid exception overhead. `FromJson` should only be used when input validity is guaranteed.
