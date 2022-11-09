// existing content ...

## TestHttpClientExtensions

The `TestHttpClientExtensions` class provides a set of extension methods for simplifying common webhook testing scenarios with `TestHttpClient`. It allows you to easily configure mock responses, create test loggers, and manage logging scopes.

Here's an example usage:

```csharp
var testClient = new TestHttpClient();
testClient.SetupSuccessResponse("valid");
var logger = testClient.CreateTestLogger("TestCategory");

using var scope = testClient.BeginTestScope(new { /* state */ });
testClient.SetupStatusCodeResponse(HttpStatusCode.OK, "{\"ok\":true}");
``` 

// existing content ...
