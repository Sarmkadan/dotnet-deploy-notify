// existing content ...

## ServiceExtensionsJsonExtensions

The `ServiceExtensionsJsonExtensions` class provides JSON serialization utilities for ServiceExtensions metadata. It allows converting metadata about ServiceExtensions methods and types to/from JSON format using `ToJson()`, `FromJson()`, and `TryFromJson()` methods.

Example usage:
```csharp
var json = ServiceExtensionsJsonExtensions.ToJson();
var metadata = ServiceExtensionsJsonExtensions.FromJson(json);
Console.WriteLine(metadata.Methods.Length); // Output: 10
var success = ServiceExtensionsJsonExtensions.TryFromJson(json, out var parsedMetadata);
Console.WriteLine(parsedMetadata.Type); // Output: ServiceExtensions
```

## TestHttpClientExtensions

The `TestHttpClientExtensions` class provides a set of extension methods for simplifying common webhook testing scenarios with `TestHttpClient`...

// rest of existing content remains unchanged
