# CollectionExtensionsJsonExtensions

Provides JSON serialization and deserialization support for `CollectionExtensions` metadata, enabling the conversion of collection extension information to and from JSON strings. This type facilitates inspection of collection-related extension methods by exposing their type, namespace, assembly, and method names in a structured, serializable format.

## API

### ToJson

```csharp
public static string ToJson(this CollectionExtensionsMetadata metadata)
```

Serializes a `CollectionExtensionsMetadata` instance to its JSON string representation.

**Parameters:**
- `metadata` — The `CollectionExtensionsMetadata` object to serialize.

**Returns:**
A JSON string representing the metadata.

**Exceptions:**
Throws `ArgumentNullException` if `metadata` is `null`.

---

### FromJson

```csharp
public static CollectionExtensionsMetadata? FromJson(string json)
```

Deserializes a JSON string into a `CollectionExtensionsMetadata` instance.

**Parameters:**
- `json` — A JSON string previously produced by `ToJson` or otherwise conforming to the expected schema.

**Returns:**
A `CollectionExtensionsMetadata` instance if deserialization succeeds; `null` if the input is `null`, empty, or whitespace.

**Exceptions:**
Throws `JsonException` if the JSON is malformed or cannot be mapped to the expected type.

---

### TryFromJson

```csharp
public static bool TryFromJson(string json, out CollectionExtensionsMetadata? result)
```

Attempts to deserialize a JSON string into a `CollectionExtensionsMetadata` instance without throwing on failure.

**Parameters:**
- `json` — A JSON string to deserialize.
- `result` — When this method returns `true`, contains the deserialized instance; when `false`, contains `null`.

**Returns:**
`true` if deserialization succeeded; `false` if the input is `null`, empty, whitespace, or malformed JSON.

**Exceptions:**
Does not throw. All failures are captured in the return value.

---

### Type

```csharp
public string? Type
```

Gets the type name associated with the collection extension metadata. May be `null` if not specified.

---

### Namespace

```csharp
public string? Namespace
```

Gets the namespace of the type associated with the collection extension metadata. May be `null` if not specified.

---

### Assembly

```csharp
public string? Assembly
```

Gets the assembly name containing the collection extension type. May be `null` if not specified.

---

### Methods

```csharp
public string[]? Methods
```

Gets the array of method signatures belonging to the collection extension type. May be `null` if no methods are recorded.

## Usage

### Example 1: Serialize and Deserialize

```csharp
var metadata = new CollectionExtensionsJson
{
    Type = "EnumerableExtensions",
    Namespace = "MyApp.Collections",
    Assembly = "MyApp.Core",
    Methods = new[] { "ForEach<T>", "ToHashSet<T>" }
};

string json = metadata.ToJson();
Console.WriteLine(json);

CollectionExtensionsMetadata? deserialized = CollectionExtensionsJsonExtensions.FromJson(json);
Console.WriteLine(deserialized?.Type);
```

### Example 2: Safe Deserialization with TryFromJson

```csharp
string input = GetJsonFromExternalSource(); // may be invalid

if (CollectionExtensionsJsonExtensions.TryFromJson(input, out var result))
{
    Console.WriteLine($"Type: {result.Type}");
    Console.WriteLine($"Namespace: {result.Namespace}");
    Console.WriteLine($"Methods: {string.Join(", ", result.Methods ?? Array.Empty<string>())}");
}
else
{
    Console.WriteLine("Failed to parse collection extensions metadata.");
}
```

## Notes

- **Null handling:** `FromJson` returns `null` for `null` or whitespace input, while `TryFromJson` returns `false` in the same scenario. Both approaches avoid throwing for missing data.
- **Schema expectations:** Deserialization methods expect JSON that matches the structure produced by `ToJson`. Unknown properties are typically ignored by the underlying serializer, but malformed JSON causes `FromJson` to throw and `TryFromJson` to return `false`.
- **Thread safety:** All members are static and operate on immutable string inputs or produce new instances. The type itself is not designed as a mutable shared state container; instances are plain data objects with no internal synchronization. Concurrent reads of the same instance are safe, but concurrent reads and writes to the same instance are not.
- **Nullable properties:** `Type`, `Namespace`, `Assembly`, and `Methods` are all nullable. Consumers should guard against `null` when accessing these members after deserialization.
