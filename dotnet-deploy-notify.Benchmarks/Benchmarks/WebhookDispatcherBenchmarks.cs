using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Environment = DotNetDeployNotify.Core.Environment;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class WebhookDispatcherBenchmarks
{
    private WebhookDispatcher _dispatcher;
    private DeploymentNotification _notification;
    private ChannelConfiguration _config;

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

    [Benchmark]
    public async Task<NotificationResult> SendToWebhook()
    {
        return await _dispatcher.SendToWebhookAsync(_config, _notification);
    }
}
