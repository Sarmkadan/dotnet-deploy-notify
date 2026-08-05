// Copyright (c) .NET Deploy Notify
// SPDX-License-Identifier: MIT

using System;
using System.Runtime.Serialization;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Unit tests for <see cref="BatchNotificationJsonExtensions"/>.
/// </summary>
public sealed class BatchNotificationJsonExtensionsTests
{
    /// <summary>
    /// Creates a <see cref="BatchNotification"/> instance without invoking any constructor.
    /// This works even if the type only has parameterised constructors.
    /// </summary>
    private static BatchNotification CreateEmptyBatchNotification()
    {
        // FormatterServices can create an uninitialized object even when no public
        // parameterless constructor exists. All properties will have their default values.
        return (BatchNotification)FormatterServices.GetUninitializedObject(typeof(BatchNotification));
    }

    [Fact]
    public void ToJson_Returns_ValidJson_WhenObjectIsNotNull()
    {
        // Arrange
        var notification = CreateEmptyBatchNotification();

        // Act
        var json = notification.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_Indents_WhenRequested()
    {
        // Arrange
        var notification = CreateEmptyBatchNotification();

        // Act
        var json = notification.ToJson(indented: true);

        // Assert
        // Indented JSON should contain at least one newline character.
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        BatchNotification? notification = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => notification!.ToJson());
    }

    [Fact]
    public void FromJson_ReturnsObject_ForValidJson()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = BatchNotificationJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_ReturnsNull_ForJsonLiteralNull()
    {
        // Arrange
        var json = "null";

        // Act
        var result = BatchNotificationJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_ForNullOrEmptyJson()
    {
        // Null string
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.FromJson(null!));

        // Empty string
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.FromJson(string.Empty));

        // Whitespace only
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndObject_ForValidJson()
    {
        // Arrange
        var json = "{}";

        // Act
        var success = BatchNotificationJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_ReturnsFalseAndNull_ForInvalidJson()
    {
        // Arrange
        var json = "{ this is not valid json }";

        // Act
        var success = BatchNotificationJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentException_ForNullOrWhitespace()
    {
        // Null
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.TryFromJson(null!, out _));

        // Empty
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.TryFromJson(string.Empty, out _));

        // Whitespace
        Assert.Throws<ArgumentException>(() => BatchNotificationJsonExtensions.TryFromJson("   ", out _));
    }
}
