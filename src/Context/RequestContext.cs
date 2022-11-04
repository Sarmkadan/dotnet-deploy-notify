#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Context;

/// <summary>
/// Represents the context of a request execution
/// </summary>
public sealed class RequestContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string? ClientId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public int ExecutionTimeMs { get; set; }

    /// <summary>
    /// Sets a value in the context metadata
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    /// <summary>
    /// Gets a value from the context metadata
    /// </summary>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Checks if a metadata key exists
    /// </summary>
    public bool HasMetadata(string key) => Metadata.ContainsKey(key);
}

/// <summary>
/// Ambient context for request tracking
/// </summary>
public static class AmbientRequestContext
{
    private static readonly AsyncLocal<RequestContext?> _context = new();

    public static RequestContext Current
    {
        get => _context.Value ??= new RequestContext();
        set => _context.Value = value;
    }

    public static void SetContext(RequestContext context)
    {
        _context.Value = context;
    }

    public static void ClearContext()
    {
        _context.Value = null;
    }

    public static void Reset()
    {
        _context.Value = new RequestContext();
    }
}

/// <summary>
/// Scope for managing request context lifecycle
/// </summary>
public sealed class RequestContextScope : IDisposable
{
    private readonly RequestContext _previousContext;

    public RequestContext Context { get; }

    public RequestContextScope()
    {
        _previousContext = AmbientRequestContext.Current;
        Context = new RequestContext();
        AmbientRequestContext.SetContext(Context);
    }

    public RequestContextScope(RequestContext context)
    {
        _previousContext = AmbientRequestContext.Current;
        Context = context;
        AmbientRequestContext.SetContext(context);
    }

    public void Dispose()
    {
        AmbientRequestContext.SetContext(_previousContext);
    }
}

/// <summary>
/// Extension methods for <see cref="RequestContext"/> that provide additional functionality
/// </summary>
public static class RequestContextExtensions
{
    /// <summary>
    /// Gets the current request context or creates a new one
    /// </summary>
    public static RequestContext GetOrCreateContext()
    {
        return AmbientRequestContext.Current;
    }

    /// <summary>
    /// Executes an action within a request context
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static void ExecuteInContext(Action<RequestContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var scope = new RequestContextScope();
        action(scope.Context);
    }

    /// <summary>
    /// Executes an async action within a request context
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    public static async Task ExecuteInContextAsync(Func<RequestContext, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var scope = new RequestContextScope();
        await action(scope.Context);
    }

    /// <summary>
    /// Executes a function within a request context and returns the result
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    public static T ExecuteInContext<T>(Func<RequestContext, T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        using var scope = new RequestContextScope();
        return func(scope.Context);
    }

    /// <summary>
    /// Executes an async function within a request context and returns the result
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <returns>The result of the function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is null.</exception>
    public static async Task<T> ExecuteInContextAsync<T>(Func<RequestContext, Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        using var scope = new RequestContextScope();
        return await func(scope.Context);
    }

    /// <summary>
    /// Gets the request duration as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>The duration as a <see cref="TimeSpan"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static TimeSpan GetRequestDuration(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return TimeSpan.FromMilliseconds(context.ExecutionTimeMs);
    }

    /// <summary>
    /// Gets the request start time as UTC.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>The request start time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static DateTime GetRequestStartTime(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.RequestTime;
    }

    /// <summary>
    /// Gets the request end time as UTC.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>The request end time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static DateTime GetRequestEndTime(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.RequestTime.AddMilliseconds(context.ExecutionTimeMs);
    }

    /// <summary>
    /// Gets the request duration formatted as a human-readable string.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>A formatted duration string (e.g., "1.2s", "45ms").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string GetFormattedDuration(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var duration = TimeSpan.FromMilliseconds(context.ExecutionTimeMs);
        return duration.TotalSeconds >= 1.0
            ? $"{duration.TotalSeconds:F1}s"
            : $"{duration.TotalMilliseconds}ms";
    }

    /// <summary>
    /// Adds a metadata entry with the current timestamp.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="timestampKind">The kind of timestamp to record.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="key"/> is null.</exception>
    public static void AddTimestampMetadata(this RequestContext context, string key, TimestampKind timestampKind = TimestampKind.RequestStart)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(key);

        context.SetMetadata(key, timestampKind switch
        {
            TimestampKind.RequestStart => context.RequestTime,
            TimestampKind.RequestEnd => context.GetRequestEndTime(),
            _ => DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets a metadata value as a string.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value as a string, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="key"/> is null.</exception>
    public static string? GetMetadataAsString(this RequestContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return context.GetMetadata<object>(key)?.ToString();
    }

    /// <summary>
    /// Checks if the context has a specific metadata key and removes it.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="key">The metadata key to remove.</param>
    /// <returns>True if the key existed and was removed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="key"/> is null.</exception>
    public static bool RemoveMetadata(this RequestContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return context.Metadata.Remove(key);
    }

    /// <summary>
    /// Clears all metadata from the context.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static void ClearMetadata(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Metadata.Clear();
    }

    /// <summary>
    /// Gets all metadata keys.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>An enumerable of metadata keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static IEnumerable<string> GetMetadataKeys(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Metadata.Keys;
    }

    /// <summary>
    /// Gets the number of metadata entries.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>The count of metadata entries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static int GetMetadataCount(this RequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Metadata.Count;
    }
}

/// <summary>
/// Specifies the kind of timestamp being recorded.
/// </summary>
public enum TimestampKind
{
    /// <summary>Request start time.</summary>
    RequestStart,

    /// <summary>Request end time.</summary>
    RequestEnd,

    /// <summary>Custom timestamp.</summary>
    Custom
}

/// <summary>
/// Structured logging helper for request context
/// </summary>
public sealed class RequestContextLogger
{
    private readonly ILogger _logger;

    public RequestContextLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogWithContext(LogLevel level, string message)
    {
        var context = AmbientRequestContext.Current;
        _logger.Log(level, "[{CorrelationId}] {Message}", context.CorrelationId, message);
    }

    public void LogInformationWithContext(string message)
    {
        LogWithContext(LogLevel.Information, message);
    }

    public void LogErrorWithContext(Exception ex, string message)
    {
        var context = AmbientRequestContext.Current;
        _logger.LogError(ex, "[{CorrelationId}] {Message}", context.CorrelationId, message);
    }

    public void LogWarningWithContext(string message)
    {
        LogWithContext(LogLevel.Warning, message);
    }

    public void LogDebugWithContext(string message)
    {
        LogWithContext(LogLevel.Debug, message);
    }
}
