#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DeploymentHistoryEntryJsonExtensionsTests
{
    private static DeploymentHistoryEntry CreateValidEntry() => new DeploymentHistoryEntry
    {
        Id = "test-id",
        ProjectName = "test-project",
        Version = "1.0.0",
        Tags = new Dictionary<string, string> { { "key", "value" } }
    };

    [Fact]
    public void ToJson_ValidEntry_ReturnsJsonString()
    {
        // Arrange
        var entry = CreateValidEntry();

        // Act
        var json = entry.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("test-id");
        json.Should().Contain("test-project");
    }

    [Fact]
    public void ToJson_Indented_ReturnsIndentedJson()
    {
        // Arrange
        var entry = CreateValidEntry();

        // Act
        var compact = entry.ToJson(indented: false);
        var indented = entry.ToJson(indented: true);

        // Assert
        compact.Should().NotContain("\n");
        indented.Should().Contain("\n");
    }

    [Fact]
    public void ToJson_NullEntry_ThrowsArgumentNullException()
    {
        // Arrange
        DeploymentHistoryEntry? entry = null;

        // Act
        Action act = () => entry!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsDeploymentHistoryEntry()
    {
        // Arrange
        var entry = CreateValidEntry();
        var json = entry.ToJson();

        // Act
        var result = DeploymentHistoryEntryJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entry.Id);
        result.ProjectName.Should().Be(entry.ProjectName);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Act
        Action act = () => DeploymentHistoryEntryJsonExtensions.FromJson("{ invalid json ");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => DeploymentHistoryEntryJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndEntry()
    {
        // Arrange
        var entry = CreateValidEntry();
        var json = entry.ToJson();

        // Act
        var result = DeploymentHistoryEntryJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value!.Id.Should().Be(entry.Id);
    }

    [Fact]
    public void TryFromJson_MalformedJson_ReturnsFalse()
    {
        // Act
        var result = DeploymentHistoryEntryJsonExtensions.TryFromJson("not json", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }
}
