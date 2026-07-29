using System;
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for <see cref="CustomTemplateValidation"/>.
/// </summary>
public sealed class CustomTemplateValidationTests
{
    private static CustomTemplate CreateValidTemplate()
    {
        return new CustomTemplate
        {
            Id = "valid-id",
            Name = "Valid Name",
            Description = new string('d', 2000),
            Content = new string('c', 100000),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Category = new string('a', 200)
        };
    }

    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        var template = CreateValidTemplate();

        var errors = template.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_BoundaryValues_ReturnsEmptyList()
    {
        var template = new CustomTemplate
        {
            Id = new string('i', 100),
            Name = new string('n', 200),
            Description = new string('d', 2000),
            Content = new string('c', 100000),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Category = new string('a', 200)
        };

        var errors = template.Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_InvalidValues_ReturnsExpectedErrors()
    {
        var template = new CustomTemplate
        {
            Id = "   ",
            Name = "",
            Description = null,
            Content = "   ",
            CreatedAt = default,
            UpdatedAt = DateTime.UtcNow.AddHours(1), // future
            Category = new string('c', 201)
        };

        var errors = template.Validate();

        Assert.Equal(7, errors.Count);
        Assert.Contains("Id cannot be null or whitespace.", errors);
        Assert.Contains("Name cannot be null or whitespace.", errors);
        Assert.Contains("Description cannot be null or empty.", errors);
        Assert.Contains("Content cannot be null or whitespace.", errors);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", errors);
        Assert.Contains("UpdatedAt cannot be in the future.", errors);
        Assert.Contains("Category cannot exceed 200 characters.", errors);
    }

    [Fact]
    public void Validate_NullTemplate_ThrowsArgumentNullException()
    {
        CustomTemplate? template = null;

        Assert.Throws<ArgumentNullException>(() => template.Validate());
    }

    [Fact]
    public void IsValid_HappyPath_ReturnsTrue()
    {
        var template = CreateValidTemplate();

        Assert.True(template.IsValid());
    }

    [Fact]
    public void IsValid_InvalidTemplate_ReturnsFalse()
    {
        var template = new CustomTemplate
        {
            Id = "",
            Name = "n",
            Description = "",
            Content = "",
            CreatedAt = default,
            UpdatedAt = default
        };

        Assert.False(template.IsValid());
    }

    [Fact]
    public void IsValid_NullTemplate_ThrowsArgumentNullException()
    {
        CustomTemplate? template = null;

        Assert.Throws<ArgumentNullException>(() => template.IsValid());
    }

    [Fact]
    public void EnsureValid_HappyPath_DoesNotThrow()
    {
        var template = CreateValidTemplate();

        var exception = Record.Exception(() => template.EnsureValid());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_InvalidTemplate_ThrowsArgumentException()
    {
        var template = new CustomTemplate
        {
            Id = "",
            Name = "",
            Description = "",
            Content = "",
            CreatedAt = default,
            UpdatedAt = default
        };

        var ex = Assert.Throws<ArgumentException>(() => template.EnsureValid());

        Assert.Contains("The CustomTemplate is invalid.", ex.Message);
        Assert.Contains("Id cannot be null or whitespace.", ex.Message);
        Assert.Contains("Name cannot be null or whitespace.", ex.Message);
        Assert.Contains("Description cannot be null or empty.", ex.Message);
        Assert.Contains("Content cannot be null or whitespace.", ex.Message);
        Assert.Contains("CreatedAt cannot be the default DateTime value.", ex.Message);
        Assert.Contains("UpdatedAt cannot be the default DateTime value.", ex.Message);
    }

    [Fact]
    public void EnsureValid_NullTemplate_ThrowsArgumentNullException()
    {
        CustomTemplate? template = null;

        Assert.Throws<ArgumentNullException>(() => template.EnsureValid());
    }
}
