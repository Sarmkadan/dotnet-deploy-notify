using System;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public sealed class CustomTemplateExtensionsTests
{
    private static CustomTemplate CreateSampleTemplate(
        string? id = "template-1",
        string? name = "SampleTemplate",
        string? category = "Email",
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        string? content = "Hello, this is a sample template content.")
    {
        return new CustomTemplate
        {
            Id = id ?? string.Empty,
            Name = name ?? string.Empty,
            Category = category ?? string.Empty,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            Content = content
        };
    }

    // ---------- GenerateSummary ----------

    [Fact]
    public void GenerateSummary_ReturnsExpectedString()
    {
        // Arrange
        var created = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var updated = new DateTime(2023, 2, 1, 12, 0, 0, DateTimeKind.Utc);
        var template = CreateSampleTemplate(
            id: "abc-123",
            name: "MyTemplate",
            category: "Notification",
            createdAt: created,
            updatedAt: updated,
            content: "content");

        var expected = $"Template 'MyTemplate' (ID: abc-123, Category: Notification) " +
                       $"Created: {created:O}, Last Updated: {updated:O}";

        // Act
        var result = template.GenerateSummary();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateSummary_NullTemplate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((CustomTemplate)null!).GenerateSummary());
    }

    [Fact]
    public void GenerateSummary_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var template = CreateSampleTemplate(name: string.Empty);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => template.GenerateSummary());
        Assert.Contains("template name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- IsOutdated ----------

    [Fact]
    public void IsOutdated_WhenUpdatedBeforeThreshold_ReturnsTrue()
    {
        // Arrange
        var updated = DateTime.UtcNow.AddDays(-10);
        var template = CreateSampleTemplate(updatedAt: updated);

        // Act
        var result = template.IsOutdated(maxAgeInDays: 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsOutdated_WhenUpdatedWithinThreshold_ReturnsFalse()
    {
        // Arrange
        var updated = DateTime.UtcNow.AddDays(-2);
        var template = CreateSampleTemplate(updatedAt: updated);

        // Act
        var result = template.IsOutdated(maxAgeInDays: 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsOutdated_NullTemplate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((CustomTemplate)null!).IsOutdated(1));
    }

    [Fact]
    public void IsOutdated_MaxAgeLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var template = CreateSampleTemplate();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => template.IsOutdated(0));
    }

    // ---------- GenerateContentPreview ----------

    [Fact]
    public void GenerateContentPreview_NullOrEmptyContent_ReturnsPlaceholder()
    {
        // Arrange
        var nullContentTemplate = CreateSampleTemplate(content: null);
        var emptyContentTemplate = CreateSampleTemplate(content: string.Empty);

        // Act
        var resultNull = nullContentTemplate.GenerateContentPreview();
        var resultEmpty = emptyContentTemplate.GenerateContentPreview();

        // Assert
        Assert.Equal("[Empty content]", resultNull);
        Assert.Equal("[Empty content]", resultEmpty);
    }

    [Fact]
    public void GenerateContentPreview_ContentLongerThanMaxLength_IsTruncatedWithEllipsis()
    {
        // Arrange
        var longContent = new string('a', 150);
        var template = CreateSampleTemplate(content: longContent);
        const int maxLength = 100;

        // Act
        var preview = template.GenerateContentPreview(maxLength);

        // Assert
        var expected = string.Concat(longContent.AsSpan(0, maxLength), "...");
        Assert.Equal(expected, preview);
        Assert.Equal(maxLength + 3, preview.Length);
    }

    [Fact]
    public void GenerateContentPreview_ContentShorterThanOrEqualToMaxLength_ReturnsFullContent()
    {
        // Arrange
        var shortContent = "Short content";
        var template = CreateSampleTemplate(content: shortContent);
        const int maxLength = 100;

        // Act
        var preview = template.GenerateContentPreview(maxLength);

        // Assert
        Assert.Equal(shortContent, preview);
    }

    [Fact]
    public void GenerateContentPreview_NullTemplate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((CustomTemplate)null!).GenerateContentPreview());
    }

    [Fact]
    public void GenerateContentPreview_MaxLengthLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var template = CreateSampleTemplate(content: "content");

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => template.GenerateContentPreview(0));
    }
}
