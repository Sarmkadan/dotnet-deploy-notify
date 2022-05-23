#nullable enable
using DotNetDeployNotify.Core;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class TemplateServiceTests
{
    private readonly TemplateService _templateService;
    private readonly ILogger<TemplateService> _mockLogger;

    public TemplateServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<TemplateService>>();
        _templateService = new TemplateService(_mockLogger);
    }

    #region RenderTemplate Tests

    [Fact]
    public void RenderTemplate_WithProjectNameVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Project: {{ProjectName}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Project: TestApp");
    }

    [Fact]
    public void RenderTemplate_WithVersionVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Version: {{Version}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Version: 2.1.0");
    }

    [Fact]
    public void RenderTemplate_WithStatusVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Status: {{Status}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Status: Success");
    }

    [Fact]
    public void RenderTemplate_WithMultipleVariables_ReplacesAllVariables()
    {
        // Arrange
        var template = "{{ProjectName}} v{{Version}} - {{Status}} on {{Branch}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("TestApp v2.1.0 - Success on main");
    }

    [Fact]
    public void RenderTemplate_WithUnknownVariable_LeavesVariableUnchanged()
    {
        // Arrange
        var template = "Deployment: {{UnknownVariable}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Deployment: {{UnknownVariable}}");
    }

    [Fact]
    public void RenderTemplate_WithEnvironmentVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Environment: {{Environment}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Environment: Production");
    }

    [Fact]
    public void RenderTemplate_WithCommitHashVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Commit: {{CommitHash}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Commit: abc1234567890def");
    }

    [Fact]
    public void RenderTemplate_WithCommitHashShortVariable_Returns7CharHash()
    {
        // Arrange
        var template = "Commit: {{CommitHashShort}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Commit: abc1234");
    }

    [Fact]
    public void RenderTemplate_WithCommitHashShortVariable_WithShortHash_ReturnsFullHash()
    {
        // Arrange
        var template = "Commit: {{CommitHashShort}}";
        var notification = new DeploymentNotification
        {
            ProjectName = "App",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            CommitHash = "abc12",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Commit: abc12");
    }

    [Fact]
    public void RenderTemplate_WithCommitAuthorVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Author: {{CommitAuthor}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Author: John Doe");
    }

    [Fact]
    public void RenderTemplate_WithRepositoryUrlVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Repo: {{RepositoryUrl}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Repo: https://github.com/org/repo");
    }

    [Fact]
    public void RenderTemplate_WithBuildUrlVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Build: {{BuildUrl}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Build: https://ci.example.com/builds/123");
    }

    [Fact]
    public void RenderTemplate_WithDurationVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Duration: {{Duration}} seconds";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Duration: 300 seconds");
    }

    [Fact]
    public void RenderTemplate_WithDurationVariable_AndNullDuration_ReturnsNA()
    {
        // Arrange
        var template = "Duration: {{Duration}}";
        var notification = new DeploymentNotification
        {
            ProjectName = "App",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            DurationSeconds = null,
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Duration: N/A");
    }

    [Fact]
    public void RenderTemplate_WithPriorityVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Priority: {{Priority}}";
        var notification = CreateTestNotification();
        notification.Priority = NotificationPriority.Critical;

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Priority: Critical");
    }

    [Fact]
    public void RenderTemplate_WithMessageVariable_ReplacesWithNotificationValue()
    {
        // Arrange
        var template = "Message: {{Message}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Contain("Deployed successfully");
    }

    [Fact]
    public void RenderTemplate_WithEmptyTemplate_ReturnsEmptyString()
    {
        // Arrange
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate("", notification);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderTemplate_WithNullTemplate_ReturnsEmptyString()
    {
        // Arrange
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(null!, notification);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void RenderTemplate_WithNoVariables_ReturnsTemplateUnchanged()
    {
        // Arrange
        var template = "This is a plain message with no variables";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public void RenderTemplate_IsCaseInsensitive_ForVariableNames()
    {
        // Arrange
        var template = "Project: {{projectname}}";
        var notification = CreateTestNotification();

        // Act
        var result = _templateService.RenderTemplate(template, notification);

        // Assert
        result.Should().Be("Project: TestApp");
    }

    #endregion

    #region GetAvailableVariables Tests

    [Fact]
    public void GetAvailableVariables_ReturnsAllSupportedVariables()
    {
        // Act
        var variables = _templateService.GetAvailableVariables();

        // Assert
        variables.Should().Contain("ProjectName");
        variables.Should().Contain("Version");
        variables.Should().Contain("Status");
        variables.Should().Contain("Message");
        variables.Should().Contain("Environment");
        variables.Should().Contain("Branch");
        variables.Should().Contain("CommitHash");
        variables.Should().Contain("CommitAuthor");
        variables.Should().Contain("RepositoryUrl");
        variables.Should().Contain("BuildUrl");
        variables.Should().Contain("Duration");
        variables.Should().Contain("Priority");
    }

    [Fact]
    public void GetAvailableVariables_ReturnsDifferentThanEmpty()
    {
        // Act
        var variables = _templateService.GetAvailableVariables();

        // Assert
        variables.Should().NotBeEmpty();
    }

    #endregion

    #region ValidateTemplate Tests

    [Fact]
    public void ValidateTemplate_WithValidTemplate_ReturnsValid()
    {
        // Arrange
        var template = "{{ProjectName}} v{{Version}}";

        // Act
        var (isValid, errors) = _templateService.ValidateTemplate(template);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTemplate_WithUnknownVariable_ReturnsInvalid()
    {
        // Arrange
        var template = "{{UnknownVar}}";

        // Act
        var (isValid, errors) = _templateService.ValidateTemplate(template);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateTemplate_WithMultipleUnknownVariables_ReturnsAllErrors()
    {
        // Arrange
        var template = "{{UnknownVar1}} and {{UnknownVar2}}";

        // Act
        var (isValid, errors) = _templateService.ValidateTemplate(template);

        // Assert
        isValid.Should().BeFalse();
        errors.Count.Should().Be(2);
    }

    [Fact]
    public void ValidateTemplate_WithMixedValidAndInvalid_ReturnsInvalid()
    {
        // Arrange
        var template = "{{ProjectName}} and {{UnknownVar}}";

        // Act
        var (isValid, errors) = _templateService.ValidateTemplate(template);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    #endregion

    #region RenderHtmlSafe Tests

    [Fact]
    public void RenderHtmlSafe_WithHtmlSpecialCharacters_EscapesCharacters()
    {
        // Arrange
        var template = "Message: {{Message}}";
        var notification = new DeploymentNotification
        {
            ProjectName = "App",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Build <failed> & caused issues",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _templateService.RenderHtmlSafe(template, notification);

        // Assert
        result.Should().Contain("&lt;");
        result.Should().Contain("&gt;");
        result.Should().Contain("&amp;");
    }

    [Fact]
    public void RenderHtmlSafe_WithQuotes_EscapesQuotes()
    {
        // Arrange
        var template = "Message: {{Message}}";
        var notification = new DeploymentNotification
        {
            ProjectName = "App",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Build \"failed\" with 'errors'",
            Channels = [NotificationChannel.Slack]
        };

        // Act
        var result = _templateService.RenderHtmlSafe(template, notification);

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region GetPresetTemplates Tests

    [Fact]
    public void GetPresetTemplates_ReturnsTemplates()
    {
        // Act
        var templates = _templateService.GetPresetTemplates();

        // Assert
        templates.Should().NotBeEmpty();
        templates.Should().BeOfType<Dictionary<string, string>>();
    }

    [Fact]
    public void GetPresetTemplates_ContainsCommonTemplates()
    {
        // Act
        var templates = _templateService.GetPresetTemplates();

        // Assert
        templates.Keys.Should().Contain(k => k.ToLower().Contains("success"));
        templates.Keys.Should().Contain(k => k.ToLower().Contains("failed"));
    }

    #endregion

    #region Helper Methods

    private DeploymentNotification CreateTestNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "2.1.0",
            BranchName = "main",
            Message = "Deployed successfully to production",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            CommitHash = "abc1234567890def",
            CommitAuthor = "John Doe",
            RepositoryUrl = "https://github.com/org/repo",
            BuildUrl = "https://ci.example.com/builds/123",
            DurationSeconds = 300,
            Channels = [NotificationChannel.Slack]
        };
    }

    #endregion
}
