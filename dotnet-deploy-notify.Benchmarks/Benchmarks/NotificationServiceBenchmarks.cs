#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Performance benchmarks for critical operations in dotnet-deploy-notify
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Benchmarks;

/// <summary>
/// Benchmarks for NotificationService operations - measures throughput and memory allocations
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class NotificationServiceBenchmarks
{
    private INotificationService _notificationService = null!;
    private INotificationRepository _notificationRepository = null!;
    private IChannelConfigRepository _configRepository = null!;
    private INotificationResultRepository _resultRepository = null!;
    private IWebhookDispatcher _webhookDispatcher = null!;
    private IValidationService _validationService = null!;
    private ILogger<NotificationService> _logger = null!;
    private ILogger<WebhookDispatcher> _dispatcherLogger = null!;
    private ILogger<PayloadBuilder> _payloadLogger = null!;
    private IPayloadBuilder _payloadBuilder = null!;

    /// <summary>
    /// Setup test dependencies before each benchmark
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Create logger instances
        var loggerFactory = new TestLoggerFactory();
        _logger = loggerFactory.CreateLogger<NotificationService>();
        _dispatcherLogger = loggerFactory.CreateLogger<WebhookDispatcher>();
        _payloadLogger = loggerFactory.CreateLogger<PayloadBuilder>();

        // Create repositories
        _notificationRepository = new NotificationRepository(_logger);
        _configRepository = new ChannelConfigRepository(_logger);
        _resultRepository = new NotificationResultRepository(_logger);

        // Create payload builder
        _payloadBuilder = new PayloadBuilder(_payloadLogger);

        // Create webhook dispatcher with mock HttpClient
        var httpClient = new TestHttpClient();
        _webhookDispatcher = new WebhookDispatcher(httpClient, _dispatcherLogger, _payloadBuilder);

        // Create validation service
        _validationService = new ValidationService();

        // Create notification service
        _notificationService = new NotificationService(
            _notificationRepository,
            _configRepository,
            _resultRepository,
            _webhookDispatcher,
            _validationService,
            _logger
        );

        // Seed test data
        SeedTestData();
    }

    /// <summary>
    /// Cleanup after each benchmark
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        // Clean up test data
    }

    /// <summary>
    /// Seeds the repositories with test data for benchmarking
    /// </summary>
    private void SeedTestData()
    {
        // Create test channel configurations
        for (int i = 0; i < 5; i++)
        {
            var config = new ChannelConfiguration
            {
                DisplayName = $"Test Channel {i}",
                ChannelType = (NotificationChannel)(i % 3), // Round-robin through Slack, Discord, Telegram
                WebhookUrl = "https://hooks.slack.com/test",
                TargetId = $"#channel-{i}",
                IncludeCommitDetails = true,
                IncludeBuildUrl = true,
                MinimumPriority = NotificationPriority.Low,
                MaxRetries = 3,
                TimeoutMs = 10000,
                IsEnabled = true
            };
            _configRepository.CreateAsync(config).GetAwaiter().GetResult();
        }

        // Create test notifications
        for (int i = 0; i < 100; i++)
        {
            var notification = new DeploymentNotification
            {
                ProjectName = $"TestProject-{i % 10}",
                Version = $"1.0.{i}",
                Status = i % 2 == 0 ? BuildStatus.DeploymentSuccess : BuildStatus.Failed,
                Message = $"Test deployment message {i}",
                TargetEnvironment = i % 3 == 0 ? Environment.Production : Environment.Staging,
                BranchName = $"main",
                CommitHash = Guid.NewGuid().ToString()[..8],
                CommitAuthor = $"author{i}@example.com",
                RepositoryUrl = "https://github.com/test/repo",
                BuildUrl = "https://ci.example.com/builds/12345",
                DurationSeconds = i % 300,
                Priority = i % 3 == 0 ? NotificationPriority.Critical : NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { (NotificationChannel)(i % 3) }
            };
            _notificationRepository.CreateAsync(notification).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Benchmark: Single notification creation throughput
    /// Measures the time to create a single notification
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Creation")]
    public async Task CreateSingleNotification()
    {
        var notification = new DeploymentNotification
        {
            ProjectName = "BenchmarkProject",
            Version = "1.0.0",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Benchmark notification",
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "abc12345",
            CommitAuthor = "benchmark@example.com",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/builds/12345",
            DurationSeconds = 120,
            Priority = NotificationPriority.High,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord }
        };

        await _notificationService.CreateNotificationAsync(notification);
    }

    /// <summary>
    /// Benchmark: Batch notification creation (100 notifications)
    /// Measures throughput for creating multiple notifications in bulk
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Creation")]
    public async Task CreateBatchNotifications()
    {
        var tasks = new List<Task<string>>();

        for (int i = 0; i < 100; i++)
        {
            var notification = new DeploymentNotification
            {
                ProjectName = $"BatchProject-{i}",
                Version = $"1.0.{i}",
                Status = BuildStatus.DeploymentSuccess,
                Message = $"Batch notification {i}",
                TargetEnvironment = Environment.Production,
                BranchName = "main",
                CommitHash = Guid.NewGuid().ToString()[..8],
                CommitAuthor = $"author{i}@example.com",
                RepositoryUrl = "https://github.com/test/repo",
                BuildUrl = "https://ci.example.com/builds/12345",
                DurationSeconds = i % 300,
                Priority = i % 3 == 0 ? NotificationPriority.Critical : NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { NotificationChannel.Slack }
            };

            tasks.Add(_notificationService.CreateNotificationAsync(notification));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: Send pending notifications
    /// Measures the time to process and send all pending notifications
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Processing")]
    public async Task SendPendingNotifications()
    {
        await _notificationService.SendPendingNotificationsAsync();
    }

    /// <summary>
    /// Benchmark: Send notification to specific channels
    /// Measures the time to send a notification to configured channels
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Processing")]
    public async Task SendNotificationToChannels()
    {
        // Get the first pending notification
        var pending = await _notificationRepository.GetPendingAsync();
        if (pending.Count > 0)
        {
            await _notificationService.SendNotificationAsync(pending[0].Id);
        }
    }

    /// <summary>
    /// Benchmark: Get notification history by project
    /// Measures query performance for retrieving notification history
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public async Task GetNotificationHistory()
    {
        await _notificationService.GetNotificationHistoryAsync("TestProject-0", 50);
    }

    /// <summary>
    /// Benchmark: Get delivery results for notification
    /// Measures query performance for delivery results
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Query")]
    public async Task GetDeliveryResults()
    {
        // Get a notification with results
        var notifications = await _notificationRepository.GetAllAsync();
        if (notifications.Count > 0)
        {
            await _notificationService.GetDeliveryResultsAsync(notifications[0].Id);
        }
    }

    /// <summary>
    /// Benchmark: Retry failed deliveries
    /// Measures the time to retry failed delivery attempts
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Processing")]
    public async Task RetryFailedDeliveries()
    {
        // Get a notification with failed results
        var notifications = await _notificationRepository.GetAllAsync();
        if (notifications.Count > 0)
        {
            await _notificationService.RetryFailedDeliveriesAsync(notifications[0].Id);
        }
    }
}

/// <summary>
/// Mock HttpClient for benchmarking that simulates webhook responses
/// </summary>
public class TestHttpClient : HttpClient
{
    public TestHttpClient()
    {
        // Setup mock handler
        var handler = new MockHttpMessageHandler();
        BaseAddress = new Uri("https://hooks.slack.com/test");
        this.SetupFakeRequest(handler);
    }
}

/// <summary>
/// Mock HttpMessageHandler that returns successful responses
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Simulate network delay
        await Task.Delay(50, cancellationToken);

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ok\":true}")
        };
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