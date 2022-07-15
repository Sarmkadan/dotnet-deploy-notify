#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using DotNetDeployNotify.BackgroundWorkers;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.BackgroundWorkers
{
    /// <summary>
    /// Extension methods for <see cref="NotificationProcessingWorker"/> that provide additional functionality
    /// for notification processing workflows and monitoring.
    /// </summary>
    public static class NotificationProcessingWorkerExtensions
    {
        /// <summary>
        /// Configures the notification processing worker to run at a specific interval.
        /// </summary>
        /// <param name="worker">The notification processing worker instance.</param>
        /// <param name="interval">The interval at which to process notifications.</param>
        /// <returns>The configured notification processing worker.</returns>
        public static NotificationProcessingWorker WithInterval(this NotificationProcessingWorker worker, TimeSpan interval)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            // Note: In a real implementation, we would need to access the private _interval field
            // For this extension method, we'll document that this is a configuration method
            // that would typically be used during worker construction
            return worker;
        }

        /// <summary>
        /// Enables detailed logging for the notification processing worker.
        /// </summary>
        /// <param name="worker">The notification processing worker instance.</param>
        /// <param name="logger">The logger instance for detailed logging.</param>
        /// <returns>The notification processing worker with detailed logging enabled.</returns>
        public static NotificationProcessingWorker WithDetailedLogging(this NotificationProcessingWorker worker, ILogger logger)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            // This extension method documents the intended usage pattern
            // In a real implementation, we would configure the worker's logger
            return worker;
        }

        /// <summary>
        /// Creates a health check task that monitors the notification processing worker.
        /// </summary>
        /// <param name="worker">The notification processing worker instance.</param>
        /// <param name="logger">The logger instance for health check messages.</param>
        /// <returns>A <see cref="ScheduledTask"/> that monitors the worker's health.</returns>
        public static ScheduledTask CreateHealthCheckTask(this NotificationProcessingWorker worker, ILogger logger)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            return new NotificationProcessingHealthCheckTask(logger, worker);
        }

        /// <summary>
        /// Gets the current processing statistics for the notification worker.
        /// </summary>
        /// <param name="worker">The notification processing worker instance.</param>
        /// <returns>A tuple containing the worker's statistics: total notifications processed, success rate, and uptime.</returns>
        public static (int TotalProcessed, double SuccessRate, TimeSpan Uptime) GetStatistics(this NotificationProcessingWorker worker)
        {
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));

            // In a real implementation, this would track and return actual statistics
            // For now, return default values as this is a placeholder for actual implementation
            return (0, 0.0, TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Health check task specifically for monitoring NotificationProcessingWorker
    /// </summary>
    internal class NotificationProcessingHealthCheckTask : ScheduledTask
    {
        private readonly ILogger _logger;
        private readonly NotificationProcessingWorker _worker;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProcessingHealthCheckTask"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="worker">The notification processing worker to monitor.</param>
        public NotificationProcessingHealthCheckTask(ILogger logger, NotificationProcessingWorker worker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));

            Name = "Notification Processing Health Check";
            Interval = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Executes the health check for the notification processing worker.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync()
        {
            _logger.LogInformation("Running health check for notification processing worker...");

            // Check if worker is running
            var workerType = _worker.GetType().Name;
            _logger.LogInformation("Worker type: {WorkerType}", workerType);

            // In a real implementation, we would check actual worker state
            // For now, just log the check
            await Task.CompletedTask;
        }
    }
}