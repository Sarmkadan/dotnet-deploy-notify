using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Xunit;
using FluentAssertions;
using System.Globalization;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides extension methods to assist with <see cref="DeploymentHistoryServiceTests"/>.
/// </summary>
public static class DeploymentHistoryServiceTestsExtensions
{
    /// <summary>
    /// Creates a standard deployment entry for test purposes.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <param name="version">The version string.</param>
    /// <param name="status">The build status.</param>
    /// <param name="deployedAt">The deployment time, defaults to UtcNow.</param>
    /// <returns>A populated <see cref="DeploymentHistoryEntry"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when projectName is null or empty.</exception>
    public static DeploymentHistoryEntry CreateTestEntry(
        this DeploymentHistoryServiceTests tests,
        string projectName,
        string version,
        BuildStatus status,
        DateTime? deployedAt = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        return new DeploymentHistoryEntry
        {
            ProjectName = projectName,
            Version = version,
            FinalStatus = status,
            TargetEnvironment = DotNetDeployNotify.Core.Environment.Production,
            BranchName = "main",
            CommitHash = "abc1234",
            CommitAuthor = "tester",
            DeployedAt = deployedAt ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Asserts that a collection of deployment entries are sorted by deployment time in descending order.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="entries">The collection of entries to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entries"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entries"/> is empty.</exception>
    public static void AssertSortedByDateDescending(
        this DeploymentHistoryServiceTests tests,
        IEnumerable<DeploymentHistoryEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var list = entries.ToList();
        list.Should().NotBeEmpty("the collection must contain at least one entry to verify sorting");

        for (int i = 0; i < list.Count - 1; i++)
        {
            list[i].DeployedAt.Should().BeOnOrAfter(list[i + 1].DeployedAt);
        }
    }
}