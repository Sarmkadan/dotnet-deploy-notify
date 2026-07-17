# NotificationBuilderExtensions

Provides extension methods for `NotificationBuilder` to fluently configure deployment notifications with metadata, source control information, timing, build references, channels, infrastructure details, test results, and priority rules based on deployment status.

## API

### `WithDeploymentMetadata`
Adds deployment-specific metadata to the notification, such as environment, region, or deployment ID.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `metadata`: A dictionary of key-value pairs representing deployment metadata.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `metadata` is `null`.

### `WithPriorityForStatus`
Configures the notification priority dynamically based on the deployment status.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `statusToPriority`: A dictionary mapping deployment statuses to priority levels.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `statusToPriority` is `null`.

### `WithSourceControl`
Attaches source control information (e.g., commit hash, branch, repository URL) to the notification.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `commitHash`: The commit hash associated with the deployment.
  - `branch`: The branch from which the deployment originated.
  - `repositoryUrl`: The URL of the source control repository.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `repositoryUrl` is `null`.

### `WithTiming`
Adds timing information (e.g., start/end times, duration) to the notification.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `startTime`: The UTC timestamp when the deployment started.
  - `endTime`: The UTC timestamp when the deployment completed.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `startTime` or `endTime` is `null`.

### `WithBuildReference`
Links the notification to a specific build in the CI/CD pipeline.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `buildId`: The unique identifier of the build.
  - `buildUrl`: The URL to access the build details.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `buildUrl` is `null`.

### `WithChannels`
Specifies the notification channels (e.g., Slack, Teams, Email) through which the notification should be sent.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `channels`: A collection of channel identifiers.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `channels` is `null`.

### `WithInfrastructureMetadata`
Adds infrastructure-related metadata (e.g., cloud provider, instance types, region) to the notification.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `metadata`: A dictionary of key-value pairs representing infrastructure metadata.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `metadata` is `null`.

### `WithTestResults`
Attaches test execution results (e.g., pass/fail counts, test suite name) to the notification.

- **Parameters**
  - `builder`: The `NotificationBuilder` instance.
  - `testSuite`: The name of the test suite.
  - `totalTests`: The total number of tests executed.
  - `failedTests`: The number of failed tests.
  - `passedTests`: The number of passed tests.
- **Return Value**
  Returns the `NotificationBuilder` for method chaining.
- **Exceptions**
  Throws `ArgumentNullException` if `testSuite` is `null`.

## Usage

### Example 1: Basic Deployment Notification
```csharp
var notification = new NotificationBuilder()
    .WithDeploymentMetadata(new Dictionary<string, string>
    {
        { "environment", "production" },
        { "region", "us-west-2" }
    })
    .WithSourceControl(
        commitHash: "a1b2c3d",
        branch: "main",
        repositoryUrl: "https://github.com/example/repo"
    )
    .WithTiming(
        startTime: DateTime.UtcNow.AddMinutes(-30),
        endTime: DateTime.UtcNow
    )
    .WithBuildReference(
        buildId: "12345",
        buildUrl: "https://ci.example.com/builds/12345"
    )
    .WithChannels(new[] { "slack", "teams" })
    .WithInfrastructureMetadata(new Dictionary<string, string>
    {
        { "cloudProvider", "aws" },
        { "instanceType", "t3.large" }
    })
    .WithTestResults(
        testSuite: "UnitTests",
        totalTests: 120,
        failedTests: 2,
        passedTests: 118
    )
    .Build();
```

### Example 2: Priority-Based Notification
```csharp
var notification = new NotificationBuilder()
    .WithPriorityForStatus(new Dictionary<DeploymentStatus, NotificationPriority>
    {
        { DeploymentStatus.Success, NotificationPriority.Low },
        { DeploymentStatus.Failed, NotificationPriority.High }
    })
    .WithDeploymentMetadata(new Dictionary<string, string>
    {
        { "environment", "staging" }
    })
    .WithChannels(new[] { "email" })
    .Build();
```

## Notes

- **Thread Safety**: All extension methods are stateless and operate on the `NotificationBuilder` instance passed as a parameter. The methods do not modify shared state, making them thread-safe for concurrent use.
- **Null Handling**: Methods validate parameters for `null` values and throw `ArgumentNullException` immediately, avoiding silent failures or unexpected behavior downstream.
- **Immutability**: The `NotificationBuilder` itself is assumed to be immutable or thread-safe; extensions do not alter its internal state directly but return a new or modified instance.
- **Order Independence**: Extension methods can be chained in any order, as each operates independently on the builder’s state. However, duplicate calls (e.g., `WithChannels` twice) may overwrite prior values depending on the builder’s implementation.
