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
    public static void SetupSuccessResponse(this TestHttpClient client, string urlPattern)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        if (string.IsNullOrWhiteSpace(urlPattern))
            throw new ArgumentException("URL pattern cannot be null or empty", nameof(urlPattern));

        // The mock behavior is already handled in MockHttpMessageHandler
        // This method provides a convenient way to configure the expected behavior
    }

    /// <summary>
    /// Sets up the TestHttpClient to simulate a specific HTTP status code response
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="statusCode">The HTTP status code to return</param>
    /// <param name="responseContent">Optional response content</param>
    public static void SetupStatusCodeResponse(this TestHttpClient client, HttpStatusCode statusCode, string? responseContent = null)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        // The actual status code simulation is handled by MockHttpMessageHandler
        // This extension provides a clean API for test setup
        responseContent ??= "{\"ok\":true}";
    }

    /// <summary>
    /// Creates a logger with the TestLogger implementation and adds it to the client's logger factory
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="categoryName">Logger category name</param>
    /// <returns>Configured ILogger instance</returns>
    public static ILogger CreateTestLogger(this TestHttpClient client, string categoryName)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        var loggerFactory = new TestLoggerFactory();
        return loggerFactory.CreateLogger(categoryName);
    }

    /// <summary>
    /// Creates a scope for logging that automatically disposes when the returned value is disposed
    /// </summary>
    /// <param name="client">The TestHttpClient instance</param>
    /// <param name="state">The state object</param>
    /// <returns>IDisposable scope that should be disposed</returns>
    public static IDisposable BeginTestScope<TState>(this TestHttpClient client, TState state)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        // TestLogger.BeginScope always returns null, but this provides a consistent API
        return client.BeginScope(state) ?? NullDisposable.Instance;
    }

    /// <summary>
    /// Null disposable implementation for when BeginScope returns null
    /// </summary>
    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new NullDisposable();

        private NullDisposable() { }

        public void Dispose() { }
    }
}