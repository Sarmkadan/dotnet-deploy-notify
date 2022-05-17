#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.BackgroundWorkers;

/// <summary>
/// Background worker that periodically processes pending notifications
/// </summary>
public class NotificationProcessingWorker : BackgroundWorker
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationProcessingWorker> _logger;
    private readonly TimeSpan _interval;

    public NotificationProcessingWorker(
        INotificationService notificationService,
        ILogger<NotificationProcessingWorker> logger,
        TimeSpan? interval = null) : base(logger)
    {
        _notificationService = notificationService;
        _logger = logger;
        _interval = interval ?? TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification processing worker started (interval: {Interval}s)",
            _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var results = await _notificationService.SendPendingNotificationsAsync().ConfigureAwait(false);

                if (results.Any())
                {
                    var duration = DateTime.UtcNow - startTime;
                    var successCount = results.Count(r => r.IsSuccessful);

                    _logger.LogInformation(
                        "Processed {Count} notifications: {Success} succeeded, {Failed} failed ({Duration}ms)",
                        results.Count, successCount, results.Count - successCount, duration.TotalMilliseconds);
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications in background worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("Notification processing worker stopped");
    }
}

/// <summary>
/// Base class for background workers with lifecycle management
/// </summary>
public abstract class BackgroundWorker : BackgroundService
{
    protected readonly ILogger Logger;
    protected DateTime StartTime { get; set; }
    protected int ExecutionCount { get; set; }

    protected BackgroundWorker(ILogger logger)
    {
        Logger = logger;
        StartTime = DateTime.UtcNow;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{WorkerName} starting...", GetType().Name);
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var uptime = DateTime.UtcNow - StartTime;
        Logger.LogInformation("{WorkerName} stopping after {Uptime} seconds ({ExecutionCount} executions)",
            GetType().Name, uptime.TotalSeconds, ExecutionCount);

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Health check worker that periodically validates configuration
/// </summary>
public class HealthCheckWorker : BackgroundWorker
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthCheckWorker> _logger;
    private readonly TimeSpan _interval;

    public HealthCheckWorker(
        IHealthCheckService healthCheckService,
        ILogger<HealthCheckWorker> logger,
        TimeSpan? interval = null) : base(logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
        _interval = interval ?? TimeSpan.FromMinutes(5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health check worker started (interval: {Interval}m)", _interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ExecutionCount++;
                var health = await _healthCheckService.CheckSystemHealthAsync().ConfigureAwait(false);

                if (!health.IsOperational)
                {
                    _logger.LogWarning("Health check failed: {Issues}",
                        string.Join(", ", health.Errors));
                }

                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running health check in background worker");
            }
        }

        _logger.LogInformation("Health check worker stopped");
    }
}

/// <summary>
/// Scheduler for executing tasks at specific intervals
/// </summary>
public class ScheduledTaskWorker : BackgroundWorker
{
    private readonly List<ScheduledTask> _tasks = new();
    private readonly ILogger<ScheduledTaskWorker> _logger;

    public ScheduledTaskWorker(ILogger<ScheduledTaskWorker> logger) : base(logger)
    {
        _logger = logger;
    }

    public void RegisterTask(ScheduledTask task)
    {
        _tasks.Add(task);
        _logger.LogInformation("Registered scheduled task: {TaskName} (interval: {Interval}s)",
            task.Name, task.Interval.TotalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled task worker started with {Count} tasks", _tasks.Count);

        // Initialize last run times
        foreach (var task in _tasks)
            task.LastRun = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var task in _tasks.Where(t => t.ShouldRun()))
                {
                    try
                    {
                        _logger.LogDebug("Executing scheduled task: {TaskName}", task.Name);
                        await task.ExecuteAsync().ConfigureAwait(false);
                        task.LastRun = DateTime.UtcNow;
                        ExecutionCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Scheduled task failed: {TaskName}", task.Name);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled task worker");
            }
        }

        _logger.LogInformation("Scheduled task worker stopped");
    }
}

/// <summary>
/// Represents a task that runs on a schedule
/// </summary>
public abstract class ScheduledTask
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
    public DateTime LastRun { get; set; }

    public bool ShouldRun()
    {
        return DateTime.UtcNow - LastRun >= Interval;
    }

    public abstract Task ExecuteAsync();
}

/// <summary>
/// Example scheduled task
/// </summary>
public class CleanupExpiredNotificationsTask : ScheduledTask
{
    private readonly ILogger<CleanupExpiredNotificationsTask> _logger;

    public CleanupExpiredNotificationsTask(ILogger<CleanupExpiredNotificationsTask> logger)
    {
        _logger = logger;
        Name = "Cleanup Expired Notifications";
        Interval = TimeSpan.FromHours(1);
    }

    public override Task ExecuteAsync()
    {
        _logger.LogDebug("Cleaning up expired notifications");
        // Implementation would go here
        return Task.CompletedTask;
    }
}
