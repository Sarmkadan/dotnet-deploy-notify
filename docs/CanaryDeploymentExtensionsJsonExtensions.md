# CanaryDeploymentExtensionsJsonExtensions

Provides JSON serialization and deserialization capabilities for `CanaryDeploymentExtensions` type metadata using System.Text.Json. This class enables serialization of extension method class information to JSON format and provides safe deserialization methods that handle malformed input gracefully.

## API

### ToJson(bool indented = false)

Serializes the `CanaryDeploymentExtensions` type metadata to a JSON string representation.

- **Parameters:**
  - `indented` (optional): When `true`, formats the JSON with indentation for readability. When `false` (default), produces compact JSON.
- **Return value:** A JSON string containing the type metadata including the type name, namespace, assembly, and array of method names.
- **Thread safety:** This method is thread-safe.

### FromJson(string json)

Deserializes a JSON string to a `CanaryDeploymentExtensionsMetadata` object.

- **Parameters:**
  - `json`: The JSON string to deserialize.
- **Return value:** A `CanaryDeploymentExtensionsMetadata` object containing the deserialized metadata, or `null` if deserialization fails.
- **Exceptions:**
  - Throws `ArgumentNullException` if `json` is `null`.
- **Error handling:** Catches `JsonException` and returns `null` on failure rather than propagating the exception.

### CanaryDeploymentExtensionsMetadata

A nested class that represents the metadata structure for `CanaryDeploymentExtensions` used in JSON serialization.

- **Properties:**
  - `Type` (`string?`): The type identifier, typically "CanaryDeploymentExtensions".
  - `Namespace` (`string?`): The namespace where the type is defined.
  - `Assembly` (`string?`): The assembly name where the type is defined.
  - `Methods` (`string[]?`): An array of method names available on the `CanaryDeploymentExtensions` class.

### TryFromJson(string json, out CanaryDeploymentExtensionsMetadata? value)

Attempts to deserialize a JSON string to `CanaryDeploymentExtensionsMetadata` and returns a boolean indicating success.

- **Parameters:**
  - `json`: The JSON string to deserialize.
  - `value`: Output parameter that receives the deserialized metadata if successful, or `null` if deserialization fails.
- **Return value:** `true` if deserialization succeeds; `false` otherwise.
- **Exceptions:**
  - Throws `ArgumentNullException` if `json` is `null`.
- **Error handling:** Catches `JsonException` and returns `false` with `value` set to `null` on failure.

## Usage

### Serializing to JSON

```csharp
using DotNetDeployNotify.Core.Models;

// Serialize with compact formatting (default)
string compactJson = CanaryDeploymentExtensionsJsonExtensions.ToJson();
Console.WriteLine(compactJson);

// Serialize with pretty-print formatting
string prettyJson = CanaryDeploymentExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(prettyJson);
```

### Deserializing from JSON

```csharp
using DotNetDeployNotify.Core.Models;

string json = """{
  "type": "CanaryDeploymentExtensions",
  "namespace": "DotNetDeployNotify.Core.Models",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "IsActive",
    "IsPromoted",
    "IsFailedOrAborted",
    "GetTrafficSplitDisplay",
    "CalculateHealthScore",
    "GetStatusSummary",
    "CanPromote",
    "GetNextTrafficPercentage",
    "GetCurrentSoakRemaining",
    "IsCurrentSoakComplete"
  ]
}""";

// Safe deserialization - returns null on failure
var metadata = CanaryDeploymentExtensionsJsonExtensions.FromJson(json);
if (metadata != null)
{
    Console.WriteLine($"Type: {metadata.Type}");
    Console.WriteLine($"Namespace: {metadata.Namespace}");
    Console.WriteLine($"Assembly: {metadata.Assembly}");
    Console.WriteLine($"Methods: {string.Join(", ", metadata.Methods ?? Array.Empty<string>())}");
}

// Try-based deserialization - returns boolean indicating success
if (CanaryDeploymentExtensionsJsonExtensions.TryFromJson(json, out var result))
{
    Console.WriteLine("Deserialization succeeded!");
}
else
{
    Console.WriteLine("Deserialization failed - invalid JSON");
}
```

## Notes

- **Thread safety:** All methods are thread-safe and can be called concurrently from multiple threads.
- **Null handling:** The `FromJson` and `TryFromJson` methods throw `ArgumentNullException` when passed a `null` JSON string, following standard .NET conventions.
- **Error tolerance:** Deserialization methods catch `JsonException` and return `null` or `false` rather than propagating exceptions, making them suitable for parsing untrusted input.
- **Naming policy:** JSON serialization uses camelCase property naming policy by default, consistent with the rest of the codebase.
- **Default options:** The class uses a shared `JsonSerializerOptions` instance configured with `PropertyNamingPolicy.CamelCase`, `WriteIndented = false`, and `DefaultIgnoreCondition.WhenWritingNull` for consistent serialization behavior.
- **Metadata accuracy:** The serialized `Methods` array contains the actual public extension methods defined on `CanaryDeploymentExtensions` class: `IsActive`, `IsPromoted`, `IsFailedOrAborted`, `GetTrafficSplitDisplay`, `CalculateHealthScore`, `GetStatusSummary`, `CanPromote`, `GetNextTrafficPercentage`, `GetCurrentSoakRemaining`, and `IsCurrentSoakComplete`.
- **Immutable output:** The `CanaryDeploymentExtensionsMetadata` class is marked as `sealed` and all properties are read-write, allowing modification after deserialization if needed.
