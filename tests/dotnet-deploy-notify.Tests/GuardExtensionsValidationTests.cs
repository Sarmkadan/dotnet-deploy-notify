#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the GuardExtensionsValidation class.
/// </summary>
public class GuardExtensionsValidationTests
{
    [Fact]
    public void ValidateObject_And_ValidateNotNull_ReturnsProblemsWhenNull()
    {
        object? obj = null;
        
        GuardExtensionsValidation.ValidateObject(obj, "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateNotNull(obj, "param").Should().NotBeEmpty();
        
        object? validObj = new object();
        GuardExtensionsValidation.ValidateObject(validObj, "param").Should().BeEmpty();
        GuardExtensionsValidation.ValidateNotNull(validObj, "param").Should().BeEmpty();
    }

    [Fact]
    public void ValidateString_ReturnsProblemsForNullOrWhiteSpace()
    {
        GuardExtensionsValidation.ValidateString(null, "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateString("", "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateString("   ", "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateString("valid", "param").Should().BeEmpty();
    }

    [Fact]
    public void ValidateCollection_ReturnsProblemsForNullOrEmpty()
    {
        IEnumerable<int>? nullCol = null;
        GuardExtensionsValidation.ValidateCollection(nullCol, "param").Should().NotBeEmpty();
        
        var emptyCol = Enumerable.Empty<int>();
        GuardExtensionsValidation.ValidateCollection(emptyCol, "param").Should().NotBeEmpty();
        
        var validCol = new List<int> { 1 };
        GuardExtensionsValidation.ValidateCollection(validCol, "param").Should().BeEmpty();
    }

    [Fact]
    public void ValidateCondition_ReturnsProblemsWhenFalse()
    {
        GuardExtensionsValidation.ValidateCondition(false, "param", "error").Should().Contain("error");
        GuardExtensionsValidation.ValidateCondition(true, "param", "error").Should().BeEmpty();
    }

    [Fact]
    public void ValidateMinimum_And_ValidateRange_ValidatesCorrectly()
    {
        GuardExtensionsValidation.ValidateMinimum(5, 10, "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateMinimum(15, 10, "param").Should().BeEmpty();
        
        GuardExtensionsValidation.ValidateRange(5, 10, 20, "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateRange(15, 10, 20, "param").Should().BeEmpty();
        GuardExtensionsValidation.ValidateRange(25, 10, 20, "param").Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateMaxLength_ReturnsProblemsWhenTooLong()
    {
        GuardExtensionsValidation.ValidateMaxLength("longer", 3, "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateMaxLength("short", 10, "param").Should().BeEmpty();
    }

    [Fact]
    public void ValidateUrl_ValidatesCorrectly()
    {
        GuardExtensionsValidation.ValidateUrl("not-a-url", "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateUrl("ftp://invalid.com", "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidateUrl("https://valid.com", "param").Should().BeEmpty();
        GuardExtensionsValidation.ValidateUrl("http://valid.com", "param").Should().BeEmpty();
    }

    [Fact]
    public void ValidatePattern_ValidatesCorrectly()
    {
        GuardExtensionsValidation.ValidatePattern("abc", @"^\d+$", "param").Should().NotBeEmpty();
        GuardExtensionsValidation.ValidatePattern("123", @"^\d+$", "param").Should().BeEmpty();
    }
}
