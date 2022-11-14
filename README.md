// existing content ...

## ServiceExtensions

The `ServiceExtensions` class provides extension methods for analyzing and manipulating deployment notifications and channel configurations. It includes methods to determine notification severity, check environment/status compatibility, format statuses for display, clone notifications, and calculate retry delays. These extensions simplify common operations in notification processing pipelines.

Example usage:
```csharp
var notification = new DeploymentNotification
{
    Status = BuildStatus.DeploymentFailed,
    TargetEnvironment = Environment.Production,
    Priority = NotificationPriority.High
};

var channelConfig = new ChannelConfiguration
{
    AllowedStatuses = new List<BuildStatus> { BuildStatus.DeploymentFailed, BuildStatus.Success },
    AllowedEnvironments = new List<Environment> { Environment.Production }
};

bool isCritical = notification.IsCritical(); // true (status is DeploymentFailed)
bool isProd = notification.IsProduction(); // true (environment is Production)
bool supportsStatus = channelConfig.SupportsStatus(notification.Status); // true
bool supportsEnv = channelConfig.SupportsEnvironment(notification.TargetEnvironment); // true
string statusDesc = notification.Status.GetDescription(); // "Deployment failed"
string envDesc = notification.TargetEnvironment.GetDescription(); // "Production"

if (isCritical && supportsStatus && supportsEnv)
{
    var cloned = notification.Clone();
    cloned.MergeMetadata(new DeploymentNotification { Metadata = { { "alert", "urgent" } } });
    string logEntry = cloned.ToCompactString(); // "[DeploymentFailed] MyProject@1.0.0 (Production/main)"
}
```
```