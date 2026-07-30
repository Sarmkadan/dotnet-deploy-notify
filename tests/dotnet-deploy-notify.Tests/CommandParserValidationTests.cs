#nullable enable
using DotNetDeployNotify.CLI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CommandParserValidationTests
{
    private readonly ILogger<CommandParser> _logger;

    public CommandParserValidationTests()
    {
        _logger = Substitute.For<ILogger<CommandParser>>();
    }

    [Fact]
    public void Validate_WithValidParser_ReturnsEmptyList()
    {
        var parser = new CommandParser(_logger);
        // The default registered commands should be valid.
        
        var result = parser.Validate();
        
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidParser_ReturnsProblems()
    {
        var parser = new CommandParser(_logger);
        // Add an invalid command
        parser.RegisterCommand(new CommandDefinition
        {
            Name = "invalid-cmd",
            Description = "", // Invalid: empty description
            Parameters = new List<ParameterDefinition>
            {
                new() { Name = "", Description = "desc" } // Invalid: empty name
            }
        });
        
        var result = parser.Validate();
        
        result.Should().NotBeEmpty();
        result.Should().Contain(p => p.Contains("empty or whitespace Description"));
        result.Should().Contain(p => p.Contains("empty or whitespace Name"));
    }

    [Fact]
    public void IsValid_WithValidParser_ReturnsTrue()
    {
        var parser = new CommandParser(_logger);
        
        parser.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidParser_ReturnsFalse()
    {
        var parser = new CommandParser(_logger);
        parser.RegisterCommand(new CommandDefinition
        {
            Name = "cmd",
            Description = "desc",
            Options = new List<OptionDefinition>
            {
                new() { Name = "opt", ShortName = "too-long", Description = "desc" } // Invalid: short name length > 1
            }
        });
        
        parser.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithValidParser_DoesNotThrow()
    {
        var parser = new CommandParser(_logger);
        
        var act = () => parser.EnsureValid();
        
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithInvalidParser_ThrowsArgumentException()
    {
        var parser = new CommandParser(_logger);
        parser.RegisterCommand(new CommandDefinition
        {
            Name = "cmd",
            Description = "desc",
            Options = new List<OptionDefinition>
            {
                new() { Name = "opt", IsFlag = true, IsRequired = true } // Invalid: Flag cannot be required
            }
        });
        
        var act = () => parser.EnsureValid();
        
        act.Should().Throw<ArgumentException>();
    }
}
