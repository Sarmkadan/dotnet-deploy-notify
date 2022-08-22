# PayloadBuilderTests

Unit test class for `PayloadBuilder` that validates the construction of notification payloads for different messaging platforms (Slack, Discord, Telegram) with various configuration options.

## API

### `PayloadBuilderTests`
Public test class containing unit tests for payload construction logic.

### `BuildPayload_WithSlackChannel_IncludesSlackFormat`
Validates that when a Slack channel is specified, the resulting payload uses the Slack message format.

### `BuildPayload_WithDiscordChannel_IncludesDiscordFormat`
Validates that when a Discord channel is specified, the resulting payload uses the Discord message format.

### `BuildPayload_WithTelegramChannel_IncludesTelegramText`
Validates that when a Telegram channel is specified, the resulting payload uses the Telegram text format.

### `BuildPayload_WithFailedStatus_SetCorrectEventType`
Ensures that a failed deployment status results in the correct event type being set in the payload.

### `BuildPayload_WithDeploymentSuccess_SetCorrectEventType`
Ensures that a successful deployment status results in the correct event type being set in the payload.

### `BuildTelegramMessage_WithValidNotification_ContainsProjectNameAndVersion`
Verifies that a valid notification includes the project name and version in the Telegram message.

### `BuildTelegramMessage_WithCommitDetailsEnabled_IncludesCommitInfo`
Checks that when commit details are enabled, the Telegram message includes commit information.

### `BuildTelegramMessage_WithCommitDetailsDisabled_ExcludesCommitInfo`
Checks that when commit details are disabled, the Telegram message excludes commit information.

### `BuildTelegramMessage_WithDuration_IncludesDurationInfo`
Validates that the Telegram message includes deployment duration information when available.

### `BuildTelegramMessage_WithBuildUrlEnabled_IncludesBuildUrl`
Ensures that when the build URL is enabled, the Telegram message includes the build URL.

### `BuildTelegramMessage_WithEmojisEnabled_IncludesStatusEmoji`
Confirms that when emojis are enabled, the Telegram message includes status emojis.

### `BuildSlackPayload_WithDefaultSettings_ReturnsAttachmentFormat`
Verifies that with default Slack settings, the payload returns the expected attachment format.

### `BuildSlackPayload_WithBlockKitEnabled_ReturnsBlockKitFormat`
Ensures that when Block Kit formatting is enabled for Slack, the payload returns the Block Kit format.

### `BuildSlackPayload_WithEmojisEnabled_IncludesStatusEmoji`
Confirms that when emojis are enabled for Slack, the payload includes status emojis.

### `BuildDiscordPayload_WithValidNotification_ReturnsValidPayload`
Validates that a valid Discord notification produces a correctly formatted payload.

### `BuildDiscordPayload_WithDifferentStatuses_ReturnsValidPayload`
Ensures that Discord payloads are correctly formatted regardless of deployment status.

## Usage
