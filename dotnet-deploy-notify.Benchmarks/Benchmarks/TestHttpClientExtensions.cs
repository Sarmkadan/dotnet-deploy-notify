using Microsoft.Extensions.Logging;
using System.Net;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Extension methods for TestHttpClient to simplify common webhook testing scenarios
/// </summary>
public static class TestHttpClientExtensions
{
    /// <summary>
    /// Sets up the TestHttpClient to simulate a successful webhook response for the given URL pattern
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="urlPattern">URL pattern to match for successful responses (e.g., "valid", "test")</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="urlPattern"/> is null or whitespace</exception>
    public static void SetupSuccessResponse(this TestHttpClient client, string urlPattern)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(urlPattern);

        // The mock behavior is configured in MockHttpMessageHandler based on URL patterns
        // This method provides a convenient way to document the expected behavior
    }

    /// <summary>
    /// Sets up the TestHttpClient to simulate a specific HTTP status code response
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="statusCode">The HTTP status code to return</param>
    /// <param name="responseContent">Optional response content</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/></exception>
    public static void SetupStatusCodeResponse(this TestHttpClient client, HttpStatusCode statusCode, string? responseContent = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        // The actual status code simulation is handled by MockHttpMessageHandler
        // This extension provides a clean API for test setup
        responseContent ??= "{\"ok\":true}";
    }

    /// <summary>
    /// Creates a logger with the TestLogger implementation for benchmarking
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="categoryName">Logger category name</param>
    /// <returns>Configured ILogger instance</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/></exception>
    public static ILogger CreateTestLogger(this TestHttpClient client, string categoryName)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryName);

        var loggerFactory = new TestLoggerFactory();
        return loggerFactory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Creates a scope for logging that automatically disposes when the returned value is disposed
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="state">The state object</param>
    /// <typeparam name="TState">The type of the state object</typeparam>
    /// <returns>IDisposable scope that should be disposed</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/></exception>
    public static IDisposable BeginTestScope<TState>(this TestHttpClient client, TState state)
    {
        ArgumentNullException.ThrowIfNull(client);

        // The TestLogger.BeginScope always returns null, but this provides a consistent API
        // This method is kept for API consistency even though it's not used in the current implementation
        return NullDisposable.Instance;
    }

    /// <summary>
    /// Null disposable implementation for when BeginScope returns null
    /// </summary>
    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();

        private NullDisposable() { }

        public void Dispose() { }
    }
}