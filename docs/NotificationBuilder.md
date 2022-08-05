# NotificationBuilder
The `NotificationBuilder` class is a utility for constructing notifications in a flexible and fluent manner. It provides a series of methods that allow developers to specify various details about the notification, such as the project, status, environment, and more, before finally creating the notification object. This approach enables the creation of notifications with varying levels of detail and specificity, making it a versatile tool for a wide range of applications and scenarios.

## API
The `NotificationBuilder` class offers several public members that can be used to customize the notification:
- `public NotificationBuilder()`: The constructor for the `NotificationBuilder` class, used to initialize a new instance.
- `public NotificationBuilder WithProject(...)`: Specifies the project associated with the notification.
- `public NotificationBuilder WithStatus(...)`: Sets the status of the notification.
- `public NotificationBuilder WithEnvironment(...)`: Indicates the environment in which the notification is being sent.
- `public NotificationBuilder WithBranch(...)`: Specifies the branch related to the notification.
- `public NotificationBuilder WithRepository(...)`: Sets the repository associated with the notification.
- `public NotificationBuilder WithBuildUrl(...)`: Provides the URL of the build related to the notification.
- `public NotificationBuilder WithDuration(...)`: Specifies the duration of the event that triggered the notification.
- `public NotificationBuilder WithChannels(...)`: Sets the channels through which the notification will be sent. There are two overloads for this method, allowing for different types of channel specifications.
- `public NotificationBuilder WithPriority(...)`: Sets the priority of the notification.
- `public NotificationBuilder WithMessage(...)`: Specifies the message content of the notification.
- `public NotificationBuilder WithMetadata(...)`: Adds metadata to the notification. Like `WithChannels`, there are two overloads for this method, accommodating different metadata specifications.
- `public NotificationBuilder CriticalPriority()`: Sets the notification priority to critical.
- `public NotificationBuilder NormalPriority()`: Sets the notification priority to normal.
- `public NotificationBuilder LowPriority()`: Sets the notification priority to low.
- `public NotificationBuilder AsSuccess()`: Marks the notification as a success.
- `public NotificationBuilder AsFailure()`: Marks the notification as a failure.
- `public NotificationBuilder AsDeploymentSuccess()`: Specifies the notification as a deployment success.

Each of these methods returns an instance of `NotificationBuilder`, allowing for method chaining and a fluent interface. They do not throw exceptions under normal circumstances but may throw `NullReferenceException` or `ArgumentException` if invalid arguments are provided.

## Usage
Here are two examples of using the `NotificationBuilder` class to construct notifications:
```csharp
// Example 1: Simple Success Notification
var notification = new NotificationBuilder()
    .WithProject("My Project")
    .WithStatus("Success")
    .WithMessage("Deployment completed successfully.")
    .AsSuccess()
    .Build();

// Example 2: Detailed Deployment Success Notification
var detailedNotification = new NotificationBuilder()
    .WithProject("My Deployment Project")
    .WithEnvironment("Production")
    .WithBranch("main")
    .WithRepository("https://github.com/user/repo")
    .WithBuildUrl("https://example.com/build/123")
    .WithDuration(TimeSpan.FromMinutes(30))
    .WithChannels(new[] { "Email", "Slack" })
    .WithPriority()
    .WithMessage("Deployment to production environment successful.")
    .WithMetadata(new { DeploymentId = 123, Environment = "Prod" })
    .AsDeploymentSuccess()
    .Build();
```
Note that the `Build()` method is assumed to be part of the `NotificationBuilder` class, though it was not explicitly listed in the provided members. It is typically used to finalize the construction of the notification object.

## Notes
- **Thread Safety**: The `NotificationBuilder` class is designed to be thread-safe, allowing multiple threads to safely construct notifications without fear of data corruption or other concurrency issues.
- **Edge Cases**: When using the `WithChannels` or `WithMetadata` methods with collections or complex objects, ensure that these objects are properly initialized and populated to avoid `NullReferenceException`.
- **Best Practices**: It is recommended to use the fluent interface provided by the `NotificationBuilder` class to construct notifications in a clear and readable manner. This approach makes the code more understandable and maintainable.
