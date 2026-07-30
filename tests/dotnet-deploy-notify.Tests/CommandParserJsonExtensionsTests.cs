#nullable enable
using DotNetDeployNotify.CLI;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CommandParserJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var command = new ParsedCommand();

        // Act
        var json = command.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => ((ParsedCommand?)null).ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsParsedCommand()
    {
        // Arrange
        var command = new ParsedCommand();
        var json = command.ToJson();

        // Act
        var parsedCommand = CommandParserJsonExtensions.FromJson(json);

        // Assert
        parsedCommand.Should().NotBeNull();
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentException()
    {
        // Act and Assert
        Assert.Throws<ArgumentException>(() => CommandParserJsonExtensions.FromJson(null));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var parsedCommand = CommandParserJsonExtensions.FromJson("Invalid Json");

        // Assert
        parsedCommand.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndParsedCommand()
    {
        // Arrange
        var command = new ParsedCommand();
        var json = command.ToJson();

        // Act
        var success = CommandParserJsonExtensions.TryFromJson(json, out var parsedCommand);

        // Assert
        success.Should().BeTrue();
        parsedCommand.Should().NotBeNull();
    }

    [Fact]
    public void TryFromJson_NullInput_ReturnsFalse()
    {
        // Act
        var success = CommandParserJsonExtensions.TryFromJson(null, out _);

        // Assert
        success.Should().BeFalse();
    }
}
