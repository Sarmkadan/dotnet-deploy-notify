# GuardExtensionsJsonExtensions
The `GuardExtensionsJsonExtensions` type provides a set of methods for serializing and deserializing `GuardExtensions` instances to and from JSON. This allows for easy storage, transmission, and reconstruction of `GuardExtensions` data in a platform-agnostic format.

## API
* `public static string ToJson`: Serializes a `GuardExtensions` instance into a JSON string. Parameters: none (extension method). Return value: a JSON string representation of the `GuardExtensions` instance. Throws: `JsonSerializationException` if serialization fails.
* `public static GuardExtensionsMetadata? FromJson`: Deserializes a JSON string into a `GuardExtensionsMetadata` instance. Parameters: `string json`. Return value: a `GuardExtensionsMetadata` instance, or `null` if deserialization fails. Throws: `JsonDeserializationException` if deserialization fails.
* `public static bool TryFromJson`: Attempts to deserialize a JSON string into a `GuardExtensionsMetadata` instance. Parameters: `string json`, `out GuardExtensionsMetadata? metadata`. Return value: `true` if deserialization succeeds, `false` otherwise. Throws: none.
* `public string? Type`: Gets the type of the `GuardExtensions` instance. Parameters: none. Return value: the type as a string, or `null` if not set. Throws: none.
* `public string? Namespace`: Gets the namespace of the `GuardExtensions` instance. Parameters: none. Return value: the namespace as a string, or `null` if not set. Throws: none.
* `public string? Assembly`: Gets the assembly of the `GuardExtensions` instance. Parameters: none. Return value: the assembly as a string, or `null` if not set. Throws: none.
* `public string[]? Methods`: Gets the methods of the `GuardExtensions` instance. Parameters: none. Return value: an array of method names as strings, or `null` if not set. Throws: none.

## Usage
```csharp
// Example 1: Serializing a GuardExtensions instance to JSON
var guardExtensions = new GuardExtensions { Type = "MyType", Namespace = "MyNamespace", Assembly = "MyAssembly", Methods = new[] { "Method1", "Method2" } };
var json = guardExtensions.ToJson();
Console.WriteLine(json);

// Example 2: Deserializing a JSON string into a GuardExtensionsMetadata instance
var json = "{\"Type\":\"MyType\",\"Namespace\":\"MyNamespace\",\"Assembly\":\"MyAssembly\",\"Methods\":[\"Method1\",\"Method2\"]}";
if (GuardExtensionsJsonExtensions.TryFromJson(json, out var metadata))
{
    Console.WriteLine($"Type: {metadata.Type}, Namespace: {metadata.Namespace}, Assembly: {metadata.Assembly}, Methods: {string.Join(", ", metadata.Methods)}");
}
else
{
    Console.WriteLine("Deserialization failed");
}
```

## Notes
When using `ToJson` and `FromJson`, be aware that the JSON serialization and deserialization process may throw exceptions if the data is malformed or cannot be converted. The `TryFromJson` method provides a safer alternative, allowing you to handle deserialization failures more elegantly. Additionally, the `Type`, `Namespace`, `Assembly`, and `Methods` properties may return `null` if the corresponding data is not set, so be sure to check for null references before using the values. The `GuardExtensionsJsonExtensions` type is thread-safe, as it only provides static methods and does not maintain any internal state. However, the underlying JSON serialization and deserialization process may not be thread-safe, so be cautious when using these methods in concurrent environments.
