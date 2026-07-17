# StringExtensionsJsonExtensions

Provides JSON serialization and deserialization support for `StringExtensionsMetadata` objects, enabling conversion between the metadata type and its JSON string representation. This type also exposes reflective metadata about the `StringExtensions` class itself, including its namespace, assembly, and available methods.

## API

### ToJson

```csharp
public static string ToJson(this StringExtensionsMetadata metadata)
```

Serializes a `StringExtensionsMetadata` instance to its JSON string representation.

**Parameters:**
- `metadata` — The `StringExtensionsMetadata` object to serialize.

**Returns:** A JSON string representing the metadata object.

**Throws:** May throw `ArgumentNullException` if `metadata` is null. Standard `System.Text.Json` serialization exceptions may propagate if the object graph contains unserializable data.

---

### FromJson

```csharp
public static StringExtensionsMetadata? FromJson(this string json)
```

Deserializes a JSON string into a `StringExtensionsMetadata` instance.

**Parameters:**
- `json` — A JSON string previously produced by `ToJson` or structurally equivalent.

**Returns:** A populated `StringExtensionsMetadata` object, or `null` if the input is null, empty, or whitespace.

**Throws:** May throw `System.Text.Json.JsonException` if the JSON is malformed or does not match the expected schema.

---

### TryFromJson

```csharp
public static bool TryFromJson(this string json, out StringExtensionsMetadata? result)
```

Attempts to deserialize a JSON string into a `StringExtensionsMetadata` instance without throwing on failure.

**Parameters:**
- `json` — A JSON string to parse.
- `result` — When this method returns `true`, contains the deserialized object; when `false`, contains `null`.

**Returns:** `true` if deserialization succeeded; `false` if the input was null, empty, whitespace, or contained invalid JSON.

**Throws:** Does not throw. All parsing errors are caught internally and result in a `false` return.

---

### Type

```csharp
public string? Type
```

Gets the full type name of the `StringExtensions` class. May be `null` if reflection metadata is unavailable.

---

### Namespace

```csharp
public string? Namespace
```

Gets the namespace in which `StringExtensions` is declared. May be `null` if reflection metadata is unavailable.

---

### Assembly

```csharp
public string? Assembly
```

Gets the assembly name containing `StringExtensions`. May be `null` if reflection metadata is unavailable.

---

### Methods

```csharp
public string[]? Methods
```

Gets the names of public methods exposed by the `StringExtensions` class. Returns `null` if reflection metadata is unavailable; otherwise an array of method name strings.

## Usage

### Example 1: Round-tripping metadata through JSON

```csharp
var metadata = new StringExtensionsMetadata
{
    Type = "StringExtensions",
    Namespace = "DotnetDeployNotify.Extensions",
    Assembly = "DotnetDeployNotify",
    Methods = new[] { "Truncate", "ToSlug", "EnsureEndsWith" }
};

// Serialize to JSON
string json = metadata.ToJson();

// Deserialize safely
if (json.TryFromJson(out StringExtensionsMetadata? restored))
{
    Console.WriteLine($"Restored type: {restored!.Type}");
    Console.WriteLine($"Methods count: {restored.Methods?.Length ?? 0}");
}
```

### Example 2: Deserializing from a stored configuration

```csharp
string storedJson = """
{
    "Type": "StringExtensions",
    "Namespace": "DotnetDeployNotify.Extensions",
    "Assembly": "DotnetDeployNotify",
    "Methods": ["Truncate", "ToSlug"]
}
""";

StringExtensionsMetadata? config = storedJson.FromJson();

if (config is not null && config.Methods is not null)
{
    foreach (string method in config.Methods)
    {
        Console.WriteLine($"Available method: {method}");
    }
}
```

## Notes

- **Null handling:** `FromJson` and `TryFromJson` treat null, empty, and whitespace strings as valid input representing absence of data, returning `null` or `false` respectively rather than throwing.
- **Schema compatibility:** `FromJson` expects JSON matching the shape produced by `ToJson`. Unexpected properties are typically ignored by `System.Text.Json` with default settings, but missing required properties may result in default values.
- **Thread safety:** All public members are static and operate on immutable inputs or return new objects. The methods do not mutate shared state and are safe to call concurrently from multiple threads.
- **Reflection metadata:** The `Type`, `Namespace`, `Assembly`, and `Methods` properties reflect the `StringExtensions` class at the time the assembly is loaded. They are not derived from serialized JSON payloads and are independent of the serialize/deserialize operations.
