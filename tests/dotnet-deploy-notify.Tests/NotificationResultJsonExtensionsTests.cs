using System;
using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class NotificationResultJsonExtensionsTests
{
    private static NotificationResult CreateSampleResult()
    {
        // Most NotificationResult implementations have a parameter‑less constructor.
        // The object is left with its default state – this is sufficient for
        // exercising the JSON (de)serialization logic.
        return new NotificationResult();
    }

    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        var result = CreateSampleResult();

        var json = result.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        // The JSON should contain the type name (or at least a opening brace)
        Assert.Contains("{", json);
    }

    [Fact]
    public void ToJson_Indented_FormatsJson()
    {
        var result = CreateSampleResult();

        var json = result.ToJson(indented: true);

        // Indented JSON contains line‑breaks and indentation spaces
        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        NotificationResult? result = null;
        Assert.Throws<ArgumentNullException>(() => result!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_Deserializes()
    {
        var original = CreateSampleResult();
        var json = original.ToJson();

        var deserialized = NotificationResultJsonExtensions.FromJson(json);

        Assert.NotNull(deserialized);
        // The deserialized instance should be of the same type
        Assert.IsType<NotificationResult>(deserialized);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => NotificationResultJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => NotificationResultJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        const string badJson = "{ not a valid json }";
        Assert.Throws<JsonException>(() => NotificationResultJsonExtensions.FromJson(badJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        var original = CreateSampleResult();
        var json = original.ToJson();

        var success = NotificationResultJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.IsType<NotificationResult>(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var success = NotificationResultJsonExtensions.TryFromJson("{bad json}", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => NotificationResultJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => NotificationResultJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
