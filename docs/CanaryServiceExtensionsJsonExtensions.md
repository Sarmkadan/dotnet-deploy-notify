# CanaryServiceExtensionsJsonExtensions

Provides JSON serialization and deserialization functionality for `CanaryServiceExtensionsMetadata` instances, enabling conversion between metadata objects and their JSON string representations. This class includes methods for converting metadata to JSON and parsing JSON into metadata, along with properties representing the structure of the metadata.

## API

### ToJson

```csharp
public static string ToJson(CanaryServiceExtensionsMetadata value)
```

Converts a `CanaryServiceExtensionsMetadata` instance into its JSON string representation.

- **Parameters**:  
  `value` – The metadata instance to serialize. Must not be `null`.

- **Return Value**:  
  A JSON string representing the metadata.

- **Exceptions**:  
  `ArgumentNullException` – Thrown when `value` is `null`.

---

### FromJson

```csharp
public static CanaryServiceExtensionsMetadata? FromJson(string json)
```

Parses a JSON string into a `CanaryServiceExtensionsMetadata` instance.

- **Parameters**:  
  `json` – The JSON string to deserialize. Must not be `null` or empty.

- **Return Value**:  
  The deserialized `CanaryServiceExtensionsMetadata` instance, or `null` if parsing fails.

- **Exceptions**:  
  `ArgumentNullException` – Thrown when `json` is `null`.  
  `JsonException` – Thrown when the JSON is malformed or does not conform to the expected schema.

---

### TryFromJson

```csharp
public static bool TryFromJson(string json, out CanaryServiceExtensionsMetadata? result)
```

Attempts to parse a JSON string into a `CanaryServiceExtensionsMetadata` instance without throwing exceptions.

- **Parameters**:  
  `json` – The JSON string to deserialize. Must not be `null`.  
  `result` – When this method returns, contains the deserialized metadata instance, or `null` if parsing failed.

- **Return Value**:  
  `true` if parsing succeeded; `false` otherwise.

- **Exceptions**:  
  `ArgumentNullException` – Thrown when `json` is `null`.

---

### Type

```csharp
public string? Type { get; }
```

Gets the type name of the canary service extension.

- **Return Value**:  
  The type name as a string, or `null` if not specified.

---

### Namespace

```csharp
public string? Namespace { get; }
```

Gets the namespace of the canary service extension.

- **Return Value**:  
  The namespace as a string, or `null` if not specified.

---

### Assembly

```csharp
public string? Assembly { get; }
```

Gets the assembly name of the canary service extension.

- **Return Value**:  
  The assembly name as a string, or `null` if not specified.

---

### Methods

```csharp
public string[]? Methods { get; }
```

Gets the list of method names associated with the canary service extension.

- **Return Value**:  
  An array of method names, or `null` if no methods are defined.

## Usage

### Serializing Metadata to JSON

```csharp
var metadata = new CanaryServiceExtensionsMetadata
{
    Type = "MyExtension",
    Namespace = "MyNamespace",
    Assembly = "MyAssembly",
    Methods = new[] { "Method1", "Method2" }
};

string json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
Console.WriteLine(json);
// Output: {"Type":"MyExtension","Namespace":"MyNamespace","Assembly":"MyAssembly","Methods":["Method1","Method2"]}
```

### Deserializing JSON to Metadata

```csharp
string json = "{\"Type\":\"MyExtension\",\"Namespace\":\"MyNamespace\",\"Assembly\":\"MyAssembly\",\"Methods\":[\"Method1\",\"Method2\"]}";

if (CanaryServiceExtensionsJsonExtensions.TryFromJson(json, out var metadata))
{
    Console.WriteLine($"Type: {metadata.Type}, Methods: {string.Join(", ", metadata.Methods ?? Array.Empty<string>())}");
}
else
{
    Console.WriteLine("Failed to parse JSON.");
}
```

## Notes

- **Null Handling**: Properties (`Type`, `Namespace`, `Assembly`, `Methods`) may return `null` if not explicitly set. Callers should handle potential `null` values when accessing these properties.
- **Thread Safety**: Static methods (`ToJson`, `FromJson`, `TryFromJson`) are thread-safe for concurrent use, assuming the underlying JSON serialization library is thread-safe. Instance properties are safe for read-only access after initialization.
- **Edge Cases**:  
  - `FromJson` throws `JsonException` for invalid JSON; use `TryFromJson` to avoid exceptions.  
  - `Methods` returns `null` instead of an empty array if no methods are defined.  
  - Empty strings for `Type`, `Namespace`, or `Assembly` are allowed unless restricted by validation logic elsewhere.
