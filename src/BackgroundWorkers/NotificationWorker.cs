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
    private readonly object _statsLock = new();

    private TimeSpan _interval;
    private ILogger? _detailLogger;
    private int _totalProcessed;
    private int _totalSucceeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationProcessingWorker"/> class.
    /// </summary>
    /// <param name="notificationService">The notification service to process pending notifications.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="interval">The interval at which to process notifications. Defaults to 30 seconds.</param>
    public NotificationProcessingWorker(
        INotificationService notificationService,
        ILogger<NotificationProcessingWorker> logger,
        TimeSpan? interval = null) : base(logger)
    {
        _notificationService = notificationService;
        _logger = logger;
        _interval = interval ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Sets the processing interval. Takes effect on the next processing cycle.
    /// </summary>
    /// <param name="interval">The new interval; must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is zero or negative.</exception>
    internal void SetInterval(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);
        _interval = interval;
    }

    /// <summary>
    /// Attaches an additional logger that receives per-cycle diagnostic messages.
    /// </summary>
    /// <param name="logger">The logger to receive detailed output.</param>
    internal void SetDetailLogger(ILogger logger) => _detailLogger = logger;

    /// <summary>
    /// Returns the statistics accumulated since the worker was constructed.
    /// </summary>
    internal (int TotalProcessed, double SuccessRate, TimeSpan Uptime) GetStatisticsCore()
    {
        lock (_statsLock)
        {
            var successRate = _totalProcessed == 0 ? 0.0 : (double)_totalSucceeded / _totalProcessed;
            return (_totalProcessed, successRate, DateTime.UtcNow - StartTime);
        }
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
                var results = await _notificationService.SendPendingNotificationsAsync();

                ExecutionCount++;

                if (results.Any())
                {
                    var duration = DateTime.UtcNow - startTime;
                    var successCount = results.Count(r => r.IsSuccessful);

                    lock (_statsLock)
                    {
                        _totalProcessed += results.Count;
                        _totalSucceeded += successCount;
                    }

                    _logger.LogInformation(
                        "Processed {Count} notifications: {Success} succeeded, {Failed} failed ({Duration}ms)",
                        results.Count, successCount, results.Count - successCount, duration.TotalMilliseconds);

                    _detailLogger?.LogDebug(
                        "Cycle detail: {Count} processed in {Duration}ms, failures: {Failures}",
                        results.Count, duration.TotalMilliseconds,
                        string.Join(", ", results.Where(r => !r.IsSuccessful).Select(r => r.ErrorMessage ?? "unknown")));
                }
                else
                {
                    _detailLogger?.LogDebug("Cycle detail: no pending notifications");
                }

                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications in background worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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

    /// <summary>
    /// Starts the background worker.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("{WorkerName} starting...", GetType().Name);
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the background worker.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var uptime = DateTime.UtcNow - StartTime;
        Logger.LogInformation("{WorkerName} stopping after {Uptime} seconds ({ExecutionCount} executions)",
            GetType().Name, uptime.TotalSeconds, ExecutionCount);

        await base.StopAsync(cancellationToken);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckWorker"/> class.
    /// </summary>
    /// <param name="healthCheckService">The service used to check system health.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="interval">The interval at which to run health checks. Defaults to 5 minutes.</param>
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
                var health = await _healthCheckService.CheckSystemHealthAsync();

                if (!health.IsOperational)
                {
                    _logger.LogWarning("Health check failed: {Issues}",
                        string.Join(", ", health.Errors));
                }

                await Task.Delay(_interval, stoppingToken);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskWorker"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ScheduledTaskWorker(ILogger<ScheduledTaskWorker> logger) : base(logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a new scheduled task.
    /// </summary>
    /// <param name="task">The task to register.</param>
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
                        await task.ExecuteAsync();
                        task.LastRun = DateTime.UtcNow;
                        ExecutionCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Scheduled task failed: {TaskName}", task.Name);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
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
    /// <summary>
    /// Gets or sets the name of the task.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the interval at which the task should run.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
    /// <summary>
    /// Gets or sets the date and time when the task was last run.
    /// </summary>
    public DateTime LastRun { get; set; }

    /// <summary>
    /// Checks if the task should be run based on its last run time and interval.
    /// </summary>
    /// <returns>True if the task should run, otherwise false.</returns>
    public bool ShouldRun()
    {
        return DateTime.UtcNow - LastRun >= Interval;
    }

    /// <summary>
    /// Executes the task.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task ExecuteAsync();
}

/// <summary>
/// Example scheduled task
/// </summary>
public class CleanupExpiredNotificationsTask : ScheduledTask
{
    private readonly ILogger<CleanupExpiredNotificationsTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleanupExpiredNotificationsTask"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public CleanupExpiredNotificationsTask(ILogger<CleanupExpiredNotificationsTask> logger)
    {
        _logger = logger;
        Name = "Cleanup Expired Notifications";
        Interval = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Executes the task to clean up expired notifications.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override Task ExecuteAsync()
    {
        _logger.LogDebug("Cleaning up expired notifications");
        // Implementation would go here
        return Task.CompletedTask;
    }
}
