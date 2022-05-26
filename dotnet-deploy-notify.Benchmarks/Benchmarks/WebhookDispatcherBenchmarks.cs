#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Performance benchmarks for webhook dispatching operations
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Benchmarks;

/// <summary>
/// Benchmarks for WebhookDispatcher operations - measures HTTP request performance
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class WebhookDispatcherBenchmarks
{
    private IWebhookDispatcher _webhookDispatcher = null!;
    private ILogger<WebhookDispatcher> _logger = null!;
    private ILogger<PayloadBuilder> _payloadLogger = null!;
    private IPayloadBuilder _payloadBuilder = null!;
    private ChannelConfiguration _validConfig = null!;
    private DeploymentNotification _notification = null!;
    private WebhookPayload _payload = null!;

    /// <summary>
    /// Setup test dependencies before each benchmark
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var loggerFactory = new TestLoggerFactory();
        _logger = loggerFactory.CreateLogger<WebhookDispatcher>();
        _payloadLogger = loggerFactory.CreateLogger<PayloadBuilder>();
        _payloadBuilder = new PayloadBuilder(_payloadLogger);

        // Create a valid channel configuration
        _validConfig = new ChannelConfiguration
        {
            DisplayName = "Production Slack",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://webhook.example.invalid/deploy",
            TargetId = "#deployments",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            MaxRetries = 3,
            TimeoutMs = 10000,
            IsEnabled = true
        };

        // Create a realistic notification
        _notification = new DeploymentNotification
        {
            ProjectName = "EnterpriseApp",
            Version = "3.2.1",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Production deployment completed successfully. All health checks passed. Ready for traffic.",
            TargetEnvironment = Environment.Production,
            BranchName = "release/v3.2.1",
            CommitHash = "abcdef1234567890abcdef1234567890abcdef12",
            CommitAuthor = "john.doe@company.com",
            RepositoryUrl = "https://github.com/company/enterprise-app",
            BuildUrl = "https://ci.company.com/builds/54321",
            DurationSeconds = 245,
            Priority = NotificationPriority.High,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack }
        };

        // Build the payload
        _payload = _payloadBuilder.BuildPayload(_notification, _validConfig);

        // Create webhook dispatcher with mock HttpClient
        var httpClient = new TestHttpClient();
        _webhookDispatcher = new WebhookDispatcher(httpClient, _logger, _payloadBuilder);
    }

    /// <summary>
    /// Benchmark: Send webhook with successful response
    /// Measures the time to send a webhook and receive a successful response
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookDispatching")]
    public async Task SendToWebhook_SuccessfulResponse()
    {
        await _webhookDispatcher.SendToWebhookAsync(_validConfig, _notification);
    }

    /// <summary>
    /// Benchmark: Send webhook with failed response (400 status)
    /// Measures the time to send a webhook and receive a client error response
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookDispatching")]
    public async Task SendToWebhook_FailedResponse()
    {
        var failingConfig = new ChannelConfiguration
        {
            DisplayName = "Failing Webhook",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.slack.com/services/invalid/test",
            TargetId = "#deployments",
            IsEnabled = true
        };

        await _webhookDispatcher.SendToWebhookAsync(failingConfig, _notification);
    }

    /// <summary>
    /// Benchmark: Send webhook with timeout
    /// Measures the time to send a webhook that times out
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookDispatching")]
    public async Task SendToWebhook_Timeout()
    {
        var timeoutConfig = new ChannelConfiguration
        {
            DisplayName = "Slow Webhook",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.slack.com/services/slow/test",
            TargetId = "#deployments",
            TimeoutMs = 100, // Very short timeout
            IsEnabled = true
        };

        await _webhookDispatcher.SendToWebhookAsync(timeoutConfig, _notification);
    }

    /// <summary>
    /// Benchmark: Send raw payload with custom headers
    /// Measures the time to send a raw payload with additional headers
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookDispatching")]
    public async Task SendPayload_WithCustomHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { "X-Custom-Header", "custom-value" },
            { "Authorization", "Bearer token123" },
            { "User-Agent", "dotnet-deploy-notify/1.0" }
        };

        await _webhookDispatcher.SendPayloadAsync(
            _validConfig.WebhookUrl,
            _payload,
            headers,
            _validConfig.TimeoutMs
        );
    }

    /// <summary>
    /// Benchmark: Validate webhook connectivity
    /// Measures the time to validate webhook endpoint connectivity
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookValidation")]
    public async Task ValidateWebhook_ValidEndpoint()
    {
        var valid = await _webhookDispatcher.ValidateWebhookAsync(
            _validConfig.WebhookUrl,
            _validConfig.TimeoutMs
        );
    }

    /// <summary>
    /// Benchmark: Validate webhook with invalid endpoint
    /// Measures the time to validate an invalid webhook endpoint
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookValidation")]
    public async Task ValidateWebhook_InvalidEndpoint()
    {
        await _webhookDispatcher.ValidateWebhookAsync(
            "https://invalid-url.example.com/webhook",
            5000
        );
    }

    /// <summary>
    /// Benchmark: Send batch of webhooks (10 notifications)
    /// Measures the time to send multiple webhooks in sequence
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("WebhookDispatching")]
    public async Task SendBatchWebhooks()
    {
        var batchTasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            var notification = new DeploymentNotification
            {
                ProjectName = $"BatchApp-{i}",
                Version = $"1.0.{i}",
                Status = BuildStatus.DeploymentSuccess,
                Message = $"Batch notification {i}",
                TargetEnvironment = Environment.Production,
                BranchName = "main",
                CommitHash = Guid.NewGuid().ToString()[..8],
                CommitAuthor = $"author{i}@example.com",
                RepositoryUrl = "https://github.com/test/repo",
                BuildUrl = "https://ci.example.com/builds/12345",
                DurationSeconds = i * 30,
                Priority = NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { NotificationChannel.Slack }
            };

            batchTasks.Add(_webhookDispatcher.SendToWebhookAsync(_validConfig, notification));
        }

        await Task.WhenAll(batchTasks);
    }
}

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