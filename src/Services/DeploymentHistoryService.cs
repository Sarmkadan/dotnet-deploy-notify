#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Persistence;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Tracks deployment events and exposes history and aggregated statistics
/// </summary>
public interface IDeploymentHistoryService
{
    /// <summary>Records a deployment event in persistent history</summary>
    Task RecordDeploymentAsync(DeploymentHistoryEntry entry);

    /// <summary>Records a deployment event derived from a notification</summary>
    Task RecordFromNotificationAsync(DeploymentNotification notification);

    /// <summary>Returns deployment history for the given project, newest first</summary>
    Task<List<DeploymentHistoryEntry>> GetProjectHistoryAsync(string projectName, int limit = 50);

    /// <summary>Returns recent deployments across all projects, newest first</summary>
    Task<List<DeploymentHistoryEntry>> GetRecentDeploymentsAsync(int limit = 20);

    /// <summary>Returns aggregated statistics for the given project</summary>
    Task<DeploymentStatistics> GetStatisticsAsync(string projectName);

    /// <summary>Returns history entries filtered by environment</summary>
    Task<List<DeploymentHistoryEntry>> GetByEnvironmentAsync(Environment environment, int limit = 50);

    /// <summary>Returns the most recent successful deployment for the given project and environment</summary>
    Task<DeploymentHistoryEntry?> GetLastSuccessfulDeploymentAsync(string projectName, Environment environment);

    /// <summary>Returns all rollback entries for the given project</summary>
    Task<List<DeploymentHistoryEntry>> GetRollbackEntriesAsync(string projectName, int limit = 50);
}

/// <summary>
/// Implementation of <see cref="IDeploymentHistoryService"/> backed by a pluggable
/// <see cref="IDeploymentHistoryRepository"/>. Statistics and filtering are computed here; durability
/// is delegated entirely to the repository, so the service behaves identically whether history is kept
/// in memory or persisted to disk
/// </summary>
public sealed class DeploymentHistoryService : IDeploymentHistoryService
{
    private readonly IDeploymentHistoryRepository _repository;
    private readonly ILogger<DeploymentHistoryService> _logger;

    /// <summary>Initialises the service with an in-memory, process-local repository</summary>
    public DeploymentHistoryService(ILogger<DeploymentHistoryService> logger)
        : this(logger, new InMemoryDeploymentHistoryRepository())
    {
    }

    /// <summary>Initialises the service with its dependencies</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is <see langword="null"/></exception>
    public DeploymentHistoryService(ILogger<DeploymentHistoryService> logger, IDeploymentHistoryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        _logger = logger;
        _repository = repository;
    }

    /// <summary>
    /// Stores a deployment history entry
    /// </summary>
    public async Task RecordDeploymentAsync(DeploymentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (string.IsNullOrWhiteSpace(entry.ProjectName))
            throw new ArgumentException("ProjectName is required", nameof(entry));

        await _repository.AddAsync(entry).ConfigureAwait(false);

        _logger.LogInformation(
            "Recorded deployment: {Project} v{Version} [{Status}] on {Environment}",
            entry.ProjectName, entry.Version, entry.FinalStatus, entry.TargetEnvironment);
    }

    /// <summary>
    /// Derives and stores a history entry from an existing notification
    /// </summary>
    public Task RecordFromNotificationAsync(DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var entry = DeploymentHistoryEntry.FromNotification(notification);
        return RecordDeploymentAsync(entry);
    }

    /// <summary>
    /// Returns deployment history for a project ordered most-recent first
    /// </summary>
    public async Task<List<DeploymentHistoryEntry>> GetProjectHistoryAsync(string projectName, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name must not be empty", nameof(projectName));

        var entries = await _repository.GetAllAsync().ConfigureAwait(false);
        var results = entries
            .Where(e => string.Equals(e.ProjectName, projectName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.DeployedAt)
            .Take(limit)
            .ToList();

        _logger.LogDebug("Retrieved {Count} history entries for {Project}", results.Count, projectName);
        return results;
    }

    /// <summary>
    /// Returns the most recent deployments across all projects
    /// </summary>
    public async Task<List<DeploymentHistoryEntry>> GetRecentDeploymentsAsync(int limit = 20)
    {
        var results = await _repository.GetRecentAsync(limit).ConfigureAwait(false);
        return results.ToList();
    }

    /// <summary>
    /// Computes aggregated statistics for the given project
    /// </summary>
    public async Task<DeploymentStatistics> GetStatisticsAsync(string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName))
            throw new ArgumentException("Project name must not be empty", nameof(projectName));

        var entries = await _repository.GetAllAsync().ConfigureAwait(false);
        var projectEntries = entries
            .Where(e => string.Equals(e.ProjectName, projectName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var successful = projectEntries.Count(e => e.IsSuccessful);
        var failed = projectEntries.Count(e =>
            e.FinalStatus is BuildStatus.Failed or BuildStatus.DeploymentFailed or BuildStatus.Cancelled);
        var rollbacks = projectEntries.Count(e => e.IsRollback);

        var durationsWithValues = projectEntries
            .Where(e => e.DurationSeconds.HasValue)
            .Select(e => (double)e.DurationSeconds!.Value)
            .ToList();

        var avgDuration = durationsWithValues.Count > 0
            ? durationsWithValues.Average()
            : (double?)null;

        var mostRecentEntry = projectEntries
            .OrderByDescending(e => e.DeployedAt)
            .FirstOrDefault();

        var mostActiveEnvironment = projectEntries
            .GroupBy(e => e.TargetEnvironment.ToString())
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var stats = new DeploymentStatistics
        {
            ProjectName = projectName,
            TotalDeployments = projectEntries.Count,
            SuccessfulDeployments = successful,
            FailedDeployments = failed,
            RollbackCount = rollbacks,
            AverageDurationSeconds = avgDuration,
            LastDeployedAt = mostRecentEntry?.DeployedAt,
            LastVersion = mostRecentEntry?.Version,
            MostActiveEnvironment = mostActiveEnvironment
        };

        return stats;
    }

    /// <summary>
    /// Returns history entries for a specific environment
    /// </summary>
    public async Task<List<DeploymentHistoryEntry>> GetByEnvironmentAsync(Environment environment, int limit = 50)
    {
        var entries = await _repository.GetAllAsync().ConfigureAwait(false);
        var results = entries
            .Where(e => e.TargetEnvironment == environment)
            .OrderByDescending(e => e.DeployedAt)
            .Take(limit)
            .ToList();

        return results;
    }

    /// <summary>
    /// Returns the last successfully completed deployment for a project/environment pair
    /// </summary>
    public async Task<DeploymentHistoryEntry?> GetLastSuccessfulDeploymentAsync(string projectName, Environment environment)
    {
        var entries = await _repository.GetAllAsync().ConfigureAwait(false);
        var result = entries
            .Where(e =>
                string.Equals(e.ProjectName, projectName, StringComparison.OrdinalIgnoreCase) &&
                e.TargetEnvironment == environment &&
                e.IsSuccessful)
            .OrderByDescending(e => e.DeployedAt)
            .FirstOrDefault();

        return result;
    }

    /// <summary>
    /// Returns rollback entries for a project
    /// </summary>
    public async Task<List<DeploymentHistoryEntry>> GetRollbackEntriesAsync(string projectName, int limit = 50)
    {
        var entries = await _repository.GetAllAsync().ConfigureAwait(false);
        var results = entries
            .Where(e =>
                string.Equals(e.ProjectName, projectName, StringComparison.OrdinalIgnoreCase) &&
                e.IsRollback)
            .OrderByDescending(e => e.DeployedAt)
            .Take(limit)
            .ToList();

        return results;
    }
}
