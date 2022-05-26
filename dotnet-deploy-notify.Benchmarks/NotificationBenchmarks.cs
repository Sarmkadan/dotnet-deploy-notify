using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks;

/// <summary>
/// Performance benchmarks for notification operations
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[MinColumn]
[MaxColumn]
public class NotificationBenchmarks
{
    private INotificationService _notificationService;
    private INotificationRepository _notificationRepository;
    private IChannelConfigRepository _configRepository;
    private INotificationResultRepository _resultRepository;
    private IWebhookDispatcher _webhookDispatcher;
    private ILoggerFactory _loggerFactory;
    private ILogger<NotificationService> _logger;
    private ILogger<WebhookDispatcher> _webhookLogger;
    private ILogger<NotificationRepository> _repoLogger;
    private ILogger<ChannelConfigRepository> _configLogger;
    private ILogger<NotificationResultRepository> _resultLogger;
    private ILogger<PayloadBuilder> _payloadLogger;

    /// <summary>
    /// Setup benchmark dependencies
    /// </summary>
    [GlobalSetup]
    public async Task Setup()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Error); // Reduce noise during benchmarks
        });

        _logger = _loggerFactory.CreateLogger<NotificationService>();
        _webhookLogger = _loggerFactory.CreateLogger<WebhookDispatcher>();
        _repoLogger = _loggerFactory.CreateLogger<NotificationRepository>();
        _configLogger = _loggerFactory.CreateLogger<ChannelConfigRepository>();
        _resultLogger = _loggerFactory.CreateLogger<NotificationResultRepository>();
        _payloadLogger = _loggerFactory.CreateLogger<PayloadBuilder>();

        // Create in-memory repositories
        _notificationRepository = new NotificationRepository(_repoLogger);
        _configRepository = new ChannelConfigRepository(_configLogger);
        _resultRepository = new NotificationResultRepository(_resultLogger);

        // Create HttpClient for webhook dispatcher
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        var payloadBuilder = new DotNetDeployNotify.Services.PayloadBuilder(_payloadLogger);
        _webhookDispatcher = new WebhookDispatcher(httpClient, _webhookLogger, payloadBuilder);

        // Create notification service
        _notificationService = new NotificationService(
            _notificationRepository,
            _configRepository,
            _resultRepository,
            _webhookDispatcher,
            new ValidationService(),
            _logger
        );

        // Pre-populate with test data
        await PreloadTestData();
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        _loggerFactory.Dispose();
    }

    /// <summary>
    /// Preload test data for benchmarking
    /// </summary>
    private async Task PreloadTestData()
    {
        // Create channel configurations
        for (int i = 0; i < 10; i++)
        {
            await _configRepository.CreateAsync(new ChannelConfiguration
            {
                DisplayName = $"Test Channel {i}",
                ChannelType = (NotificationChannel)(i % 4), // Cycle through Slack, Discord, Telegram, Webhook
                WebhookUrl = $"https://webhook.site/{Guid.NewGuid()}",
                TargetId = $"channel-{i}",
                IsEnabled = true,
                MaxRetries = 3,
                TimeoutMs = 10000
            });
        }

        // Create pending notifications
        for (int i = 0; i < 100; i++)
        {
            await _notificationService.CreateNotificationAsync(new DeploymentNotification
            {
                ProjectName = $"TestProject-{i % 10}",
                Version = $"1.0.{i}",
                Status = i % 2 == 0 ? BuildStatus.DeploymentSuccess : BuildStatus.Failed,
                Message = $"Test notification {i}",
                TargetEnvironment = i % 3 == 0 ? Environment.Production : Environment.Staging,
                BranchName = $"main",
                CommitHash = Guid.NewGuid().ToString()[..8],
                CommitAuthor = $"author{i}@example.com",
                RepositoryUrl = $"https://github.com/test/repo{i}",
                BuildUrl = $"https://ci.example.com/build/{i}",
                DurationSeconds = 120,
                Channels = new List<NotificationChannel> { (NotificationChannel)(i % 4) },
                Priority = (NotificationPriority)(i % 4)
            });
        }
    }

    /// <summary>
    /// Benchmark creating a single notification
    /// </summary>
    [Benchmark]
    public async Task CreateNotificationAsync()
    {
        await _notificationService.CreateNotificationAsync(new DeploymentNotification
        {
            ProjectName = "BenchmarkProject",
            Version = "1.0.0",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Benchmark notification",
            TargetEnvironment = Environment.Development,
            BranchName = "main",
            CommitHash = "abc12345",
            CommitAuthor = "benchmark@example.com",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/build/bench",
            DurationSeconds = 60,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack },
            Priority = NotificationPriority.Normal
        });
    }

    /// <summary>
    /// Benchmark creating multiple notifications in a batch
    /// </summary>
    [Benchmark]
    public async Task CreateNotificationsBatch()
    {
        for (int i = 0; i < 100; i++)
        {
            await _notificationService.CreateNotificationAsync(new DeploymentNotification
            {
                ProjectName = $"BatchProject-{i}",
                Version = $"1.0.{i}",
                Status = BuildStatus.DeploymentSuccess,
                Message = $"Batch notification {i}",
                TargetEnvironment = Environment.Development,
                BranchName = "main",
                CommitHash = Guid.NewGuid().ToString()[..8],
                CommitAuthor = $"batch{i}@example.com",
                RepositoryUrl = $"https://github.com/test/repo",
                BuildUrl = $"https://ci.example.com/build/batch",
                DurationSeconds = 60,
                Channels = new List<NotificationChannel> { NotificationChannel.Slack },
                Priority = NotificationPriority.Normal
            });
        }
    }

    /// <summary>
    /// Benchmark retrieving pending notifications
    /// </summary>
    [Benchmark]
    public async Task GetPendingNotifications()
    {
        await _notificationRepository.GetPendingAsync();
    }

    /// <summary>
    /// Benchmark retrieving notification history by project
    /// </summary>
    [Benchmark]
    public async Task GetNotificationHistory()
    {
        await _notificationService.GetNotificationHistoryAsync("TestProject-1", limit: 50);
    }

    /// <summary>
    /// Benchmark sending a single notification to webhook
    /// </summary>
    [Benchmark]
    public async Task SendToWebhookAsync()
    {
        var config = await _configRepository.GetByChannelAsync(NotificationChannel.Slack);
        if (config.Count > 0)
        {
            await _webhookDispatcher.SendToWebhookAsync(config[0], new DeploymentNotification
            {
                Id = Guid.NewGuid().ToString(),
                ProjectName = "WebhookTest",
                Version = "1.0.0",
                Status = BuildStatus.DeploymentSuccess,
                Message = "Webhook test notification",
                TargetEnvironment = Environment.Development,
                BranchName = "main",
                CommitHash = "webhook123",
                CommitAuthor = "webhook@example.com",
                RepositoryUrl = "https://github.com/test/repo",
                BuildUrl = "https://ci.example.com/build/webhook",
                DurationSeconds = 60,
                Channels = new List<NotificationChannel> { NotificationChannel.Slack },
                Priority = NotificationPriority.Normal
            });
        }
    }

    /// <summary>
    /// Benchmark sending multiple notifications (batch processing)
    /// </summary>
    [Benchmark]
    public async Task SendPendingNotificationsAsync()
    {
        await _notificationService.SendPendingNotificationsAsync();
    }

    /// <summary>
    /// Benchmark retrieving delivery results for a notification
    /// </summary>
    [Benchmark]
    public async Task GetDeliveryResults()
    {
        // Get a notification ID first
        var notifications = await _notificationRepository.GetAllAsync();
        if (notifications.Count > 0)
        {
            await _notificationService.GetDeliveryResultsAsync(notifications[0].Id);
        }
    }

    /// <summary>
    /// Benchmark creating notification with large payload
    /// </summary>
    [Benchmark]
    public async Task CreateNotificationWithLargePayload()
    {
        await _notificationService.CreateNotificationAsync(new DeploymentNotification
        {
            ProjectName = "LargePayloadProject",
            Version = "2.0.0",
            Status = BuildStatus.DeploymentSuccess,
            Message = "This is a very long message with lots of details about the deployment process including multiple lines of text that might be used in a real notification system to provide comprehensive information to the team about what happened during the deployment and any issues that were encountered along the way.",
            TargetEnvironment = Environment.Production,
            BranchName = "release/v2.0.0",
            CommitHash = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6",
            CommitAuthor = "john.doe@company.com",
            RepositoryUrl = "https://github.com/organization/large-repo-with-many-dependencies",
            BuildUrl = "https://jenkins.company.com/job/large-project/12345/",
            DurationSeconds = 945,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord, NotificationChannel.Telegram },
            Priority = NotificationPriority.Critical
        });
    }

    /// <summary>
    /// Benchmark getting notifications by environment
    /// </summary>
    [Benchmark]
    public async Task GetByEnvironmentAsync()
    {
        await _notificationRepository.GetByEnvironmentAsync(Environment.Production);
    }

    /// <summary>
    /// Benchmark getting notifications by status
    /// </summary>
    [Benchmark]
    public async Task GetByStatusAsync()
    {
        await _notificationRepository.GetByStatusAsync(BuildStatus.DeploymentSuccess, limit: 100);
    }
}
/// <summary>
/// Benchmark building webhook payload with different notification sizes
/// </summary>
[Benchmark]
public void BuildPayloadSmall()
{
    var notification = new DeploymentNotification
    {
        ProjectName = "TestProject",
        Version = "1.0.0",
        Status = BuildStatus.DeploymentSuccess,
        Message = "Test",
        TargetEnvironment = Environment.Development,
        BranchName = "main",
        CommitHash = "abc123",
        CommitAuthor = "test@example.com",
        RepositoryUrl = "https://github.com/test/repo",
        BuildUrl = "https://ci.example.com/build/1",
        DurationSeconds = 60,
        Channels = new List<NotificationChannel> { NotificationChannel.Slack }
    };

    var config = new ChannelConfiguration
    {
        ChannelType = NotificationChannel.Slack,
        WebhookUrl = "https://webhook.site/test"
    };

    var payloadBuilder = new PayloadBuilder(NullLogger<PayloadBuilder>.Instance);
    _ = payloadBuilder.BuildPayload(notification, config);
}

/// <summary>
/// Benchmark building webhook payload with large notification
/// </summary>
[Benchmark]
public void BuildPayloadLarge()
{
    var notification = new DeploymentNotification
    {
        ProjectName = "LargeProject",
        Version = "3.0.0",
        Status = BuildStatus.DeploymentSuccess,
        Message = "This is a very detailed message with extensive information about the deployment process including multiple paragraphs of text that would typically be used in production scenarios to provide comprehensive details to the team about what happened during the deployment cycle and any issues that were encountered along the way.",
        TargetEnvironment = Environment.Production,
        BranchName = "release/v3.0.0",
        CommitHash = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b",
        CommitAuthor = "john.doe@company.com",
        RepositoryUrl = "https://github.com/organization/very-large-repo-with-many-dependencies-and-modules",
        BuildUrl = "https://jenkins.company.com/job/very-large-project/99999/",
        DurationSeconds = 1800,
        Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord, NotificationChannel.Telegram },
        Metadata = new Dictionary<string, object>
        {
            { "environment", "production" },
            { "region", "us-east-1" },
            { "deployedBy", "ci-cd-pipeline" },
            { "affectedServices", new[] { "service-a", "service-b", "service-c" } },
            { "rollbackPlan", "automatic" },
            { "canaryPercentage", 10 },
            { "previousVersion", "2.9.5" },
            { "newVersion", "3.0.0" }
        }
    };

    var config = new ChannelConfiguration
    {
        ChannelType = NotificationChannel.Slack,
        WebhookUrl = "https://webhook.site/test"
    };

    var payloadBuilder = new PayloadBuilder(NullLogger<PayloadBuilder>.Instance);
    _ = payloadBuilder.BuildPayload(notification, config);
}

/// <summary>
/// Benchmark JSON serialization performance
/// </summary>
[Benchmark]
public void SerializeWebhookPayload()
{
    var payload = new WebhookPayload
    {
        EventType = "deployment",
        EventId = Guid.NewGuid().ToString(),
        Timestamp = DateTime.UtcNow,
        Data = new WebhookData
        {
            ProjectName = "SerializationTest",
            Version = "1.0.0",
            Status = "success",
            Environment = "production",
            Branch = "main",
            Commit = "abc123def456",
            Author = "test@example.com",
            Repository = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/build/1",
            DurationSeconds = 120,
            Metadata = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 },
                { "key3", true }
            }
        }
    };

    _ = payload.ToJson();
}

/// <summary>
/// Benchmark notification validation performance
/// </summary>
[Benchmark]
public void ValidateNotification()
{
    var notification = new DeploymentNotification
    {
        ProjectName = "ValidationTest",
        Version = "1.0.0",
        Status = BuildStatus.DeploymentSuccess,
        Message = "Test",
        TargetEnvironment = Environment.Development,
        BranchName = "main",
        CommitHash = "abc123",
        CommitAuthor = "test@example.com",
        RepositoryUrl = "https://github.com/test/repo",
        BuildUrl = "https://ci.example.com/build/1",
        DurationSeconds = 60,
        Channels = new List<NotificationChannel> { NotificationChannel.Slack }
    };

    var validator = new ValidationService();
    _ = validator.ValidateNotification(notification);
}

/// <summary>
/// Benchmark creating notification with minimal data
/// </summary>
[Benchmark]
public async Task CreateMinimalNotification()
{
    await _notificationService.CreateNotificationAsync(new DeploymentNotification
    {
        ProjectName = "Minimal",
        Version = "1.0",
        Status = BuildStatus.DeploymentSuccess,
        BranchName = "main",
        Channels = new List<NotificationChannel> { NotificationChannel.Slack }
    });
}
}
