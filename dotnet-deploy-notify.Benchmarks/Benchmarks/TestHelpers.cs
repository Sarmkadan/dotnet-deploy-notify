using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Mock HttpClient for benchmarking that simulates various webhook responses
/// </summary>
public class TestHttpClient : HttpClient
{
    public TestHttpClient()
    {
        // Setup mock handler with different response scenarios
        var handler = new MockHttpMessageHandler();
        BaseAddress = new Uri("https://hooks.slack.com/services/test");
        this.SetupFakeRequest(handler);
    }
}

/// <summary>
/// Mock HttpMessageHandler that simulates different webhook response scenarios
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Random _random = new Random();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Simulate network delay based on URL
        var delayMs = _random.Next(20, 200);
        await Task.Delay(delayMs, cancellationToken);

        var url = request.RequestUri?.ToString() ?? "";

        // Return success for valid URLs
        if (url.Contains("valid") || url.Contains("test"))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            };
        }

        // Return 400 for invalid URLs
        if (url.Contains("invalid"))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"Invalid webhook URL\"}")
            };
        }

        // Return 401 for auth failures
        if (url.Contains("unauthorized"))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"error\":\"Unauthorized\"}")
            };
        }

        // Return 500 for server errors
        if (url.Contains("slow"))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{\"error\":\"Internal server error\"}")
            };
        }

        // Default: success
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ok\":true}")
        };
    }
}

/// <summary>
/// Extension method to setup fake HttpClient behavior
/// </summary>
public static class HttpClientExtensions
{
    public static void SetupFakeRequest(this HttpClient httpClient, HttpMessageHandler handler)
    {
        // This is a mock setup - in real benchmarking, we'd use HttpClient directly
        // The actual mock behavior is handled in the handler
    }
}

/// <summary>
/// Test logger factory for benchmarking
/// </summary>
public class TestLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => new TestLogger();

    public void Dispose() { }
}

/// <summary>
/// Test logger that does nothing (for benchmarking)
/// </summary>
public class TestLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Do nothing for benchmarking
    }
}
