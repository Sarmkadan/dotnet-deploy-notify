#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Extension methods for <see cref="NotificationBuilder"/> to provide common notification patterns
/// </summary>
public static class NotificationBuilderExtensions
{
    /// <summary>
    /// Adds common metadata for deployment tracking including deployer information
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="deployer">Name of the person who triggered the deployment</param>
    /// <param name="deploymentId">Unique deployment identifier</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithDeploymentMetadata(
        this NotificationBuilder builder,
        string deployer,
        string deploymentId)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(deployer);
        ArgumentException.ThrowIfNullOrEmpty(deploymentId);

        return builder
            .WithMetadata("deployer", deployer)
            .WithMetadata("deploymentId", deploymentId)
            .WithMetadata("timestamp", DateTime.UtcNow.ToString("o"));
    }

    /// <summary>
    /// Sets the notification priority based on the build status
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="status">The build/deployment status</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithPriorityForStatus(
        this NotificationBuilder builder,
        BuildStatus status)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return status switch
        {
            BuildStatus.Failed or BuildStatus.Cancelled or BuildStatus.DeploymentFailed => builder.CriticalPriority(),
            BuildStatus.SuccessWithWarnings or BuildStatus.DeploymentSuccess => builder.WithPriority(NotificationPriority.High),
            _ => builder.NormalPriority()
        };
    }

    /// <summary>
    /// Adds source control information including repository, branch, and commit details
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="repositoryUrl">URL to the source code repository</param>
    /// <param name="branchName">Name of the branch being deployed</param>
    /// <param name="commitHash">Git commit hash</param>
    /// <param name="commitMessage">Commit message summary</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or repositoryUrl is null</exception>
    public static NotificationBuilder WithSourceControl(
        this NotificationBuilder builder,
        string repositoryUrl,
        string branchName,
        string commitHash,
        string commitMessage)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(repositoryUrl);
        ArgumentException.ThrowIfNullOrEmpty(branchName);
        ArgumentException.ThrowIfNullOrEmpty(commitHash);
        ArgumentException.ThrowIfNullOrEmpty(commitMessage);

        return builder
            .WithRepository(repositoryUrl)
            .WithBranch(branchName, commitHash)
            .WithMetadata("commitMessage", commitMessage);
    }

    /// <summary>
    /// Adds timing information for build/deployment duration
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="startTime">When the build/deployment started</param>
    /// <param name="endTime">When the build/deployment completed</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithTiming(
        this NotificationBuilder builder,
        DateTime startTime,
        DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var duration = (int)(endTime - startTime).TotalSeconds;
        return builder.WithDuration(duration);
    }

    /// <summary>
    /// Adds build URL and sets appropriate message based on status
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="buildUrl">URL to the build job</param>
    /// <param name="status">Current build/deployment status</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithBuildReference(
        this NotificationBuilder builder,
        string buildUrl,
        BuildStatus status)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(buildUrl);

        return builder
            .WithBuildUrl(buildUrl)
            .WithMessage(GetStatusMessage(status));
    }

    /// <summary>
    /// Adds multiple notification channels at once
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="channels">Collection of notification channels</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or channels is null</exception>
    public static NotificationBuilder WithChannels(
        this NotificationBuilder builder,
        params NotificationChannel[] channels)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(channels);

        return builder.WithChannels(channels.AsEnumerable());
    }

    /// <summary>
    /// Adds multiple notification channels at once from an enumerable
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="channels">Collection of notification channels</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or channels is null</exception>
    public static NotificationBuilder WithChannels(
        this NotificationBuilder builder,
        IEnumerable<NotificationChannel> channels)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(channels);

        return builder.WithChannels(channels.ToArray());
    }

    /// <summary>
    /// Adds common infrastructure metadata including server and service information
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="serverName">Name of the server/agent</param>
    /// <param name="serviceName">Name of the service being deployed</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithInfrastructureMetadata(
        this NotificationBuilder builder,
        string serverName,
        string serviceName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(serverName);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        return builder
            .WithMetadata("server", serverName)
            .WithMetadata("service", serviceName);
    }

    /// <summary>
    /// Adds test result metrics to the notification
    /// </summary>
    /// <param name="builder">The notification builder instance</param>
    /// <param name="totalTests">Total number of tests executed</param>
    /// <param name="passedTests">Number of tests that passed</param>
    /// <param name="failedTests">Number of tests that failed</param>
    /// <param name="skippedTests">Number of tests that were skipped</param>
    /// <returns>The notification builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static NotificationBuilder WithTestResults(
        this NotificationBuilder builder,
        int totalTests,
        int passedTests,
        int failedTests,
        int skippedTests = 0)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var testCoverage = totalTests > 0
            ? (double)passedTests / totalTests * 100
            : 0;

        return builder
            .WithMetadata("testTotal", totalTests)
            .WithMetadata("testPassed", passedTests)
            .WithMetadata("testFailed", failedTests)
            .WithMetadata("testSkipped", skippedTests)
            .WithMetadata("testCoverage", testCoverage.ToString("F2", CultureInfo.InvariantCulture) + "%");
    }

    private static string GetStatusMessage(BuildStatus status) => status switch
    {
        BuildStatus.Started => "Build started",
        BuildStatus.InProgress => "Build in progress",
        BuildStatus.Success => "✅ Build completed successfully",
        BuildStatus.SuccessWithWarnings => "⚠️ Build completed with warnings",
        BuildStatus.Failed => "❌ Build failed",
        BuildStatus.Cancelled => "🚫 Build cancelled",
        BuildStatus.Deploying => "🚀 Deployment in progress",
        BuildStatus.DeploymentSuccess => "✅ Deployment completed successfully",
        BuildStatus.DeploymentFailed => "💥 Deployment failed",
        _ => "Build completed"
    };
}