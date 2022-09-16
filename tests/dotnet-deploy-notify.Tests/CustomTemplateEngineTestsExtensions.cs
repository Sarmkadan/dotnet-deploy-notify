#nullable enable

using DotNetDeployNotify.Core;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.Tests;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods for <see cref="CustomTemplateEngineTests"/> that provide additional test utilities
/// for working with CustomTemplate-related types in test scenarios.
/// </summary>
public static class CustomTemplateEngineTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="CustomTemplate"/> instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="name">The template name.</param>
    /// <param name="content">The template content.</param>
    /// <exception cref="ArgumentNullException">Thrown when tests or content is null.</exception>
    /// <exception cref="ArgumentException">Thrown when name is null or whitespace.</exception>
    /// <returns>A new CustomTemplate instance.</returns>
    public static CustomTemplate CreateTemplate(this CustomTemplateEngineTests tests, string name, string content)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(content);

        return new CustomTemplate { Name = name, Content = content };
    }

    /// <summary>
    /// Creates a default <see cref="DeploymentNotification"/> for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when tests is null.</exception>
    /// <returns>A new DeploymentNotification instance.</returns>
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
