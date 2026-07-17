#nullable enable

using DotNetDeployNotify.Core;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Tests;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides extension methods for <see cref="CustomTemplateEngineTests"/> to simplify creation of test data
/// for <see cref="CustomTemplate"/> and <see cref="DeploymentNotification"/> instances in test scenarios.
/// </summary>
public static class CustomTemplateEngineTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="CustomTemplate"/> instance with the specified name and content.
    /// </summary>
    /// <param name="tests">The test instance. Must not be <see langword="null"/>.</param>
    /// <param name="name">The template name. Must not be <see langword="null"/> or whitespace.</param>
    /// <param name="content">The template content. Must not be <see langword="null"/>.</param>
    /// <returns>A new <see cref="CustomTemplate"/> instance with the specified properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="content"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static CustomTemplate CreateTemplate(this CustomTemplateEngineTests tests, string name, string content)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(content);

        return new CustomTemplate { Name = name, Content = content };
    }

    /// <summary>
    /// Creates a default <see cref="DeploymentNotification"/> for testing purposes with common test values.
    /// </summary>
    /// <param name="tests">The test instance. Must not be <see langword="null"/>.</param>
    /// <returns>A new <see cref="DeploymentNotification"/> instance populated with representative test data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static DeploymentNotification CreateDefaultNotification(this CustomTemplateEngineTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "2.0.0",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "abc1234def",
            CommitAuthor = "dev",
            Message = "Deployed OK",
            BuildUrl = "https://ci.example.com/1",
            RepositoryUrl = "https://github.com/org/repo",
            DurationSeconds = 120,
            Channels = [NotificationChannel.Slack]
        };
    }
}