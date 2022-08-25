# NotificationProcessorTestsExtensions

The `NotificationProcessorTestsExtensions` class provides a set of static factory methods designed to simplify the creation of test data for unit tests within the `dotnet-deploy-notify` project. Each method returns a new instance of a core domain type with sensible defaults, allowing test authors to quickly set up scenarios without manually constructing complex objects. The class is intended for use in test projects only and is not part of the production API.

## API

### `public static ProcessingResult CreateProcessingResult()`

Creates a new `ProcessingResult` instance with default values suitable for testing.

- **Parameters**: None.
- **Return value**: A new `ProcessingResult` object.
- **Throws**: This method does not throw exceptions.

### `public static ProcessingStatistics CreateProcessingStatistics()`

Creates a new `ProcessingStatistics` instance with default values suitable for testing.

- **Parameters**: None.
- **Return value**: A new `ProcessingStatistics` object.
- **Throws**: This method does not throw exceptions.

### `public static DeploymentNotification CreateDeploymentNotification()`

Creates a new `DeploymentNotification` instance with default values suitable for testing.

- **Parameters**: None.
- **Return value**: A new `DeploymentNotification` object.
- **Throws**: This method does not throw exceptions.

### `public static NotificationResult CreateNotificationResult()`

Creates a new `NotificationResult` instance with default values suitable for testing.

- **Parameters**: None.
- **Return value**: A new `NotificationResult` object.
- **Throws**: This method does not throw exceptions.

## Usage

The following examples demonstrate typical usage of `NotificationProcessorTestsExtensions` in xUnit and NUnit test fixtures.

**Example 1: Verifying processing statistics after a notification**

```csharp
using Xunit;

public class NotificationProcessorTests
{
    [Fact]
    public void Process_WithValidNotification_UpdatesStatistics()
    {
        // Arrange
        var notification = NotificationProcessorTestsExtensions.CreateDeploymentNotification();
        var processor = new NotificationProcessor();

        // Act
        var result = processor.Process(notification);

        // Assert
        Assert.Equal(ProcessingStatus.Success, result.Status);
        Assert.NotNull(result.Statistics);
    }
}
```

**Example 2: Combining factory methods to build a complete test scenario**

```csharp
using NUnit.Framework;

[TestFixture]
public class NotificationResultHandlerTests
{
    [Test]
    public void HandleResult_WithSuccessfulProcessing_ReturnsExpectedOutcome()
    {
        // Arrange
        var processingResult = NotificationProcessorTestsExtensions.CreateProcessingResult();
        var statistics = NotificationProcessorTestsExtensions.CreateProcessingStatistics();
        var notificationResult = NotificationProcessorTestsExtensions.CreateNotificationResult();

        var handler = new NotificationResultHandler();

        // Act
        var outcome = handler.Handle(processingResult, statistics, notificationResult);

        // Assert
        Assert.IsTrue(outcome.IsSuccess);
    }
}
```

## Notes

- Each factory method returns a **new, independent instance** every time it is called. There is no caching or shared state between calls.
- The default values provided by these methods are intended to represent a “happy path” scenario. Tests that require specific edge cases (e.g., null fields, boundary values) should construct objects manually or use a builder pattern.
- Because the methods are static and do not access any mutable shared state, they are **thread-safe**. Multiple threads can call these methods concurrently without synchronization.
- No exceptions are thrown by these methods under normal circumstances. If the underlying types’ constructors throw (e.g., due to validation), that behavior would be inherited, but the factory methods themselves do not introduce additional failure modes.
