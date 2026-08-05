#nullable enable
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class BatchNotificationJsonExtensionsTests
{
    private static DeploymentNotification CreateValidNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "SampleProject",
            Version = "1.0.0",
            BranchName = "main",
            Channels = new List<NotificationChannel> { NotificationChannel.Slack },
        };
    }

    private static BatchNotification CreateValidBatch()
    {
        return new BatchNotification
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Nightly batch",
            Notifications = new List<DeploymentNotification> { CreateValidNotification() },
            Channels = new List<NotificationChannel> { NotificationChannel.Slack },
            CreatedAt = DateTime.UtcNow,
        };
    }

    [Fact]
    public void ToJson_ValidBatch_ReturnsJsonString()
    {
        // Arrange
        var batch = CreateValidBatch();

        // Act
        var json = BatchNotificationJsonExtensions.ToJson(batch);

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToJson_NullBatch_ThrowsArgumentNullException()
    {
        // Arrange
        BatchNotification batch = null!;

        // Act
        Action act = () => BatchNotificationJsonExtensions.ToJson(batch);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_IndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var batch = CreateValidBatch();

        // Act
        var json = BatchNotificationJsonExtensions.ToJson(batch, indented: true);

        // Assert
        json.Should().Contain("\n  ", "indented JSON should contain newlines and spaces");
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsBatchNotification()
    {
        // Arrange
        var json = "{\"Id\":\"123e4567-e89b-12d3-a456-426655440000\",\"Name\":\"Nightly batch\",\"Notifications\":[{\"ProjectName\":\"SampleProject\",\"Version\":\"1.0.0\",\"BranchName\":\"main\",\"Channels\":[1]}],\"Channels\":[1],\"CreatedAt\":\"2022-01-01T12:00:00Z\"}";

        // Act
        var batch = BatchNotificationJsonExtensions.FromJson(json);

        // Assert
        batch.Should().NotBeNull();
        batch.Id.Should().Be("123e4567-e89b-12d3-a456-426655440000");
        batch.Name.Should().Be("Nightly batch");
        batch.Notifications.Should().HaveCount.Should().HaveCount(1);
        batch.Notifications.First().Channels.Should().Contain(NotificationChannel.Slack);
        batch.Channels.Should().HaveCount(1);
        batch.Channels.Should().Contain(NotificationChannel.Slack);
        batch.CreatedAt.Should().Be(DateTime.Parse("2022-01-01T12:00:00Z"));
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string[] testCases = { null, "", "   " };

        // Act & Assert
        foreach (var json in testCases)
        {
            Action act = () => BatchNotificationJsonExtensions.FromJson(json!);
            act.Should().Throw<ArgumentException>();
        }
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndBatch()
    {
        // Arrange
        var json = "{\"Id\":\"123e4567-e89b-12d3-a456-426655440000\",\"Name\":\"Nightly batch\",\"Notifications\":[{\"ProjectName\":\"SampleProject\",\"Version\":\"1.0.0\",\"BranchName\":\"main\",\"Channels\":[1]}],\"Channels\":[1],\"CreatedAt\":\"2022-01-01T12:00:00Z\"}";

        // Act
        var success = BatchNotificationJsonExtensions.TryFromJson(json, out var batch);

        // Assert
        success.Should().BeTrue();
        batch.Should().NotBeNull();
        batch.Id.Should().Be("123e4567-e89b-12d3-a456-426655440000");
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var success = BatchNotificationJsonExtensions.TryFromJson(json, out var batch);

        // Assert
        success.Should().BeFalse();
        batch.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullOrEmptyJson_ReturnsFalseAndNull()
    {
        // Arrange
        string[] testCases = { null, "", "   " };

        // Act & Assert
        foreach (var json in testCases)
        {
            var success = BatchNotificationJsonExtensions.TryFromJson(json!, out var batch);
            success.Should().BeFalse();
            batch.Should().BeNull();
        }
    }
}