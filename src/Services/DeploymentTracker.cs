#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Tracks deployment lifecycle events and maintains deployment state.
/// </summary>
public sealed class DeploymentTracker : IDisposable
{
    private readonly ILogger<DeploymentTracker> _logger;
    private readonly ConcurrentDictionary<string, DeploymentRecord> _deployments = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentTracker"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public DeploymentTracker(ILogger<DeploymentTracker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Begins tracking a new deployment.
    /// </summary>
    /// <param name="deploymentId">Unique deployment identifier.</param>
    /// <param name="environment">Target environment name.</param>
    /// <param name="version">Version being deployed.</param>
    /// <returns>The created deployment record.</returns>
    public DeploymentRecord StartTracking(string deploymentId, string environment, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        var record = new DeploymentRecord
        {
            DeploymentId = deploymentId,
            Environment = environment,
            Version = version,
            Status = DeploymentStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow
        };

        if (!_deployments.TryAdd(deploymentId, record))
        {
            _logger.LogWarning("Deployment {DeploymentId} is already being tracked", deploymentId);
            throw new InvalidOperationException($"Deployment '{deploymentId}' is already tracked.");
        }

        _logger.LogInformation("Started tracking deployment {DeploymentId} to {Environment} (v{Version})",
            deploymentId, environment, version);
        return record;
    }

    /// <summary>
    /// Marks a deployment as completed successfully.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    public void MarkCompleted(string deploymentId)
    {
        if (_deployments.TryGetValue(deploymentId, out var record))
        {
            record.Status = DeploymentStatus.Completed;
            record.CompletedAt = DateTimeOffset.UtcNow;
            _logger.LogInformation("Deployment {DeploymentId} completed in {Duration}",
                deploymentId, record.CompletedAt - record.StartedAt);
        }
        else
        {
            _logger.LogWarning("Cannot mark unknown deployment {DeploymentId} as completed", deploymentId);
        }
    }

    /// <summary>
    /// Marks a deployment as failed with an error message.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <param name="error">Error description.</param>
    public void MarkFailed(string deploymentId, string error)
    {
        if (_deployments.TryGetValue(deploymentId, out var record))
        {
            record.Status = DeploymentStatus.Failed;
            record.CompletedAt = DateTimeOffset.UtcNow;
            record.ErrorMessage = error;
            _logger.LogError("Deployment {DeploymentId} failed: {Error}", deploymentId, error);
        }
    }

    /// <summary>
    /// Returns the current record for a tracked deployment.
    /// </summary>
    /// <param name="deploymentId">The deployment identifier.</param>
    /// <returns>The deployment record, or null if not found.</returns>
    public DeploymentRecord? GetDeployment(string deploymentId)
    {
        _deployments.TryGetValue(deploymentId, out var record);
        return record;
    }

    /// <summary>
    /// Lists all deployments matching the given status.
    /// </summary>
    /// <param name="status">Filter by status, or null for all.</param>
    /// <returns>Matching deployment records.</returns>
    public IReadOnlyList<DeploymentRecord> ListDeployments(DeploymentStatus? status = null)
    {
        var query = _deployments.Values.AsEnumerable();
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        return query.OrderByDescending(d => d.StartedAt).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _deployments.Clear();
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents the state of a tracked deployment.
/// </summary>
public sealed class DeploymentRecord
{
    /// <summary>Unique deployment identifier.</summary>
    public required string DeploymentId { get; init; }

    /// <summary>Target environment.</summary>
    public required string Environment { get; init; }

    /// <summary>Version being deployed.</summary>
    public string? Version { get; init; }

    /// <summary>Current deployment status.</summary>
    public DeploymentStatus Status { get; set; }

    /// <summary>When the deployment started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When the deployment finished (success or failure).</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Possible states of a deployment.
/// </summary>
public enum DeploymentStatus
{
    /// <summary>Deployment is in progress.</summary>
    InProgress,

    /// <summary>Deployment completed successfully.</summary>
    Completed,

    /// <summary>Deployment failed.</summary>
    Failed,

    /// <summary>Deployment was rolled back.</summary>
    RolledBack
}
