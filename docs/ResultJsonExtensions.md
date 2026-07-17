# ResultJsonExtensions
The `ResultJsonExtensions` class provides a set of extension methods for working with JSON representations of `Result` objects. It enables serialization and deserialization of `Result` instances to and from JSON strings, making it easier to integrate `Result` objects with web APIs, data storage, and other systems that rely on JSON data exchange.

## API
* `public static string ToJson(this Result result)`: Serializes a `Result` object into a JSON string. The method takes a `Result` object as a parameter and returns a JSON string representation of the object. It does not throw any exceptions.
* `public static string ToJson<T>(this Result<T> result)`: Serializes a `Result<T>` object into a JSON string. The method takes a `Result<T>` object as a parameter and returns a JSON string representation of the object. It does not throw any exceptions.
* `public static Result? FromJson(string json)`: Deserializes a JSON string into a `Result` object. The method takes a JSON string as a parameter and returns a `Result` object, or `null` if deserialization fails. It throws an exception if the input JSON string is invalid.
* `public static Result<T>? FromJson<T>(string json)`: Deserializes a JSON string into a `Result<T>` object. The method takes a JSON string as a parameter and returns a `Result<T>` object, or `null` if deserialization fails. It throws an exception if the input JSON string is invalid.
* `public static bool TryFromJson(string json, out Result? result)`: Attempts to deserialize a JSON string into a `Result` object. The method takes a JSON string as a parameter and returns a boolean indicating whether deserialization was successful. If successful, the deserialized `Result` object is assigned to the `out` parameter. It does not throw any exceptions.
* `public static bool TryFromJson<T>(string json, out Result<T>? result)`: Attempts to deserialize a JSON string into a `Result<T>` object. The method takes a JSON string as a parameter and returns a boolean indicating whether deserialization was successful. If successful, the deserialized `Result<T>` object is assigned to the `out` parameter. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ResultJsonExtensions` class:
```csharp
// Serialize a Result object to JSON
var result = Result.Success();
var json = result.ToJson();
Console.WriteLine(json);

// Deserialize a JSON string to a Result<T> object
var jsonStr = "{\"IsSuccess\":true,\"Value\":\"Hello, World!\"}";
if (ResultJsonExtensions.TryFromJson<string>(jsonStr, out var resultObj))
{
    Console.WriteLine(resultObj.Value);
}
```

## Notes
When using the `ResultJsonExtensions` class, keep in mind the following edge cases:
* If the input JSON string is invalid or malformed, the `FromJson` methods will throw an exception.
* If the input JSON string represents a `Result` object with a null value, the `FromJson` methods will return a `Result` object with a null value.
* The `TryFromJson` methods are thread-safe, as they do not rely on any shared state. However, the `Result` objects returned by these methods may not be thread-safe, depending on the implementation of the `Result` class.
