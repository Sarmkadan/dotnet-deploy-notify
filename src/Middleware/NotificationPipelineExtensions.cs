#nullable enable

using System.Collections.Generic;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Middleware;

/// <summary>
/// Extension methods for <see cref="NotificationPipeline"/> that provide common pipeline operations
/// </summary>
public static class NotificationPipelineExtensions
{
    /// <summary>
    /// Executes the pipeline and returns the result with convenient access to processed data
    /// </summary>
    /// <param name="pipeline">The pipeline instance</param>
    /// <param name="notification">The notification to process</param>
    /// <returns>A result containing the processed notification and metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> or <paramref name="notification"/> is null</exception>
    public static async Task<PipelineResult> ExecuteWithMetadataAsync(this NotificationPipeline pipeline, DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(notification);

        return await pipeline.ExecuteAsync(notification).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the pipeline and returns only successful notifications, filtering out failures
    /// </summary>
    /// <param name="pipeline">The pipeline instance</param>
    /// <param name="notification">The notification to process</param>
    /// <returns>The processed notification if successful, otherwise null</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> or <paramref name="notification"/> is null</exception>
    public static async Task<DeploymentNotification?> ExecuteSuccessfullyAsync(this NotificationPipeline pipeline, DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(notification);

        var result = await pipeline.ExecuteAsync(notification).ConfigureAwait(false);
        return result.Success ? result.ProcessedNotification : null;
    }

    /// <summary>
    /// Gets all validation errors from the pipeline result in a formatted string
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>Formatted error messages or empty string if no errors</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null</exception>
    public static string GetValidationErrors(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Success
            ? string.Empty
            : string.Join("\n", result.Errors);
    }

    /// <summary>
    /// Checks if the pipeline execution was successful and has no errors
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>True if successful with no errors, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null</exception>
    public static bool IsSuccessful(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Success && !result.Errors.Any();
    }

    /// <summary>
    /// Gets the count of channels that the notification was processed for
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>Number of channels, or 0 if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null</exception>
    public static int GetChannelCount(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.ProcessedNotification?.Channels.Count ?? 0;
    }

    /// <summary>
    /// Gets the original notification from the pipeline result
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>The original notification, or null if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null</exception>
    public static DeploymentNotification? GetOriginalNotification(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Notification;
    }

    /// <summary>
    /// Gets the processed notification from the pipeline result
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>The processed notification, or null if not available</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null</exception>
    public static DeploymentNotification? GetProcessedNotification(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.ProcessedNotification;
    }

    /// <summary>
    /// Creates a new pipeline result with the given notification and success status
    /// </summary>
    /// <param name="pipeline">The pipeline instance</param>
    /// <param name="notification">The notification to process</param>
    /// <param name="success">Whether the processing was successful</param>
    /// <returns>A new pipeline result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> or <paramref name="notification"/> is null</exception>
    public static PipelineResult CreateResult(this NotificationPipeline pipeline, DeploymentNotification notification, bool success)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(notification);

        return new PipelineResult
        {
            Notification = notification,
            Success = success,
            Errors = []
        };
    }

    /// <summary>
    /// Creates a new pipeline result with the given notification and errors
    /// </summary>
    /// <param name="pipeline">The pipeline instance</param>
    /// <param name="notification">The notification to process</param>
    /// <param name="errors">List of error messages</param>
    /// <returns>A new pipeline result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/>, <paramref name="notification"/>, or <paramref name="errors"/> is null</exception>
    public static PipelineResult CreateResult(this NotificationPipeline pipeline, DeploymentNotification notification, IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(errors);

        return new PipelineResult
        {
            Notification = notification,
            Success = !errors.Any(),
            Errors = new List<string>(errors)
        };
    }
}