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
/// Extension methods for request context
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
    public static void ExecuteInContext(Action<RequestContext> action)
    {
        using var scope = new RequestContextScope();
        action(scope.Context);
    }

    /// <summary>
    /// Executes an async action within a request context
    /// </summary>
    public static async Task ExecuteInContextAsync(Func<RequestContext, Task> action)
    {
        using var scope = new RequestContextScope();
        await action(scope.Context);
    }

    /// <summary>
    /// Executes a function within a request context and returns the result
    /// </summary>
    public static T ExecuteInContext<T>(Func<RequestContext, T> func)
    {
        using var scope = new RequestContextScope();
        return func(scope.Context);
    }

    /// <summary>
    /// Executes an async function within a request context and returns the result
    /// </summary>
    public static async Task<T> ExecuteInContextAsync<T>(Func<RequestContext, Task<T>> func)
    {
        using var scope = new RequestContextScope();
        return await func(scope.Context);
    }
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
