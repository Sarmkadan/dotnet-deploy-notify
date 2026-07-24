using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class TrafficSplitterValidationTests
{
    [Fact]
    public void Validate_ValidSplit_ReturnsEmptyList()
    {
        var split = new TrafficSplit { StablePercent = 50, CanaryPercent = 50 };
        var errors = TrafficSplitterValidation.Validate(split);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidSplit_ReturnsErrors()
    {
        var split = new TrafficSplit { StablePercent = 60, CanaryPercent = 60 }; // Sums to 120
        var errors = TrafficSplitterValidation.Validate(split);
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("sum to 100"));
    }

    [Fact]
    public void IsValid_ValidSplit_ReturnsTrue()
    {
        var split = new TrafficSplit { StablePercent = 100, CanaryPercent = 0 };
        TrafficSplitterValidation.IsValid(split).Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidSplit_ReturnsFalse()
    {
        var split = new TrafficSplit { StablePercent = -10, CanaryPercent = 110 };
        TrafficSplitterValidation.IsValid(split).Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidSplit_DoesNotThrow()
    {
        var split = new TrafficSplit { StablePercent = 80, CanaryPercent = 20 };
        Action act = () => TrafficSplitterValidation.EnsureValid(split);
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidSplit_ThrowsArgumentException()
    {
        var split = new TrafficSplit { StablePercent = 100, CanaryPercent = 100 };
        Action act = () => TrafficSplitterValidation.EnsureValid(split);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_NullSplit_ThrowsArgumentNullException()
    {
        TrafficSplit? split = null;
        Action act = () => TrafficSplitterValidation.Validate(split!);
        act.Should().Throw<ArgumentNullException>();
    }
}
