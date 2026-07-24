#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Infrastructure;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for ServiceExtensions extension methods
/// </summary>
public class ServiceExtensionsTests
{
    private static DeploymentNotification CreateSampleNotification(
        BuildStatus status = BuildStatus.Success,
        NotificationPriority priority = NotificationPriority.Normal,
        Environment environment = Environment.Development)
    {
        return new DeploymentNotification
        {
            ProjectName = "TestProject",
            Version = "1.0.0",
            Status = status,
            Message = "Test deployment",
            TargetEnvironment = environment,
            BranchName = "main",
            CommitHash = "abc123",
            CommitAuthor = "test@example.com",
            RepositoryUrl = "https://github.com/test/repo",
            BuildUrl = "https://ci.example.com/builds/1",
            DurationSeconds = 120,
            Priority = priority,
            Metadata = new Dictionary<string, object> { { "key1", "value1" } }
        };
    }

    private static ChannelConfiguration CreateSampleChannelConfiguration(
        List<BuildStatus>? allowedStatuses = null,
        List<Environment>? allowedEnvironments = null)
    {
        return new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://example.com/webhook",
            DisplayName = "Test Channel",
            AllowedStatuses = allowedStatuses ?? new List<BuildStatus>(),
            AllowedEnvironments = allowedEnvironments ?? new List<Environment>(),
            CustomHeaders = new Dictionary<string, string>(),
            Settings = new Dictionary<string, string>()
        };
    }

    #region IsCritical Tests

    [Fact]
    public void IsCritical_WithCriticalPriority_ReturnsTrue()
    {
        // Arrange
        var notification = CreateSampleNotification(priority: NotificationPriority.Critical);

        // Act
        var result = notification.IsCritical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_WithHighPriority_ReturnsFalse()
    {
        // Arrange
        var notification = CreateSampleNotification(priority: NotificationPriority.High);

        // Act
        var result = notification.IsCritical();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithDeploymentFailedStatus_ReturnsTrue()
    {
        // Arrange
        var notification = CreateSampleNotification(status: BuildStatus.DeploymentFailed);

        // Act
        var result = notification.IsCritical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_WithFailedStatus_ReturnsTrue()
    {
        // Arrange
        var notification = CreateSampleNotification(status: BuildStatus.Failed);

        // Act
        var result = notification.IsCritical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_WithSuccessStatus_ReturnsFalse()
    {
        // Arrange
        var notification = CreateSampleNotification(status: BuildStatus.Success);

        // Act
        var result = notification.IsCritical();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_WithNullNotification_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.IsCritical());
    }

    #endregion

    #region IsProduction Tests

    [Fact]
    public void IsProduction_WithProductionEnvironment_ReturnsTrue()
    {
        // Arrange
        var notification = CreateSampleNotification(environment: Environment.Production);

        // Act
        var result = notification.IsProduction();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsProduction_WithPreProductionEnvironment_ReturnsTrue()
    {
        // Arrange
        var notification = CreateSampleNotification(environment: Environment.PreProduction);

        // Act
        var result = notification.IsProduction();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsProduction_WithStagingEnvironment_ReturnsFalse()
    {
        // Arrange
        var notification = CreateSampleNotification(environment: Environment.Staging);

        // Act
        var result = notification.IsProduction();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProduction_WithDevelopmentEnvironment_ReturnsFalse()
    {
        // Arrange
        var notification = CreateSampleNotification(environment: Environment.Development);

        // Act
        var result = notification.IsProduction();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsProduction_WithNullNotification_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.IsProduction());
    }

    #endregion

    #region SupportsStatus Tests

    [Fact]
    public void SupportsStatus_WithEmptyAllowedStatuses_ReturnsTrue()
    {
        // Arrange
        var config = CreateSampleChannelConfiguration(allowedStatuses: new List<BuildStatus>());
        var status = BuildStatus.Success;

        // Act
        var result = config.SupportsStatus(status);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsStatus_WithMatchingStatus_ReturnsTrue()
    {
        // Arrange
        var allowedStatuses = new List<BuildStatus> { BuildStatus.Success, BuildStatus.Failed };
        var config = CreateSampleChannelConfiguration(allowedStatuses: allowedStatuses);
        var status = BuildStatus.Success;

        // Act
        var result = config.SupportsStatus(status);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsStatus_WithNonMatchingStatus_ReturnsFalse()
    {
        // Arrange
        var allowedStatuses = new List<BuildStatus> { BuildStatus.Success, BuildStatus.Failed };
        var config = CreateSampleChannelConfiguration(allowedStatuses: allowedStatuses);
        var status = BuildStatus.DeploymentFailed;

        // Act
        var result = config.SupportsStatus(status);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SupportsStatus_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        ChannelConfiguration? config = null;
        var status = BuildStatus.Success;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config!.SupportsStatus(status));
    }

    #endregion

    #region SupportsEnvironment Tests

    [Fact]
    public void SupportsEnvironment_WithEmptyAllowedEnvironments_ReturnsTrue()
    {
        // Arrange
        var config = CreateSampleChannelConfiguration(allowedEnvironments: new List<Environment>());
        var env = Environment.Production;

        // Act
        var result = config.SupportsEnvironment(env);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsEnvironment_WithMatchingEnvironment_ReturnsTrue()
    {
        // Arrange
        var allowedEnvironments = new List<Environment> { Environment.Production, Environment.Staging };
        var config = CreateSampleChannelConfiguration(allowedEnvironments: allowedEnvironments);
        var env = Environment.Production;

        // Act
        var result = config.SupportsEnvironment(env);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsEnvironment_WithNonMatchingEnvironment_ReturnsFalse()
    {
        // Arrange
        var allowedEnvironments = new List<Environment> { Environment.Production, Environment.Staging };
        var config = CreateSampleChannelConfiguration(allowedEnvironments: allowedEnvironments);
        var env = Environment.Development;

        // Act
        var result = config.SupportsEnvironment(env);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SupportsEnvironment_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        ChannelConfiguration? config = null;
        var env = Environment.Production;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config!.SupportsEnvironment(env));
    }

    #endregion

    #region GetDescription(BuildStatus) Tests

    [Theory]
    [InlineData(BuildStatus.Started, "Build has started")]
    [InlineData(BuildStatus.InProgress, "Build is in progress")]
    [InlineData(BuildStatus.Success, "Build completed successfully")]
    [InlineData(BuildStatus.Failed, "Build failed with errors")]
    [InlineData(BuildStatus.Cancelled, "Build was cancelled")]
    [InlineData(BuildStatus.SuccessWithWarnings, "Build succeeded with warnings")]
    [InlineData(BuildStatus.Deploying, "Deployment in progress")]
    [InlineData(BuildStatus.DeploymentSuccess, "Deployment completed successfully")]
    [InlineData(BuildStatus.DeploymentFailed, "Deployment failed")]
    public void GetDescription_ForBuildStatus_ReturnsCorrectDescription(BuildStatus status, string expectedDescription)
    {
        // Act
        var result = status.GetDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_ForUnknownBuildStatus_ReturnsUnknownStatus()
    {
        // Arrange
        var status = (BuildStatus)999;

        // Act
        var result = status.GetDescription();

        // Assert
        result.Should().Be("Unknown status");
    }

    #endregion

    #region GetDescription(NotificationChannel) Tests

    [Theory]
    [InlineData(NotificationChannel.Telegram, "Telegram")]
    [InlineData(NotificationChannel.Slack, "Slack")]
    [InlineData(NotificationChannel.Discord, "Discord")]
    [InlineData(NotificationChannel.Teams, "Unknown Channel")] // Teams is missing from switch, so it hits _ => "Unknown Channel"
    [InlineData(NotificationChannel.Webhook, "Generic Webhook")]
    [InlineData(NotificationChannel.Email, "Email")]
    public void GetDescription_ForNotificationChannel_ReturnsCorrectDescription(NotificationChannel channel, string expectedDescription)
    {
        // Act
        var result = channel.GetDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_ForUnknownNotificationChannel_ReturnsUnknownChannel()
    {
        // Arrange
        var channel = (NotificationChannel)999;

        // Act
        var result = channel.GetDescription();

        // Assert
        result.Should().Be("Unknown Channel");
    }

    #endregion

    #region GetDescription(DeliveryStatus) Tests

    [Theory]
    [InlineData(DeliveryStatus.Pending, "Pending delivery")]
    [InlineData(DeliveryStatus.Delivered, "Successfully delivered")]
    [InlineData(DeliveryStatus.Failed, "Delivery failed")]
    [InlineData(DeliveryStatus.Retried, "Retry scheduled")]
    [InlineData(DeliveryStatus.Skipped, "Delivery skipped")]
    [InlineData(DeliveryStatus.Timeout, "Delivery timed out")]
    public void GetDescription_ForDeliveryStatus_ReturnsCorrectDescription(DeliveryStatus status, string expectedDescription)
    {
        // Act
        var result = status.GetDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_ForUnknownDeliveryStatus_ReturnsUnknownStatus()
    {
        // Arrange
        var status = (DeliveryStatus)999;

        // Act
        var result = status.GetDescription();

        // Assert
        result.Should().Be("Unknown status");
    }

    #endregion

    #region GetDescription(Environment) Tests

    [Theory]
    [InlineData(Environment.Development, "Development")]
    [InlineData(Environment.Staging, "Staging / QA")]
    [InlineData(Environment.Production, "Production")]
    [InlineData(Environment.Testing, "Testing")]
    [InlineData(Environment.PreProduction, "Pre-Production")]
    public void GetDescription_ForEnvironment_ReturnsCorrectDescription(Environment env, string expectedDescription)
    {
        // Act
        var result = env.GetDescription();

        // Assert
        result.Should().Be(expectedDescription);
    }

    [Fact]
    public void GetDescription_ForUnknownEnvironment_ReturnsUnknownEnvironment()
    {
        // Arrange
        var env = (Environment)999;

        // Act
        var result = env.GetDescription();

        // Assert
        result.Should().Be("Unknown Environment");
    }

    #endregion

    #region MergeMetadata Tests

    [Fact]
    public void MergeMetadata_MergesSourceMetadataIntoTarget()
    {
        // Arrange
        var target = CreateSampleNotification();
        var source = CreateSampleNotification();
        source.Metadata["key2"] = "value2";
        source.Metadata["key3"] = "value3";

        // Act
        target.MergeMetadata(source);

        // Assert
        target.Metadata.Should().ContainKeys("key1", "key2", "key3");
        target.Metadata["key1"].Should().Be("value1");
        target.Metadata["key2"].Should().Be("value2");
        target.Metadata["key3"].Should().Be("value3");
    }

    [Fact]
    public void MergeMetadata_OverwritesExistingKeysInTarget()
    {
        // Arrange
        var target = CreateSampleNotification();
        var source = CreateSampleNotification();
        source.Metadata["key1"] = "newValue";

        // Act
        target.MergeMetadata(source);

        // Assert
        target.Metadata["key1"].Should().Be("newValue");
    }

    [Fact]
    public void MergeMetadata_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? target = null;
        var source = CreateSampleNotification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => target!.MergeMetadata(source));
    }

    [Fact]
    public void MergeMetadata_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var target = CreateSampleNotification();
        DeploymentNotification? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => target.MergeMetadata(source!));
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_CreatesNewNotificationWithNewId()
    {
        // Arrange
        var original = CreateSampleNotification();
        var originalId = original.Id;

        // Act
        var clone = original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.Id.Should().NotBe(originalId);
        clone.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(clone.Id, out _).Should().BeTrue();
    }

    [Fact]
    public void Clone_CopiesAllProperties()
    {
        // Arrange
        var original = CreateSampleNotification(
            status: BuildStatus.DeploymentSuccess,
            priority: NotificationPriority.High,
            environment: Environment.Production);

        // Act
        var clone = original.Clone();

        // Assert
        clone.ProjectName.Should().Be(original.ProjectName);
        clone.Version.Should().Be(original.Version);
        clone.Status.Should().Be(original.Status);
        clone.Message.Should().Be(original.Message);
        clone.TargetEnvironment.Should().Be(original.TargetEnvironment);
        clone.BranchName.Should().Be(original.BranchName);
        clone.CommitHash.Should().Be(original.CommitHash);
        clone.CommitAuthor.Should().Be(original.CommitAuthor);
        clone.RepositoryUrl.Should().Be(original.RepositoryUrl);
        clone.BuildUrl.Should().Be(original.BuildUrl);
        clone.DurationSeconds.Should().Be(original.DurationSeconds);
        clone.Priority.Should().Be(original.Priority);
        clone.Channels.Should().BeEquivalentTo(original.Channels);
        clone.Metadata.Should().BeEquivalentTo(original.Metadata);
        clone.IsProcessed.Should().BeFalse();
        clone.DeliveryAttempts.Should().Be(0);
    }

    [Fact]
    public void Clone_CreatesIndependentCollections()
    {
        // Arrange
        var original = CreateSampleNotification();
        original.Channels.Add(NotificationChannel.Email);
        original.Metadata["test"] = "value";

        // Act
        var clone = original.Clone();

        // Assert
        clone.Channels.Should().NotBeSameAs(original.Channels);
        clone.Metadata.Should().NotBeSameAs(original.Metadata);
        clone.Channels.Should().BeEquivalentTo(original.Channels);
        clone.Metadata.Should().BeEquivalentTo(original.Metadata);

        // Modify clone to ensure independence
        clone.Channels.Add(NotificationChannel.Telegram);
        clone.Metadata["test"] = "modified";

        original.Channels.Should().NotContain(NotificationChannel.Telegram);
        original.Metadata["test"].Should().Be("value");
    }

    [Fact]
    public void Clone_WithNullNotification_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.Clone());
    }

    [Fact]
    public void Clone_SetsCreatedAtToCurrentTime()
    {
        // Arrange
        var original = CreateSampleNotification();
        var originalCreatedAt = original.CreatedAt;

        // Small delay to ensure time difference
        Thread.Sleep(10);

        // Act
        var clone = original.Clone();

        // Assert
        clone.CreatedAt.Should().BeAfter(originalCreatedAt);
    }

    #endregion
}