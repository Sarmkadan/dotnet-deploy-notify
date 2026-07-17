# CacheEntryExtensionsJsonExtensions

Provides JSON serialization and deserialization extensions for `CacheEntryExtensionsMetadata` objects, enabling conversion to and from JSON strings for storage or transmission.

## API

### `ToJson`
Serializes a `CacheEntryExtensionsMetadata` instance into a JSON string.

- **Parameters**
  - `metadata` – The metadata object to serialize.
- **Return value**
  - A JSON string representation of the metadata.
- **Exceptions**
  - Throws `ArgumentNullException` if `metadata` is `null`.

### `FromJson`
Deserializes a JSON string into a `CacheEntryExtensionsMetadata` object.

- **Parameters**
  - `json` – The JSON string to deserialize.
- **Return value**
  - The deserialized `CacheEntryExtensionsMetadata` instance.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.
  - Throws `JsonException` if the JSON is malformed or incompatible with the expected schema.

### `TryFromJson`
Attempts to deserialize a JSON string into a `CacheEntryExtensionsMetadata` object without throwing exceptions.

- **Parameters**
  - `json` – The JSON string to deserialize.
  - `result` – Output parameter receiving the deserialized metadata on success.
- **Return value**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - None.

### `Type` (instance property of `CacheEntryExtensionsMetadata`)
Gets the type name of the extension.

- **Return value**
  - The type name as a string, or `null` if not set.

### `Namespace` (instance property of `CacheEntryExtensionsMetadata`)
Gets the namespace of the extension.

- **Return value**
  - The namespace as a string, or `null` if not set.

### `Assembly` (instance property of `CacheEntryExtensionsMetadata`)
Gets the assembly name of the extension.

- **Return value**
  - The assembly name as a string, or `null` if not set.

### `Methods` (instance property of `CacheEntryExtensionsMetadata`)
Gets the method names exposed by the extension.

- **Return value**
  - An array of method names, or `null` if no methods are defined.

## Usage

```csharp
// Serialize metadata to JSON
var metadata = new CacheEntryExtensionsMetadata
{
    Type = "MyExtension",
    Namespace = "MyApp.Extensions",
    Assembly = "MyApp",
    Methods = new[] { "Method1", "Method2" }
};
string json = CacheEntryExtensionsJsonExtensions.ToJson(metadata);

// Deserialize JSON back to metadata
CacheEntryExtensionsMetadata? deserialized = CacheEntryExtensionsJsonExtensions.FromJson(json);

// Safe deserialization with error handling
if (CacheEntryExtensionsJsonExtensions.TryFromJson(json, out var safeResult))
{
    Console.WriteLine($"Deserialized: {safeResult.Type}");
}
```

## Notes

- The JSON format assumes UTF-8 encoding and standard `System.Text.Json` conventions.
- Thread safety is guaranteed for all methods, as they operate on immutable inputs or return new objects.
- Deserialization methods (`FromJson`, `TryFromJson`) may fail if the JSON schema does not match the expected structure of `CacheEntryExtensionsMetadata`.
- Empty or whitespace JSON strings passed to `FromJson` or `TryFromJson` will result in a `JsonException` or `false`, respectively.
