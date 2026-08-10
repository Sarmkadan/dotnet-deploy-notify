#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Fluent builder for constructing DeploymentNotification instances
/// </summary>
public class NotificationBuilder
{
    private readonly DeploymentNotification _notification;

    public NotificationBuilder()
    {
        _notification = new DeploymentNotification
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public NotificationBuilder WithProject(string? projectName, string? version)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(version);
        _notification.ProjectName = projectName;
        _notification.Version = version;
        return this;
    }

    public NotificationBuilder WithStatus(BuildStatus status, string? message = null)
    {
        _notification.Status = status;
        if (!string.IsNullOrWhiteSpace(message))
            _notification.Message = message;
        return this;
    }

    public NotificationBuilder WithEnvironment(Environment environment)
    {
        _notification.TargetEnvironment = environment;
        return this;
    }

    public NotificationBuilder WithBranch(string branchName, string? commitHash = null, string? author = null)
    {
        _notification.BranchName = branchName;
        if (!string.IsNullOrWhiteSpace(commitHash))
            _notification.CommitHash = commitHash;
        if (!string.IsNullOrWhiteSpace(author))
            _notification.CommitAuthor = author;
        return this;
    }

    public NotificationBuilder WithRepository(string url)
    {
        _notification.RepositoryUrl = url;
        return this;
    }

    public NotificationBuilder WithBuildUrl(string url)
    {
        _notification.BuildUrl = url;
        return this;
    }

    public NotificationBuilder WithDuration(int seconds)
    {
        _notification.DurationSeconds = seconds;
        return this;
    }

    public NotificationBuilder WithChannels(params NotificationChannel[] channels)
    {
        _notification.Channels = channels.ToList();
        return this;
    }

    public NotificationBuilder WithChannels(IEnumerable<NotificationChannel> channels)
    {
        _notification.Channels = channels.ToList();
        return this;
    }

    public NotificationBuilder WithPriority(NotificationPriority priority)
    {
        _notification.Priority = priority;
        return this;
    }

    public NotificationBuilder WithMessage(string message)
    {
        _notification.Message = message;
        return this;
    }

    public NotificationBuilder WithMetadata(string key, object value)
    {
        _notification.Metadata[key] = value;
        return this;
    }

    public NotificationBuilder WithMetadata(Dictionary<string, object> metadata)
    {
        foreach (var kvp in metadata)
            _notification.Metadata[kvp.Key] = kvp.Value;
        return this;
    }

    public NotificationBuilder CriticalPriority()
    {
        _notification.Priority = NotificationPriority.Critical;
        return this;
    }

    public NotificationBuilder NormalPriority()
    {
        _notification.Priority = NotificationPriority.Normal;
        return this;
    }

    public NotificationBuilder LowPriority()
    {
        _notification.Priority = NotificationPriority.Low;
        return this;
    }

    public NotificationBuilder AsSuccess()
    {
        _notification.Status = BuildStatus.Success;
        _notification.Priority = NotificationPriority.Normal;
        return this;
    }

    public NotificationBuilder AsFailure()
    {
        _notification.Status = BuildStatus.Failed;
        _notification.Priority = NotificationPriority.Critical;
        return this;
    }

    public NotificationBuilder AsDeploymentSuccess()
    {
        _notification.Status = BuildStatus.DeploymentSuccess;
        _notification.Priority = NotificationPriority.High;
        return this;
    }

    public NotificationBuilder AsDeploymentFailure()
    {
        _notification.Status = BuildStatus.DeploymentFailed;
        _notification.Priority = NotificationPriority.Critical;
        return this;
    }

    public DeploymentNotification Build()
    {
        if (!_notification.IsValid())
            throw new InvalidOperationException("Notification is not valid - missing required fields");

        return _notification;
    }

    public DeploymentNotification BuildUnsafe()
    {
        return _notification;
    }
}

/// <summary>
/// Template-based notification builder for common scenarios
/// </summary>
public class NotificationTemplate
{
    public static NotificationBuilder BuildSuccess(string projectName, string version, Environment environment)
    {
        return new NotificationBuilder()
            .WithProject(projectName, version)
            .WithEnvironment(environment)
            .AsSuccess()
            .WithMessage($"✅ {projectName} v{version} deployed successfully to {environment}");
    }

    public static NotificationBuilder BuildFailure(string projectName, string version, string errorMessage)
    {
        return new NotificationBuilder()
            .WithProject(projectName, version)
            .AsFailure()
            .WithMessage($"❌ Build failed: {errorMessage}");
    }

    public static NotificationBuilder BuildDeploymentSuccess(string projectName, string version, Environment environment)
    {
        return new NotificationBuilder()
            .WithProject(projectName, version)
            .WithEnvironment(environment)
            .AsDeploymentSuccess()
            .WithMessage($"🚀 {projectName} v{version} deployed to {environment}");
    }

    public static NotificationBuilder BuildDeploymentFailure(string projectName, string version, string errorMessage)
    {
        return new NotificationBuilder()
            .WithProject(projectName, version)
            .WithEnvironment(Environment.Production)
            .AsDeploymentFailure()
            .WithMessage($"💥 Deployment failed: {errorMessage}");
    }

    public static NotificationBuilder BuildUnitTestFailure(string projectName, int failedTests, string details)
    {
        return new NotificationBuilder()
            .WithProject(projectName, "")
            .AsFailure()
            .WithMessage($"⚠️ {failedTests} unit tests failed:\n{details}");
    }

    public static NotificationBuilder BuildHealthCheck(string serviceName, bool isHealthy)
    {
        var builder = new NotificationBuilder()
            .WithProject(serviceName, "")
            .WithEnvironment(Environment.Production);

        return isHealthy
            ? builder.AsSuccess().WithMessage($"✅ {serviceName} health check passed")
            : builder.AsFailure().WithMessage($"❌ {serviceName} health check failed");
    }
}
