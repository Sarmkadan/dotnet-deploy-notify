# RollbackRequest

The `RollbackRequest` type represents a request to roll back a deployment to a previous version of a project. It encapsulates metadata about the rollback request, including the target environment, versions involved, and notification preferences. This type is used to track and process rollback operations within the `dotnet-deploy-notify` system.

## API

### `Id`
A unique identifier for the rollback request. Used to track and reference the request throughout its lifecycle.

### `ProjectName`
The name of the project targeted by the rollback request. Identifies the project undergoing the rollback operation.

### `TargetVersion`
The version of the project to which the system should roll back. Specifies the desired state after the rollback operation.

### `CurrentVersion`
The version of the project currently deployed before the rollback operation. Provides context for the rollback operation.

### `TargetEnvironment`
The environment where the rollback should be applied (e.g., "Production", "Staging"). Determines the scope of the rollback operation.

### `RequestedBy`
The identifier of the user or system that initiated the rollback request. Used for auditing and accountability.

### `Reason`
A human-readable explanation for why the rollback is being requested. Provides context for stakeholders and system logs.

### `Channels`
A list of notification channels (e.g., Slack, Email) where updates about the rollback request should be sent. Enables multi-channel notifications for the rollback process.

### `Priority`
The priority level of the rollback request, indicating its urgency relative to other requests. Affects scheduling and resource allocation for the rollback operation.

### `CreatedAt`
The timestamp when the rollback request was created. Used for tracking the age and urgency of the request.

### `Metadata`
A dictionary of additional key-value pairs providing supplementary context for the rollback request. Allows for extensible data storage without modifying the type structure.

### `IsValid`
A boolean indicating whether the rollback request is valid and can be processed. Used to filter out malformed or incomplete requests before execution.

### `GetSummary`
A method that returns a human-readable summary of the rollback request, including key details such as project name, versions, and environment. Useful for logging and user interfaces.

### `RequestId`
A unique identifier for the rollback request, distinct from `Id`. Used internally for tracking the request in system logs and workflows.

### `RolledBackFromVersion`
The version from which the system is rolling back. Provides clarity on the starting point of the rollback operation.

### `RolledBackToVersion`
The version to which the system is rolling back. Specifies the target state of the rollback operation.

### `Status`
The current status of the rollback request (e.g., "Pending", "InProgress", "Completed", "Failed"). Tracks the progress and outcome of the rollback operation.

### `ErrorMessage`
An optional string containing an error message if the rollback request encountered a failure. Provides details for debugging and user feedback when the operation does not complete successfully.

## Usage

### Creating and Validating a Rollback Request
