# IHttpClientFactory

`IHttpClientFactory` provides a mechanism for creating and managing `HttpClient` instances in .NET applications, particularly in scenarios requiring resilient HTTP requests with retry logic. It abstracts the complexity of `HttpClient` lifecycle management, including connection pooling and DNS refresh, while offering built-in retry capabilities for transient failures.

## API

### `DefaultHttpClientFactory`
A concrete implementation of `IHttpClientFactory` that creates `HttpClient` instances with default configuration. Instances are typically registered as singletons in the dependency injection container.

### `HttpClient CreateClient(string name)`
Creates and configures an `HttpClient` instance based on the provided name. The name maps to named configurations (e.g., timeouts, base addresses) registered with the factory.

- **Parameters**:
  - `name` (string): The logical name of the client to create.
- **Returns**: An `HttpClient` instance configured for the specified name.
- **Throws**: `ArgumentNullException` if `name` is `null`.

### `HttpClient CreateClientWithRetry(string name, int maxRetries = 3)`
Creates an `HttpClient` instance with automatic retry logic for transient failures. The retry mechanism applies to idempotent HTTP operations (e.g., GET, PUT).

- **Parameters**:
  - `name` (string): The logical name of the client to create.
  - `maxRetries` (int, optional): Maximum number of retry attempts. Defaults to 3.
- **Returns**: A configured `HttpClient` with retry policies applied.
- **Throws**: `ArgumentNullException` if `name` is `null`; `ArgumentOutOfRangeException` if `maxRetries` is negative.

### `bool IsSuccessful`
Indicates whether the last HTTP operation completed successfully (HTTP status code 2xx).

- **Returns**: `true` if the last operation was successful; otherwise, `false`.

### `int StatusCode`
Gets the HTTP status code of the last operation.

- **Returns**: The status code as an integer (e.g., 200, 404).

### `T? Content`
Gets the deserialized response body of the last operation as type `T`.

- **Returns**: The deserialized content, or `null` if the operation failed or the body was empty.
- **Type Parameters**: `T` must be deserializable from JSON.

### `string? ErrorMessage`
Gets the error message associated with the last failed operation.

- **Returns**: A string describing the error, or `null` if no error occurred.

### `TimeSpan ElapsedTime`
Gets the duration of the last HTTP operation.

- **Returns**: The elapsed time as a `TimeSpan`.

### `override string ToString()`
Returns a formatted string representation of the last operation's outcome, including status code, elapsed time, and error message (if applicable).

- **Returns**: A string in the format `"[StatusCode] [ElapsedTime] - [ErrorMessage]"`.

### `RetryableHttpClient`
A decorator for `HttpClient` that adds retry logic for transient failures. Automatically retries idempotent operations (e.g., GET, PUT) when encountering transient errors (e.g., 5xx, timeout).

### `async Task<HttpResponse<string>> PostWithRetryAsync(string url, string content, int maxRetries = 3)`
Sends an HTTP POST request with automatic retry logic for transient failures. The request body is sent as plain text.

- **Parameters**:
  - `url` (string): The target URL.
  - `content` (string): The request body.
  - `maxRetries` (int, optional): Maximum number of retry attempts. Defaults to 3.
- **Returns**: An `HttpResponse<string>` containing the response body (as string) or error details.
- **Throws**: `ArgumentNullException` if `url` or `content` is `null`.

### `HttpRequestBuilder`
A fluent builder for constructing `HttpRequestMessage` instances with headers, content, and timeout configurations.

### `HttpRequestBuilder AddHeader(string name, string value)`
Adds an HTTP header to the request.

- **Parameters**:
  - `name` (string): The header name.
  - `value` (string): The header value.
- **Returns**: The builder instance for method chaining.
- **Throws**: `ArgumentNullException` if `name` or `value` is `null`.

### `HttpRequestBuilder AddJsonContent(object content)`
Sets the request body as JSON content, serializing the provided object.

- **Parameters**:
  - `content` (object): The object to serialize as JSON.
- **Returns**: The builder instance for method chaining.
- **Throws**: `ArgumentNullException` if `content` is `null`.

### `HttpRequestBuilder SetTimeout(TimeSpan timeout)`
Configures the request timeout.

- **Parameters**:
  - `timeout` (TimeSpan): The timeout duration.
- **Returns**: The builder instance for method chaining.
- **Throws**: `ArgumentOutOfRangeException` if `timeout` is negative or zero.

### `HttpRequestMessage Build()`
Constructs the `HttpRequestMessage` from the configured builder state.

- **Returns**: The fully configured `HttpRequestMessage`.
- **Throws**: `InvalidOperationException` if required properties (e.g., URL) are missing.

### `static HttpRequestBuilder Post(string url)`
Creates a new `HttpRequestBuilder` configured for an HTTP POST request to the specified URL.

- **Parameters**:
  - `url` (string): The target URL.
- **Returns**: A new `HttpRequestBuilder` instance.
- **Throws**: `ArgumentNullException` if `url` is `null`.

### `static HttpRequestBuilder Get(string url)`
Creates a new `HttpRequestBuilder` configured for an HTTP GET request to the specified URL.

- **Parameters**:
  - `url` (string): The target URL.
- **Returns**: A new `HttpRequestBuilder` instance.
- **Throws**: `ArgumentNullException` if `url` is `null`.

### `static HttpRequestBuilder Put(string url)`
Creates a new `HttpRequestBuilder` configured for an HTTP PUT request to the specified URL.

- **Parameters**:
  - `url` (string): The target URL.
- **Returns**: A new `HttpRequestBuilder` instance.
- **Throws**: `ArgumentNullException` if `url` is `null`.

### `static HttpRequestBuilder Delete(string url)`
Creates a new `HttpRequestBuilder` configured for an HTTP DELETE request to the specified URL.

- **Parameters**:
  - `url` (string): The target URL.
- **Returns**: A new `HttpRequestBuilder` instance.
- **Throws**: `ArgumentNullException` if `url` is `null`.

## Usage

### Example 1: Basic HTTP GET with Retry
