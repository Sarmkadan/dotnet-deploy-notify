using System;
using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CanaryDeploymentJsonExtensionsTests
{
    private static CanaryDeployment CreateDeployment()
    {
        return new CanaryDeployment
        {
            ProjectName = "TestApp",
            StableVersion = "1.0.0",
            CanaryVersion = "2.0.0",
            TargetEnvironment = default
        };
    }

    [Fact]
    public void ToJson_SerializesSuccessfully()
    {
        var deployment = CreateDeployment();
        var json = deployment.ToJson();

        Assert.False(string.IsNullOrEmpty(json));
        Assert.Contains("testApp", json); // Verify camelCase naming policy
    }

    [Fact]
    public void ToJson_Indented_ReturnsFormattedJson()
    {
        var deployment = CreateDeployment();
        var json = deployment.ToJson(indented: true);

        Assert.Contains("\n", json);
        Assert.Contains("  ", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        CanaryDeployment? deployment = null;
        Assert.Throws<ArgumentNullException>(() => deployment!.ToJson());
    }

    [Fact]
    public void FromJson_DeserializesSuccessfully()
    {
        var deployment = CreateDeployment();
        var json = deployment.ToJson();

        var result = CanaryDeploymentJsonExtensions.FromJson(json);

        Assert.NotNull(result);
        Assert.Equal("TestApp", result.ProjectName);
        Assert.Equal("1.0.0", result.StableVersion);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CanaryDeploymentJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => CanaryDeploymentJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => CanaryDeploymentJsonExtensions.FromJson("{not valid json}"));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        var deployment = CreateDeployment();
        var json = deployment.ToJson();

        var success = CanaryDeploymentJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("TestApp", result.ProjectName);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var success = CanaryDeploymentJsonExtensions.TryFromJson("{bad}", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CanaryDeploymentJsonExtensions.TryFromJson(null!, out _));
        Assert.Throws<ArgumentException>(() => CanaryDeploymentJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
