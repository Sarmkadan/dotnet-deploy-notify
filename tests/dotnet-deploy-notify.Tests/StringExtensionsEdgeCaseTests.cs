#nullable enable
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public sealed class StringExtensionsEdgeCaseTests
{
    [Fact]
    public void Truncate_NullInput_ReturnsEmpty() =>
        ((string?)null).Truncate(10).Should().BeEmpty();

    [Fact]
    public void Truncate_EmptyInput_ReturnsEmpty() =>
        "".Truncate(10).Should().BeEmpty();

    [Fact]
    public void Truncate_WhitespaceInput_ReturnsEmpty() =>
        "   ".Truncate(10).Should().BeEmpty();

    [Fact]
    public void Truncate_ShortString_ReturnsUnchanged() =>
        "hello".Truncate(10).Should().Be("hello");

    [Fact]
    public void Truncate_LongString_TruncatesWithSuffix()
    {
        var result = "This is a deploy notification".Truncate(15);
        result.Should().EndWith("...");
        result.Length.Should().BeLessThanOrEqualTo(15);
    }

    [Fact]
    public void ToSlug_NullInput_ReturnsEmpty() =>
        ((string?)null).ToSlug().Should().BeEmpty();

    [Fact]
    public void ToSlug_EmptyInput_ReturnsEmpty() =>
        "".ToSlug().Should().BeEmpty();

    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Deploy v2.0", "deploy-v20")]
    [InlineData("UPPER CASE", "upper-case")]
    public void ToSlug_VariousInputs(string input, string expected) =>
        input.ToSlug().Should().Be(expected);

    [Fact]
    public void ToPascalCase_NullInput_ReturnsEmpty() =>
        ((string?)null).ToPascalCase().Should().BeEmpty();

    [Theory]
    [InlineData("hello world", "HelloWorld")]
    [InlineData("deploy-notify", "DeployNotify")]
    [InlineData("some_variable", "SomeVariable")]
    public void ToPascalCase_VariousInputs(string input, string expected) =>
        input.ToPascalCase().Should().Be(expected);

    [Fact]
    public void ToCamelCase_NullInput_ReturnsEmpty() =>
        ((string?)null).ToCamelCase().Should().BeEmpty();

    [Theory]
    [InlineData("hello world", "helloWorld")]
    [InlineData("Deploy Notify", "deployNotify")]
    public void ToCamelCase_VariousInputs(string input, string expected) =>
        input.ToCamelCase().Should().Be(expected);
}
