#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Performance benchmarks for payload building operations
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Benchmarks;

/// <summary>
/// Benchmarks for PayloadBuilder operations - measures serialization and formatting performance
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class PayloadBuilderBenchmarks
{
    private IPayloadBuilder _payloadBuilder = null!;
    private DeploymentNotification _smallNotification = null!;
    private DeploymentNotification _largeNotification = null!;
    private ChannelConfiguration _slackConfig = null!;
    private ChannelConfiguration _discordConfig = null!;
    private ChannelConfiguration _telegramConfig = null!;

    /// <summary>
    /// Setup test data before each benchmark
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var loggerFactory = new TestLoggerFactory();
        var logger = loggerFactory.CreateLogger<PayloadBuilder>();
        _payloadBuilder = new PayloadBuilder(logger);

        // Create small notification (minimal data)
        _smallNotification = new DeploymentNotification
        {
            ProjectName = "SmallApp",
            Version = "1.0.0",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Small message",
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "abc12345",
            CommitAuthor = "author@example.com",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/builds/12345",
            DurationSeconds = 120,
            Priority = NotificationPriority.Normal,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack }
        };

        // Create large notification (with lots of metadata)
        _largeNotification = new DeploymentNotification
        {
            ProjectName = "LargeEnterpriseApp",
            Version = "2.5.1",
            Status = BuildStatus.DeploymentSuccess,
            Message = "This is a very long deployment message with lots of details about the deployment process, including multiple lines of text that need to be formatted properly for different channels like Slack, Discord, and Telegram. The message contains various information about the build process, test results, and deployment verification steps.",
            TargetEnvironment = Environment.Production,
            BranchName = "feature/new-authentication-system-with-multi-factor-auth",
            CommitHash = "a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b",
            CommitAuthor = "john.doe@company.com",
            RepositoryUrl = "https://github.com/company/large-enterprise-app",
            BuildUrl = "https://ci.company.com/builds/987654321",
            DurationSeconds = 1845,
            Priority = NotificationPriority.Critical,
            Channels = new List<NotificationChannel> { NotificationChannel.Slack, NotificationChannel.Discord, NotificationChannel.Telegram }
        };

        // Create channel configurations
        _slackConfig = new ChannelConfiguration
        {
            DisplayName = "Slack - Production",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.slack.com/services/test",
            TargetId = "#deployments",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            MaxRetries = 3,
            TimeoutMs = 10000,
            UseSlackBlockKit = true,
            EnableEmojis = true,
            IsEnabled = true
        };

        _discordConfig = new ChannelConfiguration
        {
            DisplayName = "Discord - Production",
            ChannelType = NotificationChannel.Discord,
            WebhookUrl = "https://discord.com/api/webhooks/test",
            TargetId = "deployments",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            MaxRetries = 3,
            TimeoutMs = 10000,
            EnableEmojis = true,
            IsEnabled = true
        };

        _telegramConfig = new ChannelConfiguration
        {
            DisplayName = "Telegram - Production",
            ChannelType = NotificationChannel.Telegram,
            WebhookUrl = "https://api.telegram.org/bot12345:test/sendMessage",
            TargetId = "-1234567890",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            MaxRetries = 3,
            TimeoutMs = 10000,
            EnableEmojis = true,
            IsEnabled = true
        };
    }

    /// <summary>
    /// Benchmark: Build payload for small notification
    /// Measures the time to build a webhook payload with minimal data
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("PayloadBuilding")]
    public void BuildPayload_SmallNotification()
    {
        _payloadBuilder.BuildPayload(_smallNotification, _slackConfig);
    }

    /// <summary>
    /// Benchmark: Build payload for large notification
    /// Measures the time to build a webhook payload with extensive data
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("PayloadBuilding")]
    public void BuildPayload_LargeNotification()
    {
        _payloadBuilder.BuildPayload(_largeNotification, _discordConfig);
    }

    /// <summary>
    /// Benchmark: Build Slack Block Kit payload
    /// Measures the time to build a modern Slack Block Kit formatted payload
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ChannelFormatting")]
    public void BuildSlackBlockKitPayload()
    {
        _payloadBuilder.BuildSlackPayload(_largeNotification, _slackConfig);
    }

    /// <summary>
    /// Benchmark: Build Slack legacy payload
    /// Measures the time to build a legacy Slack attachment payload
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ChannelFormatting")]
    public void BuildSlackLegacyPayload()
    {
        _slackConfig.UseSlackBlockKit = false;
        _payloadBuilder.BuildSlackPayload(_largeNotification, _slackConfig);
    }

    /// <summary>
    /// Benchmark: Build Discord payload
    /// Measures the time to build a Discord embed payload
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ChannelFormatting")]
    public void BuildDiscordPayload()
    {
        _payloadBuilder.BuildDiscordPayload(_largeNotification, _discordConfig);
    }

    /// <summary>
    /// Benchmark: Build Telegram message
    /// Measures the time to build a Telegram formatted text message
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ChannelFormatting")]
    public void BuildTelegramMessage()
    {
        _payloadBuilder.BuildTelegramMessage(_largeNotification, _telegramConfig);
    }

    /// <summary>
    /// Benchmark: Serialize payload to JSON
    /// Measures the time to serialize a webhook payload to JSON string
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Serialization")]
    public void SerializePayloadToJson()
    {
        var payload = _payloadBuilder.BuildPayload(_largeNotification, _slackConfig);
        var json = payload.ToJson();
    }

    /// <summary>
    /// Benchmark: Build and serialize complete payload
    /// Measures the combined time to build and serialize a complete payload
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("EndToEnd")]
    public void BuildAndSerializeCompletePayload()
    {
        var payload = _payloadBuilder.BuildPayload(_largeNotification, _discordConfig);
        var json = payload.ToJson();
    }

    /// <summary>
    /// Benchmark: Build payloads for multiple channels
    /// Measures the time to build payloads for all supported channels
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ChannelFormatting")]
    public void BuildPayloadsForAllChannels()
    {
        _payloadBuilder.BuildPayload(_largeNotification, _slackConfig);
        _payloadBuilder.BuildPayload(_largeNotification, _discordConfig);
        _payloadBuilder.BuildPayload(_largeNotification, _telegramConfig);
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