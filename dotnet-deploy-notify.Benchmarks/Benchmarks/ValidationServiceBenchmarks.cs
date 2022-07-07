using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Benchmark class for ValidationService.
/// </summary>
[MemoryDiagnoser]
public class ValidationServiceBenchmarks
{
    private ValidationService _validationService;
    private DeploymentNotification _notification;

    /// <summary>
    /// Initializes the benchmark by setting up the ValidationService and a DeploymentNotification instance.
    /// </summary>
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

    /// <summary>
    /// Validates a DeploymentNotification instance using the ValidationService.
    /// </summary>
    [Benchmark]
    public void ValidateNotification()
    {
        _validationService.ValidateNotification(_notification);
    }
}
