#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for notification processing worker configurations
/// by exercising the NotificationProcessingWorkerExtensions methods.
/// </summary>
public static class NotificationProcessingWorkerExtensionsValidation
{
    /// <summary>
    /// Validates the notification processing worker configuration by exercising its extension methods.
    /// </summary>
    /// <param name="worker">The worker instance to validate.</param>
    /// <returns>A list of validation problems; empty if the configuration is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateWorkerExtensions(this NotificationProcessingWorker? worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var problems = new List<string>();

        // Validate WithInterval extension method
        try
        {
            var interval = TimeSpan.FromSeconds(30);
            var configuredWorker = worker.WithInterval(interval);
            if (configuredWorker is null)
            {
                problems.Add("WithInterval returned null.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"WithInterval threw an exception: {ex.Message}.");
        }

        // Validate WithDetailedLogging extension method
        try
        {
            var logger = new TestLogger();
            var configuredWorker = worker.WithDetailedLogging(logger);
            if (configuredWorker is null)
            {
                problems.Add("WithDetailedLogging returned null.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"WithDetailedLogging threw an exception: {ex.Message}.");
        }

        // Validate CreateHealthCheckTask extension method
        try
        {
            var logger = new TestLogger();
            var healthCheckTask = worker.CreateHealthCheckTask(logger);
            if (healthCheckTask is null)
            {
                problems.Add("CreateHealthCheckTask returned null.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CreateHealthCheckTask threw an exception: {ex.Message}.");
        }

        // Validate GetStatistics extension method
        try
        {
            var stats = worker.GetStatistics();
            if (stats.TotalProcessed < 0)
            {
                problems.Add($"GetStatistics.TotalProcessed must be non-negative, but was {stats.TotalProcessed}.");
            }

            if (stats.SuccessRate < 0.0 || stats.SuccessRate > 1.0)
            {
                problems.Add($"GetStatistics.SuccessRate must be between 0.0 and 1.0, but was {stats.SuccessRate}.");
            }

            if (stats.Uptime < TimeSpan.Zero)
            {
                problems.Add($"GetStatistics.Uptime must be non-negative, but was {stats.Uptime}.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetStatistics threw an exception: {ex.Message}.");
        }

        // Validate NotificationProcessingHealthCheckTask
        try
        {
            var logger = new TestLogger();
            var healthCheckTask = new NotificationProcessingHealthCheckTask(logger, worker);
            if (healthCheckTask is null)
            {
                problems.Add("NotificationProcessingHealthCheckTask constructor returned null.");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotificationProcessingHealthCheckTask validation threw an exception: {ex.Message}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the notification processing worker configuration is valid.
    /// </summary>
    /// <param name="worker">The worker instance to check.</param>
    /// <returns><see langword="true"/> if the configuration is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
    public static bool IsWorkerExtensionsValid(this NotificationProcessingWorker? worker)
    {
        ArgumentNullException.ThrowIfNull(worker);
        return worker.ValidateWorkerExtensions().Count == 0;
    }

    /// <summary>
    /// Ensures that the notification processing worker configuration is valid.
    /// </summary>
    /// <param name="worker">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The configuration is invalid, containing a list of problems.</exception>
    public static void EnsureWorkerExtensionsValid(this NotificationProcessingWorker? worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var problems = worker.ValidateWorkerExtensions();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"NotificationProcessingWorker configuration is invalid:{System.Environment.NewLine}- {
                string.Join($"{System.Environment.NewLine}- ", problems)
            }");
    }

    /// <summary>
    /// Simple test logger implementation for validation purposes.
    /// </summary>
    private sealed class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
