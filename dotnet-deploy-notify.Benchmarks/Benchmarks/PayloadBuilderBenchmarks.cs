using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Environment = DotNetDeployNotify.Core.Environment;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Benchmark class for PayloadBuilder performance.
/// </summary>
[MemoryDiagnoser]
public class PayloadBuilderBenchmarks
{
    private PayloadBuilder _builder;
    private DeploymentNotification _notification;
    private ChannelConfiguration _config;

    /// <summary>
    /// Initializes the benchmark setup.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _builder = new PayloadBuilder(NullLogger<PayloadBuilder>.Instance);
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
    /// Builds a WebhookPayload instance using the PayloadBuilder.
    /// </summary>
    /// <returns>A WebhookPayload instance.</returns>
    [Benchmark]
    public WebhookPayload BuildPayload()
    {
        return _builder.BuildPayload(_notification, _config);
    }
}
