# SearchCriteriaExtensionsJsonExtensions

This class provides JSON serialization and deserialization for search criteria extension metadata, representing type information (Type, Namespace, Assembly) and associated methods. It includes static methods for converting to/from JSON and a TryFromJson pattern for safe parsing.

## API

### Static Methods

#### `public static string ToJson(SearchCriteriaExtensionsMetadata? metadata)`
- **Purpose**: Serializes a `SearchCriteriaExtensionsMetadata` object to its JSON representation.
- **Parameters**: `metadata` – the metadata object to serialize; can be null.
- **Returns**: A JSON string. If `metadata` is null, returns the JSON literal `null`.
- **Throws**: None.

#### `public static SearchCriteriaExtensionsMetadata? FromJson(string json)`
- **Purpose**: Deserializes a JSON string into a `SearchCriteriaExtensionsMetadata` object.
- **Parameters**: `json` – a JSON string representing the metadata.
- **Returns**: A populated `SearchCriteriaExtensionsMetadata` instance, or null if the JSON is null or cannot be deserialized.
- **Throws**: `System.Text.Json.JsonException` if the JSON is malformed or does not match the expected schema.

#### `public static bool TryFromJson(string json, out SearchCriteriaExtensionsMetadata? result)`
- **Purpose**: Attempts to deserialize a JSON string without throwing exceptions.
- **Parameters**: `json` – the JSON string; `result` – when this method returns, contains the deserialized object or null.
- **Returns**: `true` if deserialization succeeded; `false` otherwise.
- **Throws**: None.

### Instance Properties

#### `public string? Type { get; set; }`
- Gets or sets the fully qualified type name (e.g., `"MyNamespace.MyClass"`).

#### `public string? Namespace { get; set; }`
- Gets or sets the namespace of the type.

#### `public string? Assembly { get; set; }`
- Gets or sets the assembly name containing the type.

#### `public string[]? Methods { get; set; }`
- Gets or sets an array of method names associated with the search criteria extension.

## Usage

**Example 1: Serialize and deserialize metadata.**

```csharp
var metadata = new SearchCriteriaExtensionsJsonExtensions
{
    Type = "MyApp.Services.SearchService",
    Namespace = "MyApp.Services",
    Assembly = "MyApp",
    Methods = new[] { "Find", "Search" }
};

string json = SearchCriteriaExtensionsJsonExtensions.ToJson(metadata);
Console.WriteLine(json);
// Output: {"Type":"MyApp.Services.SearchService","Namespace":"MyApp.Services","Assembly":"MyApp","Methods":["Find","Search"]}

var restored = SearchCriteriaExtensionsJsonExtensions.FromJson(json);
Console.WriteLine(restored?.Type); // MyApp.Services.SearchService
```

**Example 2: Safe deserialization with TryFromJson.**

```csharp
string invalidJson = "{ \"Type\": 123 }"; // invalid type value
if (SearchCriteriaExtensionsJsonExtensions.TryFromJson(invalidJson, out var result))
{
    Console.WriteLine($"Deserialized: {result?.Type}");
}
else
{
    Console.WriteLine("Failed to deserialize JSON.");
}
```

## Notes

- **Thread safety**: The static methods are thread-safe as they do not modify shared state. Instance properties are not inherently thread-safe; concurrent reads and writes to the same instance should be synchronized externally.
- **Edge cases**: When serializing a null metadata object, `ToJson` returns the JSON literal `null`. When deserializing, `FromJson` returns null for a JSON null input. `TryFromJson` returns false for null or malformed JSON without throwing.
- The properties are nullable; ensure null checks when accessing them after deserialization, especially if the JSON omitted a field.
- The `Methods` property is an array; if the JSON contains a null value for methods, it will be deserialized as null, not an empty array.
