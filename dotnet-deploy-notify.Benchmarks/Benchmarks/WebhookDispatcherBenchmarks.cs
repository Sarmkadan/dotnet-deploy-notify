using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Environment = DotNetDeployNotify.Core.Environment;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Benchmark class for WebhookDispatcher.
/// </summary>
[MemoryDiagnoser]
public class WebhookDispatcherBenchmarks
{
    private WebhookDispatcher _dispatcher;
    private DeploymentNotification _notification;
    private ChannelConfiguration _config;

    /// <summary>
    /// Sets up the benchmark by creating a new WebhookDispatcher instance and a DeploymentNotification instance.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var httpClient = new TestHttpClient();
        var payloadBuilder = new PayloadBuilder(NullLogger<PayloadBuilder>.Instance);
        _dispatcher = new WebhookDispatcher(httpClient, NullLogger<WebhookDispatcher>.Instance, payloadBuilder);
        
        _notification = new DeploymentNotification
        {
            ProjectName = "TestProject",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test message",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            CreatedAt = DateTime.UtcNow
        };
        _config = new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.slack.com/services/test"
        };
    }

    /// <summary>
    /// Sends a DeploymentNotification to a webhook using the WebhookDispatcher instance.
    /// </summary>
    /// <returns>A NotificationResult containing the result of the webhook send operation.</returns>
    [Benchmark]
    public async Task<NotificationResult> SendToWebhook()
    {
        return await _dispatcher.SendToWebhookAsync(_config, _notification);
    }
}
