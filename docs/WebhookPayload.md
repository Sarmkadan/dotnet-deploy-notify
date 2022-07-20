# WebhookPayload
The `WebhookPayload` type represents a payload of data sent via a webhook, typically used to notify of deployment events. It encapsulates various details about the deployment, such as the event type, timestamp, source, and data specific to the deployment. This type is crucial in the `dotnet-deploy-notify` project for handling and processing deployment notifications.

## API
The `WebhookPayload` type exposes the following public members:
* `EventId`: A unique identifier for the event.
* `EventType`: The type of event that triggered the webhook.
* `Timestamp`: The date and time the event occurred.
* `Source`: The source of the event.
* `SchemaVersion`: The version of the schema used for the payload.
* `Data`: An instance of `WebhookData` containing specific details about the deployment.
* `Errors`: A list of error messages, if any occurred during the deployment.
* `IsValid`: A boolean indicating whether the payload is valid.
* `ToJson`: A string representation of the payload in JSON format.
* `ProjectName`: The name of the project being deployed.
* `Version`: The version of the project being deployed.
* `Status`: The status of the deployment.
* `Message`: A message related to the deployment.
* `Environment`: The environment where the deployment occurred.
* `Branch`: The branch of the repository being deployed.
* `CommitHash`: The hash of the commit being deployed.
* `CommitAuthor`: The author of the commit being deployed.
* `RepositoryUrl`: The URL of the repository being deployed.
* `BuildUrl`: The URL of the build being deployed.
* `DurationSeconds`: The duration of the deployment in seconds, or null if not applicable.

## Usage
Here are two examples of using the `WebhookPayload` type in C#:
```csharp
// Example 1: Creating a new WebhookPayload instance
var payload = new WebhookPayload
{
    EventId = "12345",
    EventType = "DeploymentStarted",
    Timestamp = DateTime.UtcNow,
    Source = "Azure DevOps",
    SchemaVersion = "1.0",
    Data = new WebhookData { /* initialize WebhookData properties */ },
    ProjectName = "MyProject",
    Version = "1.2.3",
    Status = "In Progress",
    Environment = "Production"
};

// Example 2: Parsing a JSON payload and accessing its properties
var jsonPayload = "{\"EventId\":\"12345\",\"EventType\":\"DeploymentCompleted\",\"Timestamp\":\"2022-01-01T12:00:00Z\",\"Source\":\"Azure DevOps\",\"SchemaVersion\":\"1.0\",\"Data\":{\"/* initialize WebhookData properties */\"},\"ProjectName\":\"MyProject\",\"Version\":\"1.2.3\",\"Status\":\"Succeeded\",\"Environment\":\"Production\"}";
var payload = WebhookPayload.FromJson(jsonPayload);
Console.WriteLine(payload.EventId); // Output: 12345
Console.WriteLine(payload.EventType); // Output: DeploymentCompleted
```

## Notes
When working with `WebhookPayload` instances, consider the following:
* The `IsValid` property should be checked before attempting to access other properties, as it indicates whether the payload is in a valid state.
* The `Errors` list may contain error messages if the deployment failed or encountered issues.
* The `DurationSeconds` property may be null if the deployment duration is not applicable or not available.
* `WebhookPayload` instances are not thread-safe by default; if concurrent access is required, consider implementing synchronization mechanisms or using thread-safe alternatives.
* When parsing JSON payloads, ensure that the JSON string is well-formed and conforms to the expected schema to avoid errors or exceptions.
