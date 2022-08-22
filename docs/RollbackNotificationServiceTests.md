# RollbackNotificationServiceTests

Unit test class for the `RollbackNotificationService` in the **dotnet-deploy-notify** project. It verifies that rollback notification formatting and delivery behave correctly across supported platforms (Slack, Discord, Telegram) and that the service handles history retrieval, error conditions, and priority settings as expected.

## API

### RollbackNotificationServiceTests  
Public test class containing the test methods below. No constructor parameters; instances are stateless and safe to create per test run.

### FormatRollbackMessage_Slack_ContainsMarkdown  
**Purpose:** Confirms that the message formatter produces Slack‑compatible markdown when targeting Slack.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** May throw an exception from the test framework if the formatted string does not contain expected markdown syntax.

### FormatRollbackMessage_Discord_ContainsBoldMarkdown  
**Purpose:** Verifies that the formatted message for Discord includes bold markdown (`**text**`).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws if the result lacks the expected bold markup.

### FormatRollbackMessage_Telegram_ContainsHtmlTags  
**Purpose:** Ensures that the Telegram formatter wraps content in appropriate HTML tags (e.g., `<b>`, `<i>`).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws when the output does not contain the required HTML tags.

### FormatRollbackMessage_Generic_ContainsProjectInfo  
**Purpose:** Checks that a generic (fallback) formatter includes the project name and version in the output.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws if project information is missing from the formatted message.

### FormatRollbackMessage_WithReason_IncludesReason  
**Purpose:** Validates that when a rollback reason is supplied, it appears verbatim in the formatted message.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws if the reason string is not found in the result.

### FormatRollbackMessage_WithAdditionalDetails_IncludesDetails  
**Purpose:** Confirms that additional details passed to the formatter are incorporated into the final message.  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws when the details are absent from the formatted output.

### FormatRollbackMessage_UsesCorrectEmoji  
**Purpose:** Asserts that the formatter selects the appropriate emoji based on rollback status (initiated, completed, failed).  
**Parameters:** None.  
**Return value:** `void`.  
**Throws:** Throws if the expected emoji character is not present.

### NotifyRollbackInitiatedAsync_CallsNotificationService  
**Purpose:** Ensures that invoking `NotifyRollbackInitiatedAsync` delegates the call to the underlying notification service mock.  
**Parameters:** None.  
**Return value:** `Task`. Completes when the verification finishes.  
**Throws:** May propagate exceptions from the mock setup or from the test assertion layer.

### NotifyRollbackInitiatedAsync_WithNullRequest_ThrowsArgumentNullException  
**Purpose:** Confirms that a `null` request argument causes the method to throw `ArgumentNullException`.  
**Parameters:** None.  
**Return value:** `Task`. Completes after the exception is verified.  
**Throws:** The test throws if the method does not throw the expected exception.

### NotifyRollbackInitiatedAsync_ReturnsDeliveryResults  
**Purpose:** Checks that the method returns a delivery result object reflecting the outcome of the notification attempts.  
**Parameters:** None.  
**Return value:** `Task`. Completes with the returned delivery results.  
**Throws:** Throws if the returned result is `null` or does not match the test method.

### NotifyRollbackCompletedAsync_SendsCompletionNotification  
**Purpose:** Verifies that a completed rollback triggers a notification with the correct completion template.  
**Parameters:** None.  
**Return value:** `Task`. Completes after the send operation is validated.  
**Throws:** Throws if the notification service mock does not receive the expected completion message.

### NotifyRollbackFailedAsync_SendsFailureNotification  
**Purpose:** Ensures that a failed rollback results in a failure‑oriented notification being dispatched.  
**Parameters:** None.  
**Return value:** `Task`. Completes after verifying the failure message was sent.  
**Throws:** Throws when the failure notification is not observed on the mock.

### NotifyRollbackFailedAsync_SetsCriticalPriority  
**Purpose:** Confirms that failure notifications are marked with critical priority in the delivery request.  
**Parameters:** None.  
**Return value:** `Task`. Completes after checking the priority flag.  
**Throws:** Throws if the priority is not set to critical.

### GetRollbackNotificationHistoryAsync_RecordsAfterDispatch  
**Purpose:** Asserts that each dispatched notification is recorded in the internal history store.  
**Parameters:** None.  
**Return value:** `Task`. Completes after the history is inspected.  
**Throws:** Throws when the history does not contain the expected entry.

### GetRollbackNotificationHistoryAsync_RespectsLimit  
**Purpose:** Verifies that the `limit` argument correctly truncates the returned history collection.  
**Parameters:** None.  
**Return value:** `Task`. Completes after confirming the count matches the limit.  
**Throws:** Throws if more items than the limit are returned.

### GetRollbackNotificationHistoryAsync_FiltersByProject  
**Purpose:** Ensures that invoking the history query with a project identifier returns only entries for that project.  
**Parameters:** None.  
**Return value:** `Task`. Completes after validating the filter.  
**Throws:** Throws when entries for other projects appear in the result.

## Usage

```csharp
using Xunit;
using Moq;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Models;

public class ExampleTests
{
    [Fact]
    public void VerifySlackFormatting()
    {
        // Arrange
        var formatter = new RollbackNotificationServiceTests(); // test class provides static helpers
        // Act
        var result = RollbackNotificationServiceTests.FormatRollbackMessage_Slack_ContainsMarkdown(); // hypothetical static call
        // Assert – the test method itself performs assertions; here we just show invocation
        Assert.True(true); // placeholder
    }
}
```

```csharp
using System.Threading.Tasks;
using Xunit;
using Moq;
using DotNetDeployNotify.Services;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyRollbackInitiatedAsync_DeliversMessage()
    {
        // Arrange
        var mockNotifier = new Mock<INotificationService>();
        var service = new RollbackNotificationService(mockNotifier.Object);
        var request = new RollbackRequest { ProjectId = "AcmeWeb", Reason = "Failed health check" };

        // Act
        await service.NotifyRollbackInitiatedAsync(request);

        // Assert
        mockNotifier.Verify(n => n.SendAsync(It.IsAny<NotificationPayload>()), Times.Once);
    }
}
```

## Notes

- The test class contains no mutable state; each method relies only on its parameters and any mocks supplied by the test framework. Consequently, instances are thread‑safe and can be executed in parallel test runners without interference.  
- Methods that accept a `null` request (e.g., `NotifyRollbackInitiatedAsync_WithNullRequest_ThrowsArgumentNullException`) are designed to validate argument‑guard clauses; passing `null` in production will result in an `ArgumentNullException`.  
- History‑related tests assume an in‑memory or mock repository; they do not persist data beyond the test scope.  
- Emoji and platform‑specific formatting checks are culture‑insensitive; they rely on literal Unicode characters defined in the service implementation.  
- All asynchronous test methods return a `Task` to enable `await` in test runners; they do not expose any return value to production code.  
- Because these are unit tests, they should be executed only within a test project; invoking them from production code will not produce meaningful behavior and may raise exceptions due to missing test‑framework attributes.
