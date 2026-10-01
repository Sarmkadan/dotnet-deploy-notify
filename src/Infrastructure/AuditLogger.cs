#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Records an immutable audit trail of deployment-related actions for compliance and debugging.
/// </summary>
public sealed class AuditLogger : IDisposable
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly ConcurrentQueue<AuditEntry> _entries = new();
    private readonly string? _outputPath;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogger"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="outputPath">Optional file path for persisting audit entries.</param>
    public AuditLogger(ILogger<AuditLogger> logger, string? outputPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _outputPath = outputPath;
    }

    /// <summary>
    /// Records an audit entry for a deployment action.
    /// </summary>
    /// <param name="action">The action performed (e.g., "deploy", "rollback", "config_change").</param>
    /// <param name="actor">Who performed the action.</param>
    /// <param name="details">Human-readable description.</param>
    /// <param name="metadata">Optional structured metadata.</param>
    public void LogAction(string action, string actor, string details, Dictionary<string, object>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Action = action,
            Actor = actor,
            Details = details,
            Metadata = metadata ?? [],
            Timestamp = DateTimeOffset.UtcNow
        };

        _entries.Enqueue(entry);
        _logger.LogInformation("[AUDIT] {Action} by {Actor}: {Details}", action, actor, details);
    }

    /// <summary>
    /// Returns all audit entries recorded since startup, newest first.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return.</param>
    /// <returns>Audit entries.</returns>
    public IReadOnlyList<AuditEntry> GetEntries(int limit = 100)
    {
        return _entries
            .Reverse()
            .Take(limit)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns entries filtered by action type.
    /// </summary>
    /// <param name="action">Action type to filter by.</param>
    /// <returns>Matching audit entries.</returns>
    public IReadOnlyList<AuditEntry> GetEntriesByAction(string action)
    {
        return _entries
            .Where(e => string.Equals(e.Action, action, StringComparison.OrdinalIgnoreCase))
            .Reverse()
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Flushes all buffered entries to the configured output file as JSON lines.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entries written.</returns>
    public async Task<int> FlushToFileAsync(CancellationToken cancellationToken = default)
    {
        var path = _outputPath ?? throw new InvalidOperationException("No output path configured for audit log.");
        var entries = _entries.ToArray();

        await using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        foreach (var entry in entries)
        {
            var line = JsonSerializer.Serialize(entry, options);
            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }

        _logger.LogInformation("Flushed {Count} audit entries to {Path}", entries.Length, path);
        return entries.Length;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a single immutable audit log entry.
/// </summary>
public sealed class AuditEntry
{
    /// <summary>Unique entry identifier.</summary>
    public required string Id { get; init; }

    /// <summary>The action that was performed.</summary>
    public required string Action { get; init; }

    /// <summary>The actor who performed the action.</summary>
    public required string Actor { get; init; }

    /// <summary>Human-readable description of what happened.</summary>
    public string? Details { get; init; }

    /// <summary>Structured metadata associated with the entry.</summary>
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>When the action occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }
}
