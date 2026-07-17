# ObjectExtensionsJsonExtensions

`ObjectExtensionsJsonExtensions` is a data class that captures metadata about a type extension—specifically its type name, namespace, assembly, and a list of method names—and provides static methods for serializing and deserializing instances to and from JSON. It is designed for use in configuration, logging, or inter-process communication where extension metadata must be persisted or transmitted as a lightweight JSON payload.

## API

### `public static string ToJson(ObjectExtensionsMetadata metadata)`

Serializes the provided `ObjectExtensionsMetadata` instance to its JSON representation.

- **Parameters**  
  `metadata` – The metadata object to serialize. Must not be `null`.

- **Returns**  
  A JSON string representing the metadata.

- **Throws**  
  `ArgumentNullException` if `metadata` is `null`.  
  `JsonSerializationException` if the object cannot be serialized (e.g., due to circular references or unsupported types in the `Methods` array).

### `public static ObjectExtensionsMetadata? FromJson(string json)`

Deserializes a JSON string into an `ObjectExtensionsMetadata` instance.

- **Parameters**  
  `json` – A valid JSON string representing the metadata. Must not be `null` or empty.

- **Returns**  
  An `ObjectExtensionsMetadata` instance if deserialization succeeds; otherwise `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  `JsonReaderException` if the JSON is malformed.

### `public static bool TryFromJson(string json, [NotNullWhen(true)] out ObjectExtensionsMetadata? metadata)`

Attempts to deserialize a JSON string into an `ObjectExtensionsMetadata` instance without throwing exceptions.

- **Parameters**  
  `json` – A JSON string to deserialize. May be `null` or empty.  
  `metadata` – When this method returns `true`, contains the deserialized metadata; otherwise `null`.

- **Returns**  
  `true` if deserialization succeeded; `false` otherwise.

- **Throws**  
  None. All parsing errors are silently caught and result in a `false` return.

### `public string? Type`

Gets or sets the fully qualified type name of the extension (e.g., `"MyApp.Extensions.LoggingExtension"`).

### `public string? Namespace`

Gets or sets the namespace of the extension type.

### `public string? Assembly`

Gets or sets the assembly name (without version) where the extension type is defined.

### `public string[]? Methods`

Gets or sets an array of method names exposed by the extension. May be `null` if no methods are defined.

## Usage

### Example 1 – Serialize and deserialize metadata

```csharp
using DotNetDeployNotify;

var metadata = new ObjectExtensionsMetadata
{
    Type = "MyApp.Extensions.HealthCheckExtension",
    Namespace = "MyApp.Extensions",
    Assembly = "MyApp.Extensions",
    Methods = new[] { "CheckHealth", "GetStatus" }
};

// Serialize to JSON
string json = ObjectExtensionsJsonExtensions.ToJson(metadata);
Console.WriteLine(json);
// Output: {"Type":"MyApp.Extensions.HealthCheckExtension","Namespace":"MyApp.Extensions","Assembly":"MyApp.Extensions","Methods":["CheckHealth","GetStatus"]}

// Deserialize back
var deserialized = ObjectExtensionsJsonExtensions.FromJson(json);
if (deserialized != null)
{
    Console.WriteLine(deserialized.Type); // MyApp.Extensions.HealthCheckExtension
}
```

### Example 2 – Safe deserialization with TryFromJson

```csharp
using DotNetDeployNotify;

string invalidJson = "{ \"Type\": \"Broken\" "; // missing closing brace

if (ObjectExtensionsJsonExtensions.TryFromJson(invalidJson, out var metadata))
{
    Console.WriteLine($"Deserialized type: {metadata.Type}");
}
else
{
    Console.WriteLine("Failed to deserialize metadata. Using defaults.");
    metadata = new ObjectExtensionsMetadata
    {
        Type = "FallbackExtension",
        Namespace = "Fallback",
        Assembly = "Fallback",
        Methods = null
    };
}
```

## Notes

- **Null handling** – All instance properties (`Type`, `Namespace`, `Assembly`, `Methods`) are nullable. A `null` value for `Methods` is equivalent to an empty method list; serialization will omit the property or write `null` depending on the JSON serializer settings used internally.
- **Empty or malformed JSON** – `FromJson` returns `null` for any deserialization failure, while `TryFromJson` returns `false` without throwing. Both methods treat `null` or empty string input as a failure condition.
- **Thread safety** – Instance members are not inherently thread-safe. Concurrent reads and writes to the same `ObjectExtensionsMetadata` object should be synchronized externally. The static methods `ToJson`, `FromJson`, and `TryFromJson` are thread-safe as they operate only on their parameters and do not modify shared state.
- **Serialization contract** – The JSON structure produced by `ToJson` is compatible with the deserialization expected by `FromJson` and `TryFromJson`. Adding or removing properties in future versions may break backward compatibility unless handled with version-tolerant serialization settings.
