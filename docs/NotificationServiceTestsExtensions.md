# NotificationServiceTestsExtensions

Utility class providing extension methods and factory methods for creating and verifying test artifacts related to the `NotificationService` in unit tests. Designed to simplify test setup and assertions when verifying notification creation, updates, and result handling.

## API

### `public static DeploymentNotification CreateTestNotification(string deploymentId = null, string status = null, DateTimeOffset? timestamp = null)`

Creates a minimal `DeploymentNotification` instance suitable for testing. All parameters are optional and default to sensible test values.

- **Parameters**
  - `deploymentId`: Optional deployment identifier. Defaults to `"test-deployment-123"`.
  - `status`: Optional deployment status. Defaults to `"Succeeded"`.
  - `timestamp`: Optional notification timestamp. Defaults to `DateTimeOffset.UtcNow`.

- **Return value**
  Returns a new `DeploymentNotification` with the specified or default values.

### `public static ChannelConfiguration CreateTestChannelConfiguration(string channelName = null, string webhookUrl = null)`

Creates a minimal `ChannelConfiguration` instance suitable for testing.

- **Parameters**
  - `channelName`: Optional channel name. Defaults to `"test-channel"`.
  - `webhookUrl`: Optional webhook URL. Defaults to `"https://example.com/webhook"`.

- **Return value**
  Returns a new `ChannelConfiguration` with the specified or default values.

### `public static NotificationResult CreateSuccessfulResult(string message = null)`

Creates a `NotificationResult` indicating a successful notification operation.

- **Parameters**
  - `message`: Optional result message. Defaults to `"Notification sent successfully"`.

- **Return value**
  Returns a new `NotificationResult` with `IsSuccess = true` and the specified message.

### `public static NotificationResult CreateFailedResult(string errorMessage = null)`

Creates a `NotificationResult` indicating a failed notification operation.

- **Parameters**
  - `errorMessage`: Optional error message. Defaults to `"Failed to send notification"`.

- **Return value**
  Returns a new `NotificationResult` with `IsSuccess = false` and the specified error message.

### `public static void VerifyNotificationCreated(Mock<INotificationService> mock, DeploymentNotification expected)`

Verifies that the `INotificationService` was called exactly once to create a notification matching the expected values.

- **Parameters**
  - `mock`: The mocked `INotificationService` to verify.
  - `expected`: The expected `DeploymentNotification` instance.

- **Exceptions**
  - Throws `MockException` if the method was not called exactly once or if the arguments do not match.

### `public static void VerifyNotificationUpdated(Mock<INotificationService> mock, DeploymentNotification expected)`

Verifies that the `INotificationService` was called exactly once to update a notification matching the expected values.

- **Parameters**
  - `mock`: The mocked `INotificationService` to verify.
  - `expected`: The expected `DeploymentNotification` instance.

- **Exceptions**
  - Throws `MockException` if the method was not called exactly once or if the arguments do not match.

### `public static void VerifyResultCreated(Mock<INotificationService> mock, NotificationResult expected)`

Verifies that the `INotificationService` was called exactly once to create a result matching the expected values.

- **Parameters**
  - `mock`: The mocked `INotificationService` to verify.
  - `expected`: The expected `NotificationResult` instance.

- **Exceptions**
  - Throws `MockException` if the method was not called exactly once or if the arguments do not match.

### `public static void SetupValidationResult(Mock<INotificationService> mock, bool isValid = true, string errorMessage = null)`

Configures the mocked `INotificationService` to return a specific validation result.

- **Parameters**
  - `mock`: The mocked `INotificationService` to configure.
  - `isValid`: Whether validation should pass. Defaults to `true`.
  - `errorMessage`: Optional error message if validation fails. Defaults to `null`.

### `public static void SetupChannelConfigurations(Mock<INotificationService> mock, IEnumerable<ChannelConfiguration> configurations)`

Configures the mocked `INotificationService` to return a specific set of channel configurations.

- **Parameters**
  - `mock`: The mocked `INotificationService` to configure.
  - `configurations`: The channel configurations to return.

### `public static void SetupWebhookDispatch(Mock<INotificationService> mock, bool shouldSucceed = true, string errorMessage = null)`

Configures the mocked `INotificationService` to simulate webhook dispatch behavior.

- **Parameters**
  - `mock`: The mocked `INotificationService` to configure.
  - `shouldSucceed`: Whether the webhook dispatch should succeed. Defaults to `true`.
  - `errorMessage`: Optional error message if dispatch fails. Defaults to `null`.

## Usage

### Example 1: Testing notification creation
