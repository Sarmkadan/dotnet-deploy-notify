#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents a single recorded deployment event in the history log
/// </summary>
public sealed class DeploymentHistoryEntry
{
    /// <summary>Unique identifier for this history record</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Project or application name</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Version that was deployed</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Final resolved status of the deployment</summary>
    public BuildStatus FinalStatus { get; set; }

    /// <summary>Target deployment environment</summary>
    public Environment TargetEnvironment { get; set; }

    /// <summary>Git branch name</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Git commit hash at time of deployment</summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>Author of the commit being deployed</summary>
    public string CommitAuthor { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the deployment completed</summary>
    public DateTime DeployedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Total deployment duration in seconds</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>Error or failure details, if applicable</summary>
    public string? ErrorDetails { get; set; }

    /// <summary>Whether this entry represents a rollback deployment</summary>
    public bool IsRollback { get; set; }

    /// <summary>Version that was active before this deployment (populated for rollbacks)</summary>
    public string? RolledBackFromVersion { get; set; }

    /// <summary>Optional user-defined tags for filtering and categorisation</summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Returns true when the deployment completed without errors
    /// </summary>
    public bool IsSuccessful =>
        FinalStatus is BuildStatus.Success or BuildStatus.DeploymentSuccess;

    /// <summary>
    /// Creates a <see cref="DeploymentHistoryEntry"/> from a processed <see cref="DeploymentNotification"/>
    /// </summary>
    public static DeploymentHistoryEntry FromNotification(DeploymentNotification notification)
    {
        return new DeploymentHistoryEntry
        {
            ProjectName    = notification.ProjectName,
            Version        = notification.Version,
            FinalStatus    = notification.Status,
            TargetEnvironment = notification.TargetEnvironment,
            BranchName     = notification.BranchName,
            CommitHash     = notification.CommitHash,
            CommitAuthor   = notification.CommitAuthor,
            DeployedAt     = notification.CreatedAt,
            DurationSeconds = notification.DurationSeconds,
            IsRollback     = notification.Metadata.ContainsKey("RollbackFromVersion"),
            RolledBackFromVersion = notification.GetMetadata<string>("RollbackFromVersion")
        };
    }
}

/// <summary>
/// Aggregated deployment statistics for a project
/// </summary>
public sealed class DeploymentStatistics
{
    /// <summary>Name of the project these statistics relate to</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Total number of recorded deployments</summary>
    public int TotalDeployments { get; set; }

    /// <summary>Number of deployments that completed successfully</summary>
    public int SuccessfulDeployments { get; set; }

    /// <summary>Number of deployments that failed</summary>
    public int FailedDeployments { get; set; }

    /// <summary>Number of rollback operations recorded</summary>
    public int RollbackCount { get; set; }

    /// <summary>Percentage of deployments that succeeded (0–100)</summary>
    public double SuccessRate =>
        TotalDeployments == 0 ? 0 : Math.Round((double)SuccessfulDeployments / TotalDeployments * 100, 2);

    /// <summary>Mean duration across all timed deployments, in seconds</summary>
    public double? AverageDurationSeconds { get; set; }

    /// <summary>UTC timestamp of the most recent deployment</summary>
    public DateTime? LastDeployedAt { get; set; }

    /// <summary>Version string of the most recent deployment</summary>
    public string? LastVersion { get; set; }

    /// <summary>Environment that received the most deployments</summary>
    public string? MostActiveEnvironment { get; set; }
}
