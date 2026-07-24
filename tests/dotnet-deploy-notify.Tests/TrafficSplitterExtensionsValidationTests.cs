#nullable enable
using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Environment = DotNetDeployNotify.Core.Environment;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for <see cref="TrafficSplitterExtensionsValidation"/> validation methods.
/// </summary>
public class TrafficSplitterExtensionsValidationTests
{
    // Test ValidateCreateLinearCanaryDeployment
    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithValidParameters_ReturnsEmptyList()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0", 5);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithNullProjectName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            null!, "1.1.0", "1.0.0", 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("projectName");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithNullCanaryVersion_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "MyProject", null!, "1.0.0", 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("canaryVersion");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithNullStableVersion_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "MyProject", "1.1.0", null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("stableVersion");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithEmptyProjectName_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "", "1.1.0", "1.0.0", 5);

        // Assert
        result.Should().ContainSingle(error => error == "ProjectName cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithWhitespaceProjectName_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "   ", "1.1.0", "1.0.0", 5);

        // Assert
        result.Should().ContainSingle(error => error == "ProjectName cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithEmptyCanaryVersion_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "MyProject", "", "1.0.0", 5);

        // Assert
        result.Should().ContainSingle(error => error == "CanaryVersion cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateCreateLinearCanaryDeployment_WithStepCountLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateLinearCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0", 0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("stepCount");
    }

    // Test ValidateCreateExponentialCanaryDeployment
    [Fact]
    public void ValidateCreateExponentialCanaryDeployment_WithValidParameters_ReturnsEmptyList()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateExponentialCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCreateExponentialCanaryDeployment_WithNullProjectName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateExponentialCanaryDeployment(
            null!, "1.1.0", "1.0.0");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("projectName");
    }

    [Fact]
    public void ValidateCreateExponentialCanaryDeployment_WithEmptyProjectName_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateExponentialCanaryDeployment(
            "", "1.1.0", "1.0.0");

        // Assert
        result.Should().ContainSingle(error => error == "ProjectName cannot be null or whitespace.");
    }

    // Test ValidateShouldProceedToNextStepAsync
    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25.0),
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) },
                new() { StepNumber = 2, CanaryPercent = 50.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithNullDeployment_ThrowsArgumentNullException()
    {
        // Arrange
        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            null!, healthEvaluator);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("deployment");
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithNullHealthEvaluator_ThrowsArgumentNullException()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25.0),
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("healthEvaluator");
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithEmptyProjectName_ReturnsError()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25.0),
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().ContainSingle(error => error == "Deployment.ProjectName cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithEmptyRolloutPlan_ReturnsError()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25.0),
            RolloutPlan = new List<CanaryRolloutStep>(),
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().ContainSingle(error => error == "Deployment.RolloutPlan must contain at least one step.");
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithInvalidStablePercent_ReturnsError()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = new TrafficSplit { StablePercent = 150.0, CanaryPercent = 0.0 }, // Invalid: > 100
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().ContainSingle(error => error.Contains("Deployment.CurrentSplit.StablePercent must be between 0 and 100"));
    }

    [Fact]
    public void ValidateShouldProceedToNextStepAsync_WithPercentagesNotSummingTo100_ReturnsError()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = new TrafficSplit { StablePercent = 30.0, CanaryPercent = 60.0 }, // Sum = 90
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().ContainSingle(error => error.Contains("Deployment.CurrentSplit percentages must sum to 100"));
    }

    // Test ValidateGetCanaryPercentageNormalized
    [Fact]
    public void ValidateGetCanaryPercentageNormalized_WithValidSplit_ReturnsEmptyList()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateGetCanaryPercentageNormalized(
            TrafficSplit.FromCanaryPercent(25.0));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGetCanaryPercentageNormalized_WithNullSplit_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateGetCanaryPercentageNormalized(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("split");
    }

    [Fact]
    public void ValidateGetCanaryPercentageNormalized_WithNegativeCanaryPercent_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateGetCanaryPercentageNormalized(
            new TrafficSplit { CanaryPercent = -5.0, StablePercent = 105.0 });

        // Assert
        result.Should().ContainSingle(error => error.Contains("Split.CanaryPercent must be between 0 and 100"));
    }

    [Fact]
    public void ValidateGetCanaryPercentageNormalized_WithPercentagesNotSummingTo100_ReturnsError()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateGetCanaryPercentageNormalized(
            new TrafficSplit { CanaryPercent = 30.0, StablePercent = 60.0 }); // Sum = 90

        // Assert
        result.Should().ContainSingle(error => error.Contains("Split percentages must sum to 100"));
    }

    // Test ValidateCreateBlueGreenCanaryDeployment
    [Fact]
    public void ValidateCreateBlueGreenCanaryDeployment_WithValidParameters_ReturnsEmptyList()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.ValidateCreateBlueGreenCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCreateBlueGreenCanaryDeployment_WithNullProjectName_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.ValidateCreateBlueGreenCanaryDeployment(
            null!, "1.1.0", "1.0.0");

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("projectName");
    }

    // Test IsValid methods
    [Fact]
    public void IsValidCreateLinearCanaryDeployment_WithValidParameters_ReturnsTrue()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.IsValidCreateLinearCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0", 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCreateLinearCanaryDeployment_WithInvalidParameters_ReturnsFalse()
    {
        // Act
        var result = TrafficSplitterExtensionsValidation.IsValidCreateLinearCanaryDeployment(
            "", "1.1.0", "1.0.0", 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidShouldProceedToNextStepAsync_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "TestProject",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = Environment.Development,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25.0),
            RolloutPlan = new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 25.0, SoakDuration = TimeSpan.FromMinutes(5) }
            },
            CreatedAt = DateTime.UtcNow
        };

        var healthEvaluator = new CanaryHealthEvaluator(
            Options.Create(new CanaryOptions()),
            Substitute.For<ILogger<CanaryHealthEvaluator>>()
        );

        // Act
        var result = TrafficSplitterExtensionsValidation.IsValidShouldProceedToNextStepAsync(
            deployment, healthEvaluator);

        // Assert
        result.Should().BeTrue();
    }

    // Test EnsureValid methods (they throw exceptions when validation fails)
    [Fact]
    public void EnsureValidCreateLinearCanaryDeployment_WithValidParameters_DoesNotThrow()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.EnsureValidCreateLinearCanaryDeployment(
            "MyProject", "1.1.0", "1.0.0", 5);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValidCreateLinearCanaryDeployment_WithInvalidProjectName_ThrowsArgumentException()
    {
        // Act
        Action act = () => TrafficSplitterExtensionsValidation.EnsureValidCreateLinearCanaryDeployment(
            "", "1.1.0", "1.0.0", 5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.Message.Contains("ProjectName cannot be null or whitespace"));
    }
}