#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Manages deployment rollback operations, including state snapshots and restoration.
/// </summary>
public sealed class RollbackManager
{
    private readonly ILogger<RollbackManager> _logger;
    private readonly Stack<RollbackSnapshot> _snapshots = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackManager"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public RollbackManager(ILogger<RollbackManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a snapshot of the current deployment state before applying changes.
    /// </summary>
    /// <param name="deploymentId">Deployment identifier.</param>
    /// <param name="environment">Target environment.</param>
    /// <param name="previousVersion">The version currently running.</param>
    /// <param name="metadata">Optional key-value metadata to preserve.</param>
    /// <returns>The created snapshot.</returns>
    public RollbackSnapshot CreateSnapshot(
        string deploymentId,
        string environment,
        string previousVersion,
        Dictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);

        var snapshot = new RollbackSnapshot
        {
            Id = Guid.NewGuid().ToString("N"),
            DeploymentId = deploymentId,
            Environment = environment,
            PreviousVersion = previousVersion,
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = metadata ?? []
        };

        lock (_lock)
        {
            _snapshots.Push(snapshot);
        }

        _logger.LogInformation("Created rollback snapshot {SnapshotId} for deployment {DeploymentId} (v{Version})",
            snapshot.Id, deploymentId, previousVersion);
        return snapshot;
    }

    /// <summary>
    /// Executes a rollback to the most recent snapshot for the given deployment.
    /// </summary>
    /// <param name="deploymentId">Deployment identifier to roll back.</param>
    /// <returns>The snapshot that was restored, or null if none available.</returns>
    public RollbackSnapshot? Rollback(string deploymentId)
    {
        lock (_lock)
        {
            var matching = new Stack<RollbackSnapshot>();
            RollbackSnapshot? target = null;

            while (_snapshots.Count > 0)
            {
                var snap = _snapshots.Pop();
                if (snap.DeploymentId == deploymentId && target is null)
                {
                    target = snap;
                    target.RestoredAt = DateTimeOffset.UtcNow;
                    break;
                }
                matching.Push(snap);
            }

            // Push non-matching ones back
            while (matching.Count > 0)
                _snapshots.Push(matching.Pop());

            if (target is not null)
            {
                _logger.LogWarning("Rolled back deployment {DeploymentId} to version {Version}",
                    deploymentId, target.PreviousVersion);
            }
            else
            {
                _logger.LogError("No rollback snapshot found for deployment {DeploymentId}", deploymentId);
            }

            return target;
        }
    }

    /// <summary>
    /// Checks whether a rollback snapshot exists for the specified deployment.
    /// </summary>
    /// <param name="deploymentId">Deployment identifier.</param>
    /// <returns>True if at least one snapshot exists.</returns>
    public bool CanRollback(string deploymentId)
    {
        lock (_lock)
        {
            return _snapshots.Any(s => s.DeploymentId == deploymentId);
        }
    }

    /// <summary>
    /// Returns the number of snapshots currently held in memory.
    /// </summary>
    public int SnapshotCount
    {
        get { lock (_lock) { return _snapshots.Count; } }
    }
}

/// <summary>
/// Represents a pre-deployment state snapshot used for rollback.
/// </summary>
public sealed class RollbackSnapshot
{
    /// <summary>Unique snapshot identifier.</summary>
    public required string Id { get; init; }

    /// <summary>Associated deployment identifier.</summary>
    public required string DeploymentId { get; init; }

    /// <summary>Target environment.</summary>
    public required string Environment { get; init; }

    /// <summary>The version that was running before deployment.</summary>
    public required string PreviousVersion { get; init; }

    /// <summary>When the snapshot was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the snapshot was restored (null if not yet rolled back).</summary>
    public DateTimeOffset? RestoredAt { get; set; }

    /// <summary>Arbitrary metadata preserved with the snapshot.</summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}
