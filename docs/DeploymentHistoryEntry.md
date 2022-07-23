# DeploymentHistoryEntry

Represents a single deployment event in the application’s history, capturing metadata such as project identity, build status, environment, timing, and related source control information. Used to track and audit deployments across environments.

## API

### `public string Id`
A unique identifier for the deployment entry. Serves as the primary key in storage.

### `public string ProjectName`
The name of the project being deployed. Must not be null or empty.

### `public string Version`
The version string of the deployed artifact (e.g., `1.2.3`). Must not be null or empty.

### `public BuildStatus FinalStatus`
The final outcome of the deployment (e.g., `Succeeded`, `Failed`, `Rollback`). Indicates whether the deployment completed successfully or encountered an error.

### `public Environment TargetEnvironment`
The target environment where the deployment occurred (e.g., `Development`, `Staging`, `Production`). Must be a valid environment value.

### `public string BranchName`
The name of the source control branch from which the deployment originated. Must not be null or empty.

### `public string CommitHash`
The full commit hash of the source control revision deployed. Must not be null or empty.

### `public string CommitAuthor`
The author of the commit being deployed. Must not be null or empty.

### `public DateTime DeployedAt`
The UTC timestamp when the deployment completed. Must be a valid date and time.

### `public int? DurationSeconds`
The duration of the deployment in seconds, if available. Null if the duration was not measured.

### `public string? ErrorDetails`
Detailed error message, if the deployment failed. Null if the deployment succeeded or no error occurred.

### `public bool IsRollback`
Indicates whether this deployment is a rollback to a previous version. True if a rollback occurred.

### `public string? RolledBackFromVersion`
The version string from which this deployment rolled back, if applicable. Null if not a rollback.

### `public Dictionary<string, string> Tags`
A collection of key-value pairs used to annotate the deployment with additional metadata (e.g., build number, pipeline ID, triggered by).

### `public static DeploymentHistoryEntry FromNotification(...)`
Constructs a `DeploymentHistoryEntry` from a deployment notification payload. Parameters and exact structure are defined by the notification schema; throws `ArgumentNullException` if required fields are missing or invalid.

### `public string ProjectName`
The name of the project being tracked. Must not be null or empty.

### `public int TotalDeployments`
The total number of deployments recorded for the project. Always non-negative.

### `public int SuccessfulDeployments`
The number of successful deployments for the project. Must be less than or equal to `TotalDeployments`.

### `public int FailedDeployments`
The number of failed deployments for the project. Must be less than or equal to `TotalDeployments`.

### `public int RollbackCount`
The number of rollback deployments for the project. Must be non-negative.

## Usage
