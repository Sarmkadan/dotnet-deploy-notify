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

## TrafficSplitterExtensions

The `TrafficSplitterExtensions` class provides a set of extension methods for simplifying common canary deployment scenarios with `TrafficSplitter`. It allows you to create canary deployments with linear, exponential, or blue-green rollout strategies, determine if a deployment should proceed to the next step, and normalize canary percentages. Here's an example usage:

```csharp
var splitter = new TrafficSplitter();
var deployment = splitter.CreateLinearCanaryDeployment("MyProject", "v1.0", "v0.9", Environment.Production);
var shouldProceed = await splitter.ShouldProceedToNextStepAsync(deployment, new CanaryHealthEvaluator());
var normalizedPercentage = splitter.GetCanaryPercentageNormalized(deployment.CurrentSplit);
```

// existing content ...
