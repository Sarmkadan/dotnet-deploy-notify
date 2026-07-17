# PayloadBuilderTestsExtensions
The `PayloadBuilderTestsExtensions` class provides a set of static methods for creating test notifications and channel configurations, as well as asserting the presence of specific properties in these objects. This class is designed to simplify the process of writing unit tests for the `dotnet-deploy-notify` project, allowing developers to focus on the logic of their tests rather than the creation of test data.

## API
* `CreateTestNotification`: Creates a test deployment notification.
* `CreateSlackChannelConfig`: Creates a Slack channel configuration.
* `CreateDiscordChannelConfig`: Creates a Discord channel configuration.
* `CreateTelegramChannelConfig`: Creates a Telegram channel configuration.
* `ShouldContainCustomProperty`: Asserts that a notification contains a custom property.
* `ShouldHaveEventType`: Asserts that a notification has a specific event type.
* `WithStatus`: Sets the status of a deployment notification.
* `WithEnvironment`: Sets the environment of a deployment notification.
* `WithEmojisEnabled`: Enables emojis in a channel configuration.
* `WithEmojisDisabled`: Disables emojis in a channel configuration.
* `WithCommitDetails`: Includes commit details in a channel configuration.
* `WithoutCommitDetails`: Excludes commit details from a channel configuration.
* `WithBuildUrl`: Includes a build URL in a channel configuration.
* `WithoutBuildUrl`: Excludes a build URL from a channel configuration.
* `WithSlackBlockKit`: Enables Slack Block Kit in a channel configuration.
* `WithoutSlackBlockKit`: Disables Slack Block Kit in a channel configuration.
* `ShouldContainProjectAndVersion`: Asserts that a notification contains project and version information.
* `ShouldContainCommitInfo`: Asserts that a notification contains commit information.
* `ShouldContainDuration`: Asserts that a notification contains duration information.
* `ShouldContainBuildUrl`: Asserts that a notification contains a build URL.

## Usage
```csharp
// Example 1: Creating a test notification and asserting its properties
var notification = PayloadBuilderTestsExtensions.CreateTestNotification();
PayloadBuilderTestsExtensions.ShouldContainCustomProperty(notification, "CustomProperty");
PayloadBuilderTestsExtensions.ShouldHaveEventType(notification, "Deployment");

// Example 2: Creating a channel configuration and customizing its properties
var channelConfig = PayloadBuilderTestsExtensions.CreateSlackChannelConfig();
channelConfig = PayloadBuilderTestsExtensions.WithEmojisEnabled(channelConfig);
channelConfig = PayloadBuilderTestsExtensions.WithCommitDetails(channelConfig);
```

## Notes
When using the `PayloadBuilderTestsExtensions` class, note that the `ShouldContainCustomProperty`, `ShouldHaveEventType`, `ShouldContainProjectAndVersion`, `ShouldContainCommitInfo`, `ShouldContainDuration`, and `ShouldContainBuildUrl` methods will throw an exception if the expected property or value is not found. Additionally, the `WithStatus`, `WithEnvironment`, `WithEmojisEnabled`, `WithEmojisDisabled`, `WithCommitDetails`, `WithoutCommitDetails`, `WithBuildUrl`, `WithoutBuildUrl`, `WithSlackBlockKit`, and `WithoutSlackBlockKit` methods return a new instance of the modified object, rather than modifying the original object. This class is designed to be thread-safe, as all methods are static and do not rely on any shared state.
