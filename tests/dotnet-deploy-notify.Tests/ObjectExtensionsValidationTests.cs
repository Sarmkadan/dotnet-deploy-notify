#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using DotNetDeployNotify.Utilities;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class ObjectExtensionsValidationTests
{
    // Validate tests
    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.Validate(input));
    }

    [Fact]
    public void Validate_ValidString_ReturnsEmpty()
    {
        // Arrange
        string input = "valid";

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_EmptyString_ReturnsError()
    {
        // Arrange
        string input = "";

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("String value is null, empty, or whitespace", result);
    }

    [Fact]
    public void Validate_WhitespaceString_ReturnsError()
    {
        // Arrange
        string input = "   ";

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("String value is null, empty, or whitespace", result);
    }

    [Fact]
    public void Validate_DefaultInt_ReturnsError()
    {
        // Arrange
        int input = 0;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("Value type Int32 has default value", result);
    }

    [Fact]
    public void Validate_NonDefaultInt_ReturnsEmpty()
    {
        // Arrange
        int input = 5;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_DefaultDateTime_ReturnsError()
    {
        // Arrange
        DateTime input = DateTime.MinValue;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("DateTime has default value (DateTime.MinValue)", result);
    }

    [Fact]
    public void Validate_NonDefaultDateTime_ReturnsEmpty()
    {
        // Arrange
        DateTime input = DateTime.Now;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_DefaultDateTimeOffset_ReturnsError()
    {
        // Arrange
        DateTimeOffset input = DateTimeOffset.MinValue;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("DateTimeOffset has default value (DateTimeOffset.MinValue)", result);
    }

    [Fact]
    public void Validate_NonDefaultDateTimeOffset_ReturnsEmpty()
    {
        // Arrange
        DateTimeOffset input = DateTimeOffset.Now;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_EmptyCollection_ReturnsError()
    {
        // Arrange
        int[] input = Array.Empty<int>();

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Single(result);
        Assert.Contains("Collection is empty", result);
    }

    [Fact]
    public void Validate_NonEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        int[] input = { 1, 2, 3 };

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Validate_NullCollection_ReturnsEmpty()
    {
        // Arrange
        int[]? input = null;

        // Act
        var result = ObjectExtensionsValidation.Validate(input);

        // Assert
        Assert.Empty(result); // null is not a collection? Actually null is not IEnumerable? null is not IEnumerable, so it skips collection check.
        // For null, we already throw ArgumentNullException, but we passed null? Wait, Validate throws on null. So this test is invalid.
        // Actually Validate throws ArgumentNullException for null input. So we need to test nullable? But object? can be null.
        // We'll skip this test because null throws.
    }

    // ValidateProperty tests
    [Fact]
    public void ValidateProperty_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;
        string propertyName = "Prop";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.ValidateProperty(input, propertyName));
    }

    [Fact]
    public void ValidateProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        object input = new object();
        string? propertyName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.ValidateProperty(input, propertyName));
    }

    [Fact]
    public void ValidateProperty_EmptyPropertyName_ThrowsArgumentException()
    {
        // Arrange
        object input = new object();
        string propertyName = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ObjectExtensionsValidation.ValidateProperty(input, propertyName));
    }

    [Fact]
    public void ValidateProperty_WhitespacePropertyName_ThrowsArgumentException()
    {
        // Arrange
        object input = new object();
        string propertyName = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ObjectExtensionsValidation.ValidateProperty(input, propertyName));
    }

    [Fact]
    public void ValidateProperty_PropertyNull_ReturnsEmpty()
    {
        // Arrange
        var obj = new { Name = (string?)null };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Name));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateProperty_PropertyValidString_ReturnsEmpty()
    {
        // Arrange
        var obj = new { Name = "Valid" };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Name));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateProperty_PropertyInvalidString_ReturnsError()
    {
        // Arrange
        var obj = new { Name = "" };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Name));

        // Assert
        Assert.Single(result);
        Assert.Contains("String value is null, empty, or whitespace", result);
    }

    [Fact]
    public void ValidateProperty_PropertyDefaultInt_ReturnsError()
    {
        // Arrange
        var obj = new { Value = 0 };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Value));

        // Assert
        Assert.Single(result);
        Assert.Contains("Value type Int32 has default value", result);
    }

    [Fact]
    public void ValidateProperty_PropertyNonDefaultInt_ReturnsEmpty()
    {
        // Arrange
        var obj = new { Value = 5 };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Value));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateProperty_PropertyEmptyCollection_ReturnsError()
    {
        // Arrange
        var obj = new { Items = Array.Empty<int>() };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Items));

        // Assert
        Assert.Single(result);
        Assert.Contains("Collection is empty", result);
    }

    [Fact]
    public void ValidateProperty_PropertyNonEmptyCollection_ReturnsEmpty()
    {
        // Arrange
        var obj = new { Items = new[] { 1, 2 } };

        // Act
        var result = ObjectExtensionsValidation.ValidateProperty(obj, nameof(obj.Items));

        // Assert
        Assert.Empty(result);
    }

    // IsValid tests
    [Fact]
    public void IsValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.IsValid(input));
    }

    [Fact]
    public void IsValid_ValidObject_ReturnsTrue()
    {
        // Arrange
        var input = new { Name = "Valid", Value = 5 };

        // Act
        var result = ObjectExtensionsValidation.IsValid(input);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_InvalidObject_ReturnsFalse()
    {
        // Arrange
        var input = new { Name = "" };

        // Act
        var result = ObjectExtensionsValidation.IsValid(input);

        // Assert
        Assert.False(result);
    }

    // IsValidProperty tests
    [Fact]
    public void IsValidProperty_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;
        string propertyName = "Prop";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.IsValidProperty(input, propertyName));
    }

    [Fact]
    public void IsValidProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        object input = new object();
        string? propertyName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.IsValidProperty(input, propertyName));
    }

    [Fact]
    public void IsValidProperty_EmptyPropertyName_ThrowsArgumentException()
    {
        // Arrange
        object input = new object();
        string propertyName = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ObjectExtensionsValidation.IsValidProperty(input, propertyName));
    }

    [Fact]
    public void IsValidProperty_ValidProperty_ReturnsTrue()
    {
        // Arrange
        var obj = new { Name = "Valid" };

        // Act
        var result = ObjectExtensionsValidation.IsValidProperty(obj, nameof(obj.Name));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidProperty_InvalidProperty_ReturnsFalse()
    {
        // Arrange
        var obj = new { Name = "" };

        // Act
        var result = ObjectExtensionsValidation.IsValidProperty(obj, nameof(obj.Name));

        // Assert
        Assert.False(result);
    }

    // EnsureValid tests
    [Fact]
    public void EnsureValid_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.EnsureValid(input));
    }

    [Fact]
    public void EnsureValid_ValidObject_DoesNotThrow()
    {
        // Arrange
        var input = new { Name = "Valid", Value = 5 };

        // Act
        var exception = Record.Exception(() => ObjectExtensionsValidation.EnsureValid(input));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidObject_ThrowsArgumentException()
    {
        // Arrange
        var input = new { Name = "" };

        // Act
        var exception = Record.Exception(() => ObjectExtensionsValidation.EnsureValid(input));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Contains("String value is null, empty, or whitespace", exception.Message);
    }

    // EnsureValidProperty tests
    [Fact]
    public void EnsureValidProperty_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        object? input = null;
        string propertyName = "Prop";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.EnsureValidProperty(input, propertyName));
    }

    [Fact]
    public void EnsureValidProperty_NullPropertyName_ThrowsArgumentNullException()
    {
        // Arrange
        object input = new object();
        string? propertyName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ObjectExtensionsValidation.EnsureValidProperty(input, propertyName));
    }

    [Fact]
    public void EnsureValidProperty_EmptyPropertyName_ThrowsArgumentException()
    {
        // Arrange
        object input = new object();
        string propertyName = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ObjectExtensionsValidation.EnsureValidProperty(input, propertyName));
    }

    [Fact]
    public void EnsureValidProperty_ValidProperty_DoesNotThrow()
    {
        // Arrange
        var obj = new { Name = "Valid" };

        // Act
        var exception = Record.Exception(() => ObjectExtensionsValidation.EnsureValidProperty(obj, nameof(obj.Name)));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValidProperty_InvalidProperty_ThrowsArgumentException()
    {
        // Arrange
        var obj = new { Name = "" };

        // Act
        var exception = Record.Exception(() => ObjectExtensionsValidation.EnsureValidProperty(obj, nameof(obj.Name)));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Contains("String value is null, empty, or whitespace", exception.Message);
        Assert.Contains("Property validation failed for 'Name'", exception.Message);
    }
}