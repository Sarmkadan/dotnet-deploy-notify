# TypeHelperJsonExtensions

Provides JSON serialization and deserialization support for `TypeHelperMetadata` objects, along with properties that expose metadata about a .NET type, such as its name, namespace, assembly, and methods. This type bridges type reflection information with JSON representation.

## API

### ToJson

```csharp
public static string ToJson
```

Serializes the current `TypeHelperJsonExtensions` instance to its JSON string representation.

**Returns:** A JSON-formatted string containing the type metadata.

**Exceptions:** Throws if serialization fails due to an invalid object state.

---

### FromJson

```csharp
public static TypeHelperMetadata? FromJson
```

Deserializes a JSON string into a `TypeHelperMetadata` object.

**Returns:** A `TypeHelperMetadata` instance if deserialization succeeds; `null` if the JSON is invalid or cannot be mapped.

**Parameters:** Expects a JSON string as input (typically set via a property or method argument before calling).

**Exceptions:** Throws if the JSON is malformed and cannot be processed.

---

### TryFromJson

```csharp
public static bool TryFromJson
```

Attempts to deserialize a JSON string into a `TypeHelperMetadata` object without throwing exceptions.

**Returns:** `true` if deserialization succeeds; `false` otherwise.

**Parameters:** Expects a JSON string as input. The resulting object is typically accessed through an output parameter or property after the call.

**Exceptions:** Does not throw. All failures are communicated through the return value.

---

### Type

```csharp
public string? Type
```

Gets the full type name (e.g., `System.String`). Returns `null` if no type information is available.

---

### Namespace

```csharp
public string? Namespace
```

Gets the namespace of the type (e.g., `System`). Returns `null` if no namespace is set.

---

### Assembly

```csharp
public string? Assembly
```

Gets the assembly name where the type is defined (e.g., `System.Private.CoreLib`). Returns `null` if no assembly information is available.

---

### Methods

```csharp
public string[]? Methods
```

Gets an array of method names belonging to the type. Returns `null` if no methods are recorded.

## Usage

### Example 1: Serialize Type Metadata to JSON

```csharp
var metadata = new TypeHelperJsonExtensions
{
    Type = "System.String",
    Namespace = "System",
    Assembly = "System.Private.CoreLib",
    Methods = new[] { "Contains", "IndexOf", "Substring" }
};

string json = metadata.ToJson;
Console.WriteLine(json);
```

### Example 2: Deserialize and Inspect Type Metadata

```csharp
string json = @"{
    ""Type"": ""System.Collections.Generic.List`1"",
    ""Namespace"": ""System.Collections.Generic"",
    ""Assembly"": ""System.Collections"",
    ""Methods"": [""Add"", ""Remove"", ""Clear""]
}";

TypeHelperMetadata? result = TypeHelperJsonExtensions.FromJson;
if (result != null)
{
    Console.WriteLine($"Type: {result.Type}");
    Console.WriteLine($"Methods: {string.Join(", ", result.Methods ?? Array.Empty<string>())}");
}
```

## Notes

- The `Type`, `Namespace`, `Assembly`, and `Methods` properties are nullable; always check for `null` before accessing their values to avoid `NullReferenceException`.
- `FromJson` returns `null` when the JSON is invalid or empty. Use `TryFromJson` when you need to avoid exceptions and handle failures gracefully.
- The `Methods` array may be `null` or empty. An empty array indicates the type has no methods recorded, while `null` indicates the data was not provided.
- This type is not thread-safe. If instances are shared across threads, external synchronization is required.
- The JSON format expected by `FromJson` and `TryFromJson` must match the schema produced by `ToJson`; mismatched schemas will result in deserialization failures.
