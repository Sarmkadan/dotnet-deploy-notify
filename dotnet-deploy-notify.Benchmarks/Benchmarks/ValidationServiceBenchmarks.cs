using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ValidationServiceBenchmarks
{
    private ValidationService _validationService;
    private DeploymentNotification _notification;

    [GlobalSetup]
    public void Setup()
    {
        _validationService = new ValidationService();
        _notification = new DeploymentNotification
        {
            ProjectName = "TestProject",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Deployment successful",
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Slack
            }
        };
    }

    [Benchmark]
    public void ValidateNotification()
    {
        _validationService.ValidateNotification(_notification);
    }
}
