#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Search;
using Xunit;

public class SearchCriteriaJsonExtensionsTests
{
    [Fact]
    public void ToJson_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        SearchCriteria? criteria = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => criteria!.ToJson());
    }

    [Fact]
    public void ToJson_DefaultInstance_ReturnsEmptyJsonObject()
    {
        // Arrange
        var criteria = new SearchCriteria();

        // Act
        var json = criteria.ToJson();

        // Assert
        Assert.Equal("{}", json);
    }

    [Fact]
    public void ToJson_Indented_ReturnsPrettyPrintedJson()
    {
        // Arrange
        var criteria = new SearchCriteria();

        // Act
        var json = criteria.ToJson(indented: true);

        // Assert
        // Indented JSON for an empty object should be "{\n  \n}"
        // However JsonSerializer formats empty objects as "{\n  \n}" (newline + two spaces + newline)
        // We'll just verify that the string contains a newline and spaces.
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        // Null
        Assert.Throws<ArgumentException>(() => SearchCriteriaJsonExtensions.FromJson(null!));

        // Empty
        Assert.Throws<ArgumentException>(() => SearchCriteriaJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsDeserializedInstance()
    {
        // Arrange
        var original = new SearchCriteria();
        var json = original.ToJson();

        // Act
        var deserialized = SearchCriteriaJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        // Since SearchCriteria has no required properties, a simple reference equality check is enough.
        // The deserialized instance should be of the same type.
        Assert.IsType<SearchCriteria>(deserialized);
    }

    [Fact]
    public void TryFromJson_NullArgument_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SearchCriteriaJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndInstance()
    {
        // Arrange
        var criteria = new SearchCriteria();
        var json = criteria.ToJson();

        // Act
        var result = SearchCriteriaJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.IsType<SearchCriteria>(value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act
        var result = SearchCriteriaJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
}
