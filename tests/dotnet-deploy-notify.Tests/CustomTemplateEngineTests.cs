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

/// <summary>
/// Unit tests for the <see cref="CustomTemplateEngine"/> class.
/// Tests template registration, retrieval, rendering, validation, and preset loading functionality.
/// </summary>
public class CustomTemplateEngineTests
{
    private readonly CustomTemplateEngine _engine;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTemplateEngineTests"/> class.
    /// Sets up a mocked logger and creates a <see cref="CustomTemplateEngine"/> instance for testing.
    /// </summary>
    public CustomTemplateEngineTests()
    {
        var logger = Substitute.For<ILogger<CustomTemplateEngine>>();
        _engine = new CustomTemplateEngine(logger);
    }

    // ─── RegisterTemplate / GetTemplate ────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RegisterTemplate(CustomTemplate)"/> correctly stores a template
    /// and that <see cref="CustomTemplateEngine.GetTemplate(string)"/> can retrieve it.
    /// </summary>
    [Fact]
    public void RegisterTemplate_StoresTemplate()
    {
        var template = CreateTemplate("MyTpl", "Hello {{ProjectName}}");
        _engine.RegisterTemplate(template);
        _engine.GetTemplate("MyTpl").Should().NotBeNull();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RegisterTemplate(CustomTemplate)"/> overwrites an existing template
    /// with the same name, ensuring the latest version is stored.
    /// </summary>
    [Fact]
    public void RegisterTemplate_OverwritesExisting()
    {
        _engine.RegisterTemplate(CreateTemplate("Tpl", "v1 {{Version}}"));
        _engine.RegisterTemplate(CreateTemplate("Tpl", "v2 {{ProjectName}}"));

        _engine.GetTemplate("Tpl")!.Content.Should().Be("v2 {{ProjectName}}");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RegisterTemplate(CustomTemplate)"/> throws an <see cref="ArgumentNullException"/>
    /// when a null template is provided.
    /// </summary>
    [Fact]
    public void RegisterTemplate_WithNullTemplate_ThrowsArgumentNullException()
    {
        Action act = () => _engine.RegisterTemplate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RegisterTemplate(CustomTemplate)"/> throws an <see cref="ArgumentException"/>
    /// when a template with an empty name is provided.
    /// </summary>
    [Fact]
    public void RegisterTemplate_WithEmptyName_ThrowsArgumentException()
    {
        var template = CreateTemplate(string.Empty, "content");
        Action act = () => _engine.RegisterTemplate(template);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.GetTemplate(string)"/> returns null when querying for a template
    /// that does not exist in the engine's registry.
    /// </summary>
    [Fact]
    public void GetTemplate_UnknownName_ReturnsNull()
    {
        _engine.GetTemplate("DoesNotExist").Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.GetTemplate(string)"/> performs case-insensitive template name matching,
    /// allowing retrieval of templates regardless of name casing.
    /// </summary>
    [Fact]
    public void GetTemplate_IsCaseInsensitive()
    {
        _engine.RegisterTemplate(CreateTemplate("Alert", "{{Status}}"));
        _engine.GetTemplate("ALERT").Should().NotBeNull();
    }

    // ─── ListTemplates ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ListTemplates()"/> returns all registered active templates,
    /// excluding any that have been deleted.
    /// </summary>
    [Fact]
    public void ListTemplates_ReturnsAllActive()
    {
        _engine.RegisterTemplate(CreateTemplate("T1", "a"));
        _engine.RegisterTemplate(CreateTemplate("T2", "b"));

        _engine.ListTemplates().Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ListTemplates()"/> excludes deleted templates from the returned list,
    /// ensuring only active templates are included.
    /// </summary>
    [Fact]
    public void ListTemplates_ExcludesDeletedTemplates()
    {
        _engine.RegisterTemplate(CreateTemplate("Keep", "a"));
        _engine.RegisterTemplate(CreateTemplate("Remove", "b"));
        _engine.DeleteTemplate("Remove");

        _engine.ListTemplates().Should().ContainSingle(t => t.Name == "Keep");
    }

    // ─── DeleteTemplate ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.DeleteTemplate(string)"/> returns true when successfully deleting an existing template.
    /// </summary>
    [Fact]
    public void DeleteTemplate_ReturnsTrueForExisting()
    {
        _engine.RegisterTemplate(CreateTemplate("TplDel", "content"));
        _engine.DeleteTemplate("TplDel").Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.DeleteTemplate(string)"/> returns false when attempting to delete a template
    /// that does not exist in the registry.
    /// </summary>
    [Fact]
    public void DeleteTemplate_ReturnsFalseForUnknown()
    {
        _engine.DeleteTemplate("Ghost").Should().BeFalse();
    }

    // ─── RenderInline – variable substitution ───────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> correctly replaces the "ProjectName" variable
    /// in the template with the actual project name from the deployment notification.
    /// </summary>
    [Fact]
    public void RenderInline_ReplacesProjectName()
    {
        var notification = CreateNotification();
        _engine.RenderInline("Project: {{ProjectName}}", notification)
            .Should().Be("Project: TestApp");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> correctly replaces the "Version" variable
    /// in the template with the actual version from the deployment notification.
    /// </summary>
    [Fact]
    public void RenderInline_ReplacesVersion()
    {
        var notification = CreateNotification();
        _engine.RenderInline("v{{Version}}", notification)
            .Should().Be("v2.0.0");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> correctly replaces multiple variables
    /// in the template with their corresponding values from the deployment notification.
    /// </summary>
    [Fact]
    public void RenderInline_ReplacesMultipleVariables()
    {
        var notification = CreateNotification();
        var result = _engine.RenderInline("{{ProjectName}} v{{Version}} [{{Status}}]", notification);
        result.Should().Be("TestApp v2.0.0 [Success]");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> leaves unknown variables unchanged
    /// in the output when they are not defined in the deployment notification.
    /// </summary>
    [Fact]
    public void RenderInline_LeavesUnknownVariablesUnchanged()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{Unknown}}", notification)
            .Should().Be("{{Unknown}}");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> returns an empty string
    /// when an empty template is provided.
    /// </summary>
    [Fact]
    public void RenderInline_WithEmptyTemplate_ReturnsEmpty()
    {
        _engine.RenderInline(string.Empty, CreateNotification())
            .Should().BeEmpty();
    }

    // ─── RenderInline – filters ─────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> applies the "upper" filter
    /// to convert the project name to uppercase.
    /// </summary>
    [Fact]
    public void RenderInline_UpperFilter_ConvertsToUpperCase()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{ProjectName | upper}}", notification)
            .Should().Be("TESTAPP");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> applies the "lower" filter
    /// to convert the status to lowercase.
    /// </summary>
    [Fact]
    public void RenderInline_LowerFilter_ConvertsToLowerCase()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{Status | lower}}", notification)
            .Should().Be("success");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> applies the "trim" filter
    /// to remove leading and trailing whitespace from the project name.
    /// </summary>
    [Fact]
    public void RenderInline_TrimFilter_TrimsWhitespace()
    {
        var notification = new DeploymentNotification
        {
            ProjectName = " SpacedApp ",
            Version = "1.0",
            BranchName = "main",
            Channels = [NotificationChannel.Slack]
        };
        _engine.RenderInline("'{{ProjectName | trim}}'", notification)
            .Should().Be("'SpacedApp'");
    }

    // ─── RenderInline – custom variables ────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification, IDictionary{string, string})"/> allows custom variables
    /// to override built-in variables like "ProjectName".
    /// </summary>
    [Fact]
    public void RenderInline_CustomVariable_OverridesBuiltin()
    {
        var notification = CreateNotification();
        var custom = new Dictionary<string, string> { ["ProjectName"] = "OverriddenApp" };

        _engine.RenderInline("{{ProjectName}}", notification, custom)
            .Should().Be("OverriddenApp");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification, IDictionary{string, string})"/> includes custom variables
    /// that are not defined in the deployment notification.
    /// </summary>
    [Fact]
    public void RenderInline_CustomVariable_AppearsInOutput()
    {
        var notification = CreateNotification();
        var custom = new Dictionary<string, string> { ["Deployer"] = "jenkins-bot" };

        _engine.RenderInline("Deployed by {{Deployer}}", notification, custom)
            .Should().Be("Deployed by jenkins-bot");
    }

    // ─── RenderInline – conditional blocks ──────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> renders conditional blocks
    /// when the condition matches the actual status.
    /// </summary>
    [Fact]
    public void RenderInline_ConditionalBlock_ShowsWhenMatches()
    {
        var notification = CreateNotification();
        notification.Status = BuildStatus.Failed;

        var result = _engine.RenderInline(
            "{{#if Status == \"Failed\"}}ALERT: build broken{{/if}}",
            notification);

        result.Should().Contain("ALERT: build broken");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> hides conditional blocks
    /// when the condition does not match the actual status.
    /// </summary>
    [Fact]
    public void RenderInline_ConditionalBlock_HidesWhenNotMatches()
    {
        var notification = CreateNotification();
        notification.Status = BuildStatus.Success;

        var result = _engine.RenderInline(
            "{{#if Status == \"Failed\"}}ALERT: build broken{{/if}}",
            notification);

        result.Trim().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.RenderInline(string, DeploymentNotification)"/> performs case-insensitive comparison
    /// for conditional blocks.
    /// </summary>
    [Fact]
    public void RenderInline_ConditionalBlock_CaseInsensitiveComparison()
    {
        var notification = CreateNotification();
        notification.Status = BuildStatus.Success;

        var result = _engine.RenderInline(
            "{{#if Status == \"success\"}}OK{{/if}}",
            notification);

        result.Should().Contain("OK");
    }

    // ─── Render (named template) ────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.Render(string, DeploymentNotification)"/> successfully renders a named template
    /// when it exists in the registry.
    /// </summary>
    [Fact]
    public void Render_KnownTemplate_ReturnsRenderedContent()
    {
        _engine.RegisterTemplate(CreateTemplate("Greeting", "Hello from {{ProjectName}}"));
        var notification = CreateNotification();

        _engine.Render("Greeting", notification)
            .Should().Be("Hello from TestApp");
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.Render(string, DeploymentNotification)"/> throws a <see cref="KeyNotFoundException"/>
    /// when attempting to render a template that does not exist.
    /// </summary>
    [Fact]
    public void Render_UnknownTemplate_ThrowsKeyNotFoundException()
    {
        Action act = () => _engine.Render("NoSuchTemplate", CreateNotification());
        act.Should().Throw<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.Render(string, DeploymentNotification)"/> throws a <see cref="KeyNotFoundException"/>
    /// when attempting to render a template that has been deleted.
    /// </summary>
    [Fact]
    public void Render_DeletedTemplate_ThrowsKeyNotFoundException()
    {
        _engine.RegisterTemplate(CreateTemplate("Gone", "content"));
        _engine.DeleteTemplate("Gone");

        Action act = () => _engine.Render("Gone", CreateNotification());
        act.Should().Throw<KeyNotFoundException>();
    }

    // ─── ValidateTemplate ───────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ValidateTemplate(string)"/> returns valid for a properly formatted template.
    /// </summary>
    [Fact]
    public void ValidateTemplate_ValidTemplate_ReturnsNoErrors()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{ProjectName}} v{{Version}}");
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ValidateTemplate(string)"/> detects unknown variables
    /// and returns appropriate error messages.
    /// </summary>
    [Fact]
    public void ValidateTemplate_UnknownVariable_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{UnknownVar}}");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("UnknownVar"));
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ValidateTemplate(string)"/> detects unknown filters
    /// and returns appropriate error messages.
    /// </summary>
    [Fact]
    public void ValidateTemplate_UnknownFilter_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{ProjectName | explode}}");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("explode"));
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ValidateTemplate(string)"/> detects unbalanced conditional blocks
    /// and returns appropriate error messages.
    /// </summary>
    [Fact]
    public void ValidateTemplate_UnbalancedConditional_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{#if Status == \"ok\"}}text with no close");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Unbalanced"));
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.ValidateTemplate(string)"/> considers an empty template as valid.
    /// </summary>
    [Fact]
    public void ValidateTemplate_EmptyTemplate_IsValid()
    {
        var (isValid, errors) = _engine.ValidateTemplate(string.Empty);
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    // ─── LoadPresets ────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.LoadPresets()"/> registers built-in templates
    /// including SuccessAlert and FailureAlert.
    /// </summary>
    [Fact]
    public void LoadPresets_RegistersBuiltinTemplates()
    {
        _engine.LoadPresets();

        _engine.ListTemplates().Should().NotBeEmpty();
        _engine.GetTemplate("SuccessAlert").Should().NotBeNull();
        _engine.GetTemplate("FailureAlert").Should().NotBeNull();
    }

    /// <summary>
    /// Tests that <see cref="CustomTemplateEngine.LoadPresets()"/> registers templates that can be rendered
    /// without throwing exceptions for valid deployment notifications.
    /// </summary>
    [Fact]
    public void LoadPresets_PresetsRenderWithoutErrors()
    {
        _engine.LoadPresets();
        var notification = CreateNotification();

        foreach (var template in _engine.ListTemplates())
        {
            var rendered = _engine.Render(template.Name, notification);
            rendered.Should().NotBeNull();
        }
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static CustomTemplate CreateTemplate(string name, string content)
    {
        return new CustomTemplate { Name = name, Content = content };
    }

    private static DeploymentNotification CreateNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "2.0.0",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "abc1234def",
            CommitAuthor = "dev",
            Message = "Deployed OK",
            BuildUrl = "https://ci.example.com/1",
            RepositoryUrl = "https://github.com/org/repo",
            DurationSeconds = 120,
            Channels = [NotificationChannel.Slack]
        };
    }
}
