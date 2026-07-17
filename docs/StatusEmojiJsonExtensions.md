# StatusEmojiJsonExtensions

Provides JSON serialization and deserialization support for `StatusEmoji` objects. This static class exposes methods to convert `StatusEmoji` instances to their JSON string representation and to parse JSON strings back into `StatusEmoji` objects, with both exception-throwing and safe-try patterns available.

## API

### `public static string ToJson`

Serializes a `StatusEmoji` instance to its JSON string representation.

- **Parameters**: Accepts a `StatusEmoji` object to serialize.
- **Returns**: A JSON string representing the `StatusEmoji` instance, including its `Status`, `Emoji`, and `Label` properties.
- **Exceptions**: Throws `ArgumentNullException` if the input is `null`. May throw `System.Text.Json.JsonException` if serialization fails due to an invalid object state.

### `public static StatusEmoji? FromJson`

Deserializes a JSON string into a `StatusEmoji` object.

- **Parameters**: A JSON string containing the serialized `StatusEmoji` data.
- **Returns**: A `StatusEmoji` instance if deserialization succeeds, or `null` if the input string is `null` or empty.
- **Exceptions**: Throws `System.Text.Json.JsonException` if the JSON is malformed or cannot be mapped to a `StatusEmoji` object. Throws `ArgumentNullException` if the input string is `null`.

### `public static bool TryFromJson`

Attempts to deserialize a JSON string into a `StatusEmoji` object without throwing exceptions on failure.

- **Parameters**: A JSON string to deserialize, and an output parameter that receives the resulting `StatusEmoji` object on success.
- **Returns**: `true` if deserialization succeeded and the output parameter contains a valid `StatusEmoji`; `false` if the input is `null`, empty, or contains invalid JSON that cannot be mapped to a `StatusEmoji`.
- **Exceptions**: Does not throw. All errors are communicated through the return value.

### `public required BuildStatus Status`

The build status value associated with the emoji notification.

- **Type**: `BuildStatus` enum.
- **Remarks**: This property is required and must be set during object construction. It represents the deployment or build outcome (e.g., Success, Failure, InProgress).

### `public required string Emoji`

The emoji character or sequence representing the status visually.

- **Type**: `string`.
- **Remarks**: This property is required. It typically contains a single emoji or a short emoji sequence used in notification messages.

### `public required string Label`

A human-readable label describing the status.

- **Type**: `string`.
- **Remarks**: This property is required. It provides a textual description accompanying the emoji, such as "Deployment Succeeded" or "Build Failed".

## Usage

### Example 1: Serializing a StatusEmoji to JSON

```csharp
var statusEmoji = new StatusEmoji
{
    Status = BuildStatus.Success,
    Emoji = "✅",
    Label = "Deployment Succeeded"
};

string json = StatusEmojiJsonExtensions.ToJson(statusEmoji);
Console.WriteLine(json);
// Output: {"Status":"Success","Emoji":"✅","Label":"Deployment Succeeded"}
```

### Example 2: Safe Deserialization with TryFromJson

```csharp
string jsonInput = "{\"Status\":\"Failure\",\"Emoji\":\"❌\",\"Label\":\"Build Failed\"}";

if (StatusEmojiJsonExtensions.TryFromJson(jsonInput, out StatusEmoji? result))
{
    Console.WriteLine($"Status: {result.Status}");
    Console.WriteLine($"Emoji: {result.Emoji}");
    Console.WriteLine($"Label: {result.Label}");
}
else
{
    Console.WriteLine("Failed to parse StatusEmoji JSON.");
}
```

## Notes

- **Required Members**: The `Status`, `Emoji`, and `Label` properties are all marked `required`. Any attempt to construct a `StatusEmoji` without providing values for these properties will result in a compile-time error. When deserializing from JSON, the input must contain corresponding keys for all three properties; otherwise, deserialization will fail.
- **Null Handling**: `FromJson` returns `null` when the input string is `null` or empty. `TryFromJson` returns `false` in the same scenario and sets the output parameter to `default`. Neither method throws for `null` input.
- **JSON Format**: The serialization format follows standard JSON conventions. Enum values for `BuildStatus` are serialized as their string names (e.g., "Success", "Failure"). The `Emoji` and `Label` fields are serialized as JSON strings.
- **Thread Safety**: All methods in this class are static and operate on immutable input data. They do not maintain shared state and are safe to call concurrently from multiple threads.
- **Error Resilience**: Prefer `TryFromJson` when consuming JSON from external sources or user input to avoid exception-driven control flow. Use `FromJson` when the JSON is known to be well-formed and a failure is truly exceptional.
