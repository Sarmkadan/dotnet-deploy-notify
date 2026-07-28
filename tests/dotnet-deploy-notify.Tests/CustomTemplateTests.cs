using Xunit;
using DotNetDeployNotify.Core.Models;
using System;

namespace DotNetDeployNotify.Tests;

public class CustomTemplateTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var template = new CustomTemplate();

        // Assert
        Assert.NotEmpty(template.Id);
        Assert.NotEqual(Guid.Empty.ToString(), template.Id);
        Assert.Empty(template.Name);
        Assert.Empty(template.Description);
        Assert.Empty(template.Content);
        Assert.Empty(template.Category);
        Assert.True(template.IsActive);
        Assert.True((DateTime.UtcNow - template.CreatedAt).TotalSeconds < 1);
        Assert.True((DateTime.UtcNow - template.UpdatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        // Act
        var template = new CustomTemplate
        {
            Id = id,
            Name = "Test Template",
            Description = "Test Description",
            Content = "Test Content",
            Category = "Test Category",
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        Assert.Equal(id, template.Id);
        Assert.Equal("Test Template", template.Name);
        Assert.Equal("Test Description", template.Description);
        Assert.Equal("Test Content", template.Content);
        Assert.Equal("Test Category", template.Category);
        Assert.False(template.IsActive);
        Assert.Equal(now, template.CreatedAt);
        Assert.Equal(now, template.UpdatedAt);
    }

    [Fact]
    public void Touch_UpdatesUpdatedAtProperty()
    {
        // Arrange
        var template = new CustomTemplate();
        var originalUpdatedAt = template.UpdatedAt;

        // Ensure time passes to verify the update
        System.Threading.Thread.Sleep(10);

        // Act
        template.Touch();

        // Assert
        Assert.True(template.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void Touch_DoesNotModifyCreatedAtProperty()
    {
        // Arrange
        var template = new CustomTemplate();
        var originalCreatedAt = template.CreatedAt;

        // Act
        template.Touch();

        // Assert
        Assert.Equal(originalCreatedAt, template.CreatedAt);
    }

    [Fact]
    public void Id_GeneratesUniqueValuesForNewInstances()
    {
        // Arrange & Act
        var template1 = new CustomTemplate();
        var template2 = new CustomTemplate();

        // Assert
        Assert.NotEqual(template1.Id, template2.Id);
    }
}
