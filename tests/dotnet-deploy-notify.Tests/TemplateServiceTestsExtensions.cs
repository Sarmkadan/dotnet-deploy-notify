#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Globalization;

using Environment = DotNetDeployNotify.Core.Environment;

namespace DotNetDeployNotify.Tests;

public static class TemplateServiceTestsExtensions
{
    /// <summary>
    /// Creates a test deployment notification with default values for testing template rendering.
    /// </summary>
    /// <param name="projectName">The project name to use in the notification.</param>
    /// <param name="version">The version to use in the notification.</param>
    /// <param name="status">The build status to use in the notification.</param>
    /// <returns>A configured <see cref="DeploymentNotification"/> instance.</returns>
    public static DeploymentNotification CreateTestNotification(
        this TemplateServiceTests _,
        string projectName = "TestApp",
        string version = "1.0.0",
        BuildStatus status = BuildStatus.Success)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(version);

        return new DeploymentNotification
        {
            ProjectName = projectName,
            Version = version,
            BranchName = "main",
            Message = "Test deployment message",
            Status = status,
            TargetEnvironment = Environment.Development,
            CommitHash = "abc1234567890def",
            CommitAuthor = "Test Author",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/builds/123",
            DurationSeconds = 120,
            Priority = NotificationPriority.Normal,
            Channels = [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Asserts that a template renders correctly with the given notification.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="template">The template to render.</param>
    /// <param name="notification">The deployment notification.</param>
    /// <param name="expected">The expected rendered result.</param>
    public static void ShouldRenderTemplateCorrectly(
        this TemplateServiceTests templateServiceTests,
        string template,
        DeploymentNotification notification,
        string expected)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentException.ThrowIfNullOrEmpty(expected);

        var result = templateServiceTests.TemplateService().RenderTemplate(template, notification);
        result.Should().Be(expected);
    }

    /// <summary>
    /// Creates a deployment notification with null duration for testing N/A handling.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="projectName">The project name.</param>
    /// <returns>A notification with null duration.</returns>
    public static DeploymentNotification CreateNotificationWithNullDuration(
        this TemplateServiceTests templateServiceTests,
        string projectName = "TestApp")
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        return new DeploymentNotification
        {
            ProjectName = projectName,
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test message",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Development,
            CommitHash = "abc1234567890def",
            Channels = [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Creates a deployment notification with custom priority for testing priority variable.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="priority">The notification priority.</param>
    /// <returns>A notification with the specified priority.</returns>
    public static DeploymentNotification CreateNotificationWithPriority(
        this TemplateServiceTests templateServiceTests,
        NotificationPriority priority)
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test message",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Development,
            CommitHash = "abc1234567890def",
            Priority = priority,
            Channels = [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Creates a deployment notification with custom environment for testing environment variable.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="environment">The target environment.</param>
    /// <returns>A notification with the specified environment.</returns>
    public static DeploymentNotification CreateNotificationWithEnvironment(
        this TemplateServiceTests templateServiceTests,
        Environment environment)
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test message",
            Status = BuildStatus.Success,
            TargetEnvironment = environment,
            CommitHash = "abc1234567890def",
            Channels = [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Gets the template service instance from the test class.
    /// </summary>
    public static TemplateService TemplateService(this TemplateServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return test.GetFieldValue<TemplateService>("_templateService");
    }

    /// <summary>
    /// Gets the mock logger instance from the test class.
    /// </summary>
    public static ILogger<TemplateService> MockLogger(this TemplateServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return test.GetFieldValue<ILogger<TemplateService>>("_mockLogger");
    }

    /// <summary>
    /// Gets a field value from the test class using reflection.
    /// </summary>
    /// <typeparam name="T">The field type.</typeparam>
    /// <param name="fieldName">The field name.</param>
    /// <returns>The field value.</returns>
    private static T GetFieldValue<T>(this TemplateServiceTests test, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        var field = typeof(TemplateServiceTests).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field?.GetValue(test) is T value ? value : throw new InvalidOperationException(
            $"Field '{fieldName}' not found or has wrong type. Expected: {typeof(T).Name}");
    }

    /// <summary>
    /// Creates a deployment notification with custom message for testing message variable.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="message">The custom message.</param>
    /// <returns>A notification with the specified message.</returns>
    public static DeploymentNotification CreateNotificationWithMessage(
        this TemplateServiceTests templateServiceTests,
        string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = message,
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Development,
            CommitHash = "abc1234567890def",
            Channels = [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Creates a deployment notification with custom repository URL for testing repository URL variable.
    /// </summary>
    /// <param name="templateServiceTests">The test instance.</param>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <returns>A notification with the specified repository URL.</returns>
    public static DeploymentNotification CreateNotificationWithRepositoryUrl(
        this TemplateServiceTests templateServiceTests,
        string repositoryUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(repositoryUrl);

        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test message",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Development,
            RepositoryUrl = repositoryUrl,
            CommitHash = "abc1234567890def",
            Channels = [NotificationChannel.Slack]
        };
    }
}