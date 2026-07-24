#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Persistence;

/// <summary>
/// Provides durable storage for <see cref="DeploymentHistoryEntry"/> records, independent of the
/// higher-level statistics/filtering logic in <c>IDeploymentHistoryService</c>
/// </summary>
public interface IDeploymentHistoryRepository
{
    /// <summary>Appends a new deployment history entry to the store</summary>
    Task AddAsync(DeploymentHistoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Returns the most recent entries across all projects, newest first</summary>
    Task<IReadOnlyList<DeploymentHistoryEntry>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Returns the entry with the given deployment id, or <see langword="null"/> if none exists</summary>
    Task<DeploymentHistoryEntry?> GetByDeploymentIdAsync(string deploymentId, CancellationToken cancellationToken = default);

    /// <summary>Returns every stored entry in no particular order</summary>
    Task<IReadOnlyList<DeploymentHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Volatile, process-local implementation of <see cref="IDeploymentHistoryRepository"/>. History does
/// not survive a process restart - suitable for tests and for callers that opt out of file persistence
/// </summary>
public sealed class InMemoryDeploymentHistoryRepository : IDeploymentHistoryRepository
{
    private readonly List<DeploymentHistoryEntry> _entries = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Appends the entry to the in-memory store
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is <see langword="null"/></exception>
    public Task AddAsync(DeploymentHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_lockObject)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the most recent entries, newest first
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative</exception>
    public Task<IReadOnlyList<DeploymentHistoryEntry>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative");

        lock (_lockObject)
        {
            IReadOnlyList<DeploymentHistoryEntry> results = _entries
                .OrderByDescending(e => e.DeployedAt)
                .Take(count)
                .ToList();

            return Task.FromResult(results);
        }
    }

    /// <summary>
    /// Returns the entry matching the given id, or <see langword="null"/> if not found
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deploymentId"/> is <see langword="null"/> or empty</exception>
    public Task<DeploymentHistoryEntry?> GetByDeploymentIdAsync(string deploymentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(deploymentId);

        lock (_lockObject)
        {
            return Task.FromResult(_entries.FirstOrDefault(e => e.Id == deploymentId));
        }
    }

    /// <summary>
    /// Returns every stored entry
    /// </summary>
    public Task<IReadOnlyList<DeploymentHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            IReadOnlyList<DeploymentHistoryEntry> results = _entries.ToList();
            return Task.FromResult(results);
        }
    }
}

/// <summary>
/// JSON-file-backed implementation of <see cref="IDeploymentHistoryRepository"/>. The whole history is
/// held in a single file as a JSON array; every write reads the current file, appends in memory, then
/// replaces the file atomically via a temp-file-and-rename so a crash mid-write cannot corrupt existing
/// history. A per-instance semaphore serialises concurrent access from within the same process
/// </summary>
public sealed class JsonFileDeploymentHistoryRepository : IDeploymentHistoryRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    /// <summary>
    /// Initialises the repository against the given JSON history file. The file (and its parent
    /// directory) is created lazily on first write and does not need to exist beforehand
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/> or empty</exception>
    public JsonFileDeploymentHistoryRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        _filePath = filePath;
    }

    /// <summary>
    /// Appends the entry to the JSON file, rewriting it atomically
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is <see langword="null"/></exception>
    public async Task AddAsync(DeploymentHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var entries = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
            entries.Add(entry);
            await WriteAllAsync(entries, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Returns the most recent entries, newest first
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative</exception>
    public async Task<IReadOnlyList<DeploymentHistoryEntry>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative");

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var entries = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
            return entries
                .OrderByDescending(e => e.DeployedAt)
                .Take(count)
                .ToList();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Returns the entry matching the given id, or <see langword="null"/> if not found
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="deploymentId"/> is <see langword="null"/> or empty</exception>
    public async Task<DeploymentHistoryEntry?> GetByDeploymentIdAsync(string deploymentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(deploymentId);

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var entries = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
            return entries.FirstOrDefault(e => e.Id == deploymentId);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Returns every entry currently persisted in the file
    /// </summary>
    public async Task<IReadOnlyList<DeploymentHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ReadAllAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<DeploymentHistoryEntry>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return new List<DeploymentHistoryEntry>();

        await using var stream = File.OpenRead(_filePath);
        if (stream.Length == 0)
            return new List<DeploymentHistoryEntry>();

        var entries = await JsonSerializer
            .DeserializeAsync<List<DeploymentHistoryEntry>>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        return entries ?? new List<DeploymentHistoryEntry>();
    }

    private async Task WriteAllAsync(List<DeploymentHistoryEntry> entries, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, entries, SerializerOptions, cancellationToken).ConfigureAwait(false);
        }

        File.Move(tempPath, _filePath, overwrite: true);
    }
}
