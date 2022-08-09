# BuildStatusConverter

A converter that translates between `BuildStatus` enum values and their JSON string representations, and provides helper methods for serializing and deserializing related notification objects.

## API

### `public override BuildStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)`

Reads a JSON string value and converts it to a `BuildStatus` enum value.

- **Parameters**
  - `reader`: The `Utf8JsonReader` instance to read from.
  - `typeToConvert`: The type to convert to (should be `BuildStatus`).
  - `options`: Serialization options.
- **Return Value**: The deserialized `BuildStatus` value.
- **Throws**: `JsonException` if the JSON value is not a valid string or cannot be mapped to a `BuildStatus`.

### `public override void Write(Utf8JsonWriter writer, BuildStatus value, JsonSerializerOptions options)`

Writes a `BuildStatus` enum value as a JSON string.

- **Parameters**
  - `writer`: The `Utf8JsonWriter` instance to write to.
  - `value`: The `BuildStatus` value to serialize.
  - `options`: Serialization options.
- **Throws**: `JsonException` if the `value` is not a defined `BuildStatus`.

### `public override NotificationChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)`

Reads a JSON string value and converts it to a `NotificationChannel` enum value.

- **Parameters**
  - `reader`: The `Utf8JsonReader` instance to read from.
  - `typeToConvert`: The type to convert to (should be `NotificationChannel`).
  - `options`: Serialization options.
- **Return Value**: The deserialized `NotificationChannel` value.
- **Throws**: `JsonException` if the JSON value is not a valid string or cannot be mapped to a `NotificationChannel`.

### `public override void Write(Utf8JsonWriter writer, NotificationChannel value, JsonSerializerOptions options)`

Writes a `NotificationChannel` enum value as a JSON string.

- **Parameters**
  - `writer`: The `Utf8JsonWriter` instance to write to.
  - `value`: The `NotificationChannel` value to serialize.
  - `options`: Serialization options.
- **Throws**: `JsonException` if the `value` is not a defined `NotificationChannel`.

### `public JsonSerializationHelper`

Gets the shared instance of `JsonSerializationHelper` used for serialization and deserialization operations.

- **Return Value**: The singleton `JsonSerializationHelper` instance.

### `public string Serialize<T>(T value)`

Serializes an object to a JSON string.

- **Parameters**
  - `value`: The object to serialize.
- **Return Value**: The JSON string representation of `value`.
- **Throws**: `JsonException` if serialization fails.

### `public T? Deserialize<T>(string json)`

Deserializes a JSON string to an object of type `T`.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Return Value**: The deserialized object of type `T`, or `null` if deserialization fails.
- **Throws**: `JsonException` if deserialization fails.

### `public Dictionary<string, object?> ObjectToDictionary<T>(T obj)`

Converts an object to a dictionary of its public properties.

- **Parameters**
  - `obj`: The object to convert.
- **Return Value**: A dictionary where keys are property names and values are property values.
- **Throws**: `ArgumentNullException` if `obj` is `null`.

### `public static (bool Success, T? Result) TryParse<T>(string input)`

Attempts to parse a JSON string into an object of type `T`.

- **Parameters**
  - `input`: The JSON string to parse.
- **Return Value**: A tuple where `Success` indicates whether parsing succeeded, and `Result` contains the parsed object (or `null` if parsing failed).

### `public static string MergeJsonObjects(string baseJson, string overlayJson)`

Merges two JSON strings by overlaying properties from `overlayJson` onto `baseJson`.

- **Parameters**
  - `baseJson`: The base JSON string.
  - `overlayJson`: The JSON string containing properties to overlay.
- **Return Value**: The merged JSON string.
- **Throws**: `JsonException` if either input is not valid JSON.

### `public static bool IsValidJson(string json)`

Checks whether a string is valid JSON.

- **Parameters**
  - `json`: The string to validate.
- **Return Value**: `true` if the string is valid JSON; otherwise, `false`.

## Usage

### Example 1: Serializing and deserializing a `BuildStatus`

```csharp
var converter = new BuildStatusConverter();
var status = BuildStatus.Succeeded;

// Serialize
string json = converter.Serialize(status);
Console.WriteLine(json); // Output: "Succeeded"

// Deserialize
BuildStatus deserialized = converter.Deserialize<BuildStatus>(json);
Console.WriteLine(deserialized); // Output: Succeeded
```

### Example 2: Merging JSON objects

```csharp
string baseJson = "{\"channel\":\"Teams\",\"recipients\":[\"user1@domain.com\"]}";
string overlayJson = "{\"recipients\":[\"user2@domain.com\"],\"priority\":\"High\"}";

string merged = BuildStatusConverter.MergeJsonObjects(baseJson, overlayJson);
Console.WriteLine(merged);
// Output: {"channel":"Teams","recipients":["user2@domain.com"],"priority":"High"}
```

## Notes

- The `Read` and `Write` methods for `BuildStatus` and `NotificationChannel` are thread-safe as they do not maintain mutable state.
- `Serialize`, `Deserialize`, and `ObjectToDictionary` are not thread-safe when using the shared `JsonSerializationHelper` instance, as they rely on mutable `JsonSerializerOptions`.
- `MergeJsonObjects` and `IsValidJson` are stateless and thread-safe.
- `TryParse` is thread-safe as it does not modify shared state.
- Edge cases such as empty strings, `null` inputs, or malformed JSON are handled by throwing `JsonException` or returning `false`/`null` as appropriate.
