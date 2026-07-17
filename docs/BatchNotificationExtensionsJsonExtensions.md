# BatchNotificationExtensionsJsonExtensions

Provides JSON serialization and deserialization support for `BatchNotificationExtensionsMetadata` objects, allowing the metadata to be persisted, transmitted, or inspected as a JSON string.

## API

### `public static string ToJson(BatchNotificationExtensionsMetadata value)`
Serializes a `BatchNotificationExtensionsMetadata` instance to its JSON representation.

- **Parameters**  
  - `value`: The metadata object to serialize. Must not be `null`.
- **Return value**  
  - A JSON‑encoded string representing the supplied metadata.
- **Exceptions**  
  - `ArgumentNullException` if `value` is `null`.  
  - `JsonException` if serialization fails (e.g., due to unsupported member types).

### `public static BatchNotificationExtensionsMetadata? FromJson(string json)`
Deserializes a JSON string into a `BatchNotificationExtensionsMetadata` instance.

- **Parameters**  
  - `json`: The JSON string to parse. Must not be `null`.
- **Return value**  
  - The deserialized metadata object, or `null` if the JSON represents a null value.
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed or does not correspond to the expected metadata structure.

### `public static bool TryFromJson(string json, [NotNullWhen(true)] out BatchNotificationExtensionsMetadata? result)`
Attempts to parse a JSON string into a `BatchNotificationExtensionsMetadata` instance without throwing exceptions on failure.

- **Parameters**  
  - `json`: The JSON string to parse. May be `null`.  
  - `result`: Receives the deserialized metadata if parsing succeeds; otherwise receives `null`.
- **Return value**  
  - `true` if `json` was successfully parsed; otherwise `false`.
- **Exceptions**  
  - None. All error conditions are reported via the return value.

### `public string? Type`
Gets the CLR type name of the extension described by the metadata.

- **Return value**  
  - The simple name of the type (e.g., `BatchNotificationExtensions`), or `null` if not set.

### `public string? Namespace`
Gets the namespace of the extension type.

- **Return value**  
  - The namespace string (e.g., `MyApp.Notifications`), or `null` if not set.

### `public string? Assembly`
Gets the assembly name that contains the extension type.

- **Return value**  
  - The simple assembly name (e.g., `MyApp.Notifications`), or `null` if not set.

### `public string[]? Methods`
Gets the names of the methods exposed by the extension.

- **Return value**  
  - An array of method names, or `null` if no method information is available.

## Usage

```csharp
using DotNetDeployNotify.Json;

// Create a metadata instance (example)
var metadata = new BatchNotificationExtensionsMetadata
{
    Type = "BatchNotificationExtensions",
    Namespace = "MyApp.Notifications",
    Assembly = "MyApp.Notifications",
    Methods = new[] { "Send", "Flush" }
};

// Serialize to JSON
string json = BatchNotificationExtensionsJsonExtensions.ToJson(metadata);
// json now contains something like:
// {"Type":"BatchNotificationExtensions","Namespace":"MyApp.Notifications","Assembly":"MyApp.Notifications","Methods":["Send","Flush"]}

// Deserialize from JSON
BatchNotificationExtensionsMetadata? parsed =
    BatchNotificationExtensionsJsonExtensions.FromJson(json);
// parsed contains the original values
```

```csharp
using DotNetDeployNotify.Json;

// Safely attempt to parse JSON that may be invalid
string maybeJson = GetJsonFromSomewhere(); // could be null or malformed
if (BatchNotificationExtensionsJsonExtensions.TryFromJson(maybeJson, out var metadata))
{
    // Use metadata.Type, metadata.Namespace, etc.
    Console.WriteLine($"Extension: {metadata.Namespace}.{metadata.Type}");
}
else
{
    // Handle the error case – log, fallback, etc.
    Console.WriteLine("Failed to parse notification extensions metadata.");
}
```

## Notes

- The static JSON methods do not retain any internal state; they are thread‑safe and can be invoked concurrently from multiple threads.
- Instance properties (`Type`, `Namespace`, `Assembly`, `Methods`) are read‑only after the object is constructed; modifying them requires creating a new instance.
- Passing `null` to the static serialization or deserialization methods will result in an `ArgumentNullException`. The `TryFromJson` method treats a `null` input as a failure and returns `false` without throwing.
- If the JSON contains extra properties not defined in `BatchNotificationExtensionsMetadata`, they are ignored during deserialization; missing properties result in their respective fields being set to `null` (or default for value types).  
- The `Methods` array, if present, is expected to contain only non‑empty, unique strings; duplicates or empty entries are preserved as‑is because the extensions perform no validation beyond JSON mapping.  
- These extensions rely on the default `System.Text.Json` serializer settings; custom converters or options are not applied. If specialized serialization behavior is required, wrap the calls with appropriate `JsonSerializerOptions`.
