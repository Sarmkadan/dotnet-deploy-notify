# MetricsCollectorJsonExtensions

Provides JSON serialization and deserialization helpers for the types used by the metrics collection functionality in the **dotnet-deploy-notify** project. The extension methods are pure static functions that convert objects to and from their JSON representation without maintaining any internal state.

## API

### ToJson (multiple overloads)

**Purpose**  
Serializes a supported metric‑related object to a JSON string.

**Parameters**  
- `value`: The instance to serialize. The exact type varies by overload (e.g., `MetricsCollector`, `MetricValue`, `MetricStatistics`, or other metric‑related types supported by the class).

**Return value**  
A JSON‑encoded string representing `value`. If `value` is `null`, the result is `null` for overloads that accept nullable inputs, or an empty JSON object/array as appropriate for the specific overload.

**Exceptions**  
- `ArgumentNullException` if the passed argument is `null` and the overload does not accept null.  
- `JsonException` if serialization fails for any reason (e.g., unsupported type, circular reference).

### FromJson

**Purpose**  
Deserializes a JSON string into a `MetricsCollector?` instance.

**Parameters**  
- `json`: The JSON text to parse.

**Return value**  
A `MetricsCollector` instance if the JSON is valid and represents a metrics collector; otherwise `null`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`.  
- `JsonException` if `json` is not valid JSON or does not correspond to a `MetricsCollector`.

### TryFromJson

**Purpose**  
Attempts to parse a JSON string into a `MetricsCollector?` instance without throwing on failure.

**Parameters**  
- `json`: The JSON text to parse.  
- `result`: When the method returns `true`, contains the deserialized `MetricsCollector`; otherwise `null`.

**Return value**  
`true` if `json` was successfully parsed into a `MetricsCollector`; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`. The method does not throw `JsonException`; parse failures are indicated by the return value.

### FromJsonToMetricValue

**Purpose**  
Deserializes a JSON string into a `MetricValue?` instance.

**Parameters**  
- `json`: The JSON text to parse.

**Return value**  
A `MetricValue` instance if the JSON is valid and represents a metric value; otherwise `null`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`.  
- `JsonException` if `json` is not valid JSON or does not correspond to a `MetricValue`.

### TryFromJsonToMetricValue

**Purpose**  
Attempts to parse a JSON string into a `MetricValue?` instance without throwing on failure.

**Parameters**  
- `json`: The JSON text to parse.  
- `result`: When the method returns `true`, contains the deserialized `MetricValue`; otherwise `null`.

**Return value**  
`true` if `json` was successfully parsed into a `MetricValue`; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`. No `JsonException` is thrown.

### FromJsonToMetricStatistics

**Purpose**  
Deserializes a JSON string into a `MetricStatistics?` instance.

**Parameters**  
- `json`: The JSON text to parse.

**Return value**  
A `MetricStatistics` instance if the JSON is valid and represents metric statistics; otherwise `null`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`.  
- `JsonException` if `json` is not valid JSON or does not correspond to a `MetricStatistics`.

### TryFromJsonToMetricStatistics

**Purpose**  
Attempts to parse a JSON string into a `MetricStatistics?` instance without throwing on failure.

**Parameters**  
- `json`: The JSON text to parse.  
- `result`: When the method returns `true`, contains the deserialized `MetricStatistics`; otherwise `null`.

**Return value**  
`true` if `json` was successfully parsed into a `MetricStatistics`; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`. No `JsonException` is thrown.

### Additional ToJson overloads

The class contains further `ToJson` overloads that serialize other metric‑related types to JSON strings. Their purpose, parameters, return values, and exception behavior follow the same pattern as the primary `ToJson` overload described above: they accept an instance of a specific type and return its JSON representation, throwing `ArgumentNullException` for disallowed null inputs and `JsonException` on serialization failures.

## Usage

```csharp
using DotnetDeployNotify.Metrics;

// Create a metrics collector instance
var collector = new MetricsCollector();
// ... populate collector with metric data ...

// Serialize to JSON
string json = MetricsCollectorJsonExtensions.ToJson(collector);
// json now contains the JSON representation of the collector

// Deserialize back from JSON
MetricsCollector? restored = MetricsCollectorJsonExtensions.FromJson(json);
// restored holds the deserialized collector, or null if json was invalid
```

```csharp
using DotnetDeployNotify.Metrics;

// Attempt to parse JSON safely; avoid exceptions on malformed input
string maybeJson = GetJsonFromSomewhere(); // could be null or invalid
if (MetricsCollectorJsonExtensions.TryFromJson(maybeJson, out var collector))
{
    // Use collector
    Process(collector);
}
else
{
    // Handle invalid or null JSON
    Log.Warning("Failed to deserialize metrics collector.");
}
```

## Notes

- All methods are **static** and operate solely on their inputs; they contain no mutable state and are therefore **thread‑safe**. Multiple threads may invoke them concurrently without synchronization.
- The `FromJson*` methods return `null` when the input JSON does not represent the expected type, allowing callers to treat missing or malformed data as an absent value rather than throwing.
- The `TryFromJson*` variants are intended for scenarios where exceptions are undesirable; they only throw `ArgumentNullException` for a `null` JSON string and otherwise report failure via the boolean return value.
- Consumers should verify that the JSON encoding used by these methods matches the expectations of any external systems (e.g., UTF‑8, no BOM). The implementation relies on the default `System.Text.Json` serializer with its standard options.
- Because the overloads are differentiated only by their parameter types, calling the correct overload depends on compile‑time type matching; ambiguous calls will result in a compiler error.
