# MathExtensionsJsonExtensions

Utility class providing JSON serialization and deserialization for `MathExtensionsMetadata` objects, enabling portable representation of mathematical extension method metadata across application domains or serialization boundaries.

## API

### `public static string ToJson(MathExtensionsMetadata metadata)`

Serializes a `MathExtensionsMetadata` instance into a compact JSON string.

- **Parameters**
  - `metadata`: The metadata object to serialize.
- **Return value**
  - A JSON string representing the metadata.
- **Exceptions**
  - Throws `ArgumentNullException` if `metadata` is `null`.

### `public static MathExtensionsMetadata? FromJson(string json)`

Deserializes a JSON string into a `MathExtensionsMetadata` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Return value**
  - The deserialized `MathExtensionsMetadata` instance, or `null` if deserialization fails.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.

### `public static bool TryFromJson(string json, [NotNullWhen(true)] out MathExtensionsMetadata? metadata)`

Attempts to deserialize a JSON string into a `MathExtensionsMetadata` instance, safely handling malformed input.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `metadata`: Receives the deserialized metadata on success.
- **Return value**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.

### `public string? Type`

Gets or sets the fully qualified type name of the mathematical extension class (e.g., `"System.Math"`).

### `public string? Namespace`

Gets or sets the namespace of the mathematical extension class.

### `public string? Assembly`

Gets or sets the assembly name housing the mathematical extension class.

### `public string[]? Methods`

Gets or sets an array of method names exposed by the mathematical extension class.

## Usage

```csharp
// Example 1: Serializing metadata to JSON
var metadata = new MathExtensionsMetadata
{
    Type = "System.Math",
    Namespace = "System",
    Assembly = "System.Runtime",
    Methods = new[] { "Abs", "Max", "Min", "Round" }
};

string json = MathExtensionsJsonExtensions.ToJson(metadata);

// Example 2: Deserializing metadata from JSON
if (MathExtensionsJsonExtensions.TryFromJson(json, out var deserialized))
{
    Console.WriteLine($"Type: {deserialized.Type}");
    Console.WriteLine($"Methods: {string.Join(", ", deserialized.Methods)}");
}
```

## Notes

- **Thread safety**: All methods are thread-safe and may be called concurrently from multiple threads.
- **Null handling**: `ToJson` throws on `null` input; `FromJson` and `TryFromJson` accept `null` JSON strings, treating them as invalid input and returning `null` or `false` respectively.
- **Malformed JSON**: `FromJson` returns `null` for invalid JSON; `TryFromJson` returns `false` without throwing.
- **Metadata validity**: The class does not validate the contents of the `Methods` array or other properties during serialization/deserialization; callers must ensure semantic correctness.
