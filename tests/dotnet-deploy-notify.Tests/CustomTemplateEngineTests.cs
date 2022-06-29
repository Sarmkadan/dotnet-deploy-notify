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

public class CustomTemplateEngineTests
{
    private readonly CustomTemplateEngine _engine;

    public CustomTemplateEngineTests()
    {
        var logger = Substitute.For<ILogger<CustomTemplateEngine>>();
        _engine = new CustomTemplateEngine(logger);
    }

    // ─── RegisterTemplate / GetTemplate ────────────────────────────────────

    [Fact]
    public void RegisterTemplate_StoresTemplate()
    {
        var template = CreateTemplate("MyTpl", "Hello {{ProjectName}}");
        _engine.RegisterTemplate(template);
        _engine.GetTemplate("MyTpl").Should().NotBeNull();
    }

    [Fact]
    public void RegisterTemplate_OverwritesExisting()
    {
        _engine.RegisterTemplate(CreateTemplate("Tpl", "v1 {{Version}}"));
        _engine.RegisterTemplate(CreateTemplate("Tpl", "v2 {{ProjectName}}"));

        _engine.GetTemplate("Tpl")!.Content.Should().Be("v2 {{ProjectName}}");
    }

    [Fact]
    public void RegisterTemplate_WithNullTemplate_ThrowsArgumentNullException()
    {
        Action act = () => _engine.RegisterTemplate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterTemplate_WithEmptyName_ThrowsArgumentException()
    {
        var template = CreateTemplate(string.Empty, "content");
        Action act = () => _engine.RegisterTemplate(template);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetTemplate_UnknownName_ReturnsNull()
    {
        _engine.GetTemplate("DoesNotExist").Should().BeNull();
    }

    [Fact]
    public void GetTemplate_IsCaseInsensitive()
    {
        _engine.RegisterTemplate(CreateTemplate("Alert", "{{Status}}"));
        _engine.GetTemplate("ALERT").Should().NotBeNull();
    }

    // ─── ListTemplates ──────────────────────────────────────────────────────

    [Fact]
    public void ListTemplates_ReturnsAllActive()
    {
        _engine.RegisterTemplate(CreateTemplate("T1", "a"));
        _engine.RegisterTemplate(CreateTemplate("T2", "b"));

        _engine.ListTemplates().Should().HaveCount(2);
    }

    [Fact]
    public void ListTemplates_ExcludesDeletedTemplates()
    {
        _engine.RegisterTemplate(CreateTemplate("Keep", "a"));
        _engine.RegisterTemplate(CreateTemplate("Remove", "b"));
        _engine.DeleteTemplate("Remove");

        _engine.ListTemplates().Should().ContainSingle(t => t.Name == "Keep");
    }

    // ─── DeleteTemplate ──────────────────────────────────────────────────────

    [Fact]
    public void DeleteTemplate_ReturnsTrueForExisting()
    {
        _engine.RegisterTemplate(CreateTemplate("TplDel", "content"));
        _engine.DeleteTemplate("TplDel").Should().BeTrue();
    }

    [Fact]
    public void DeleteTemplate_ReturnsFalseForUnknown()
    {
        _engine.DeleteTemplate("Ghost").Should().BeFalse();
    }

    // ─── RenderInline – variable substitution ───────────────────────────────

    [Fact]
    public void RenderInline_ReplacesProjectName()
    {
        var notification = CreateNotification();
        _engine.RenderInline("Project: {{ProjectName}}", notification)
               .Should().Be("Project: TestApp");
    }

    [Fact]
    public void RenderInline_ReplacesVersion()
    {
        var notification = CreateNotification();
        _engine.RenderInline("v{{Version}}", notification)
               .Should().Be("v2.0.0");
    }

    [Fact]
    public void RenderInline_ReplacesMultipleVariables()
    {
        var notification = CreateNotification();
        var result = _engine.RenderInline("{{ProjectName}} v{{Version}} [{{Status}}]", notification);
        result.Should().Be("TestApp v2.0.0 [Success]");
    }

    [Fact]
    public void RenderInline_LeavesUnknownVariablesUnchanged()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{Unknown}}", notification)
               .Should().Be("{{Unknown}}");
    }

    [Fact]
    public void RenderInline_WithEmptyTemplate_ReturnsEmpty()
    {
        _engine.RenderInline(string.Empty, CreateNotification())
               .Should().BeEmpty();
    }

    // ─── RenderInline – filters ─────────────────────────────────────────────

    [Fact]
    public void RenderInline_UpperFilter_ConvertsToUpperCase()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{ProjectName | upper}}", notification)
               .Should().Be("TESTAPP");
    }

    [Fact]
    public void RenderInline_LowerFilter_ConvertsToLowerCase()
    {
        var notification = CreateNotification();
        _engine.RenderInline("{{Status | lower}}", notification)
               .Should().Be("success");
    }

    [Fact]
    public void RenderInline_TrimFilter_TrimsWhitespace()
    {
        var notification = new DeploymentNotification
        {
            ProjectName = "  SpacedApp  ",
            Version = "1.0",
            BranchName = "main",
            Channels = [NotificationChannel.Slack]
        };
        _engine.RenderInline("'{{ProjectName | trim}}'", notification)
               .Should().Be("'SpacedApp'");
    }

    // ─── RenderInline – custom variables ────────────────────────────────────

    [Fact]
    public void RenderInline_CustomVariable_OverridesBuiltin()
    {
        var notification = CreateNotification();
        var custom = new Dictionary<string, string> { ["ProjectName"] = "OverriddenApp" };

        _engine.RenderInline("{{ProjectName}}", notification, custom)
               .Should().Be("OverriddenApp");
    }

    [Fact]
    public void RenderInline_CustomVariable_AppearsInOutput()
    {
        var notification = CreateNotification();
        var custom = new Dictionary<string, string> { ["Deployer"] = "jenkins-bot" };

        _engine.RenderInline("Deployed by {{Deployer}}", notification, custom)
               .Should().Be("Deployed by jenkins-bot");
    }

    // ─── RenderInline – conditional blocks ──────────────────────────────────

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

    [Fact]
    public void Render_KnownTemplate_ReturnsRenderedContent()
    {
        _engine.RegisterTemplate(CreateTemplate("Greeting", "Hello from {{ProjectName}}"));
        var notification = CreateNotification();

        _engine.Render("Greeting", notification)
               .Should().Be("Hello from TestApp");
    }

    [Fact]
    public void Render_UnknownTemplate_ThrowsKeyNotFoundException()
    {
        Action act = () => _engine.Render("NoSuchTemplate", CreateNotification());
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Render_DeletedTemplate_ThrowsKeyNotFoundException()
    {
        _engine.RegisterTemplate(CreateTemplate("Gone", "content"));
        _engine.DeleteTemplate("Gone");

        Action act = () => _engine.Render("Gone", CreateNotification());
        act.Should().Throw<KeyNotFoundException>();
    }

    // ─── ValidateTemplate ───────────────────────────────────────────────────

    [Fact]
    public void ValidateTemplate_ValidTemplate_ReturnsNoErrors()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{ProjectName}} v{{Version}}");
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTemplate_UnknownVariable_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{UnknownVar}}");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("UnknownVar"));
    }

    [Fact]
    public void ValidateTemplate_UnknownFilter_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{ProjectName | explode}}");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("explode"));
    }

    [Fact]
    public void ValidateTemplate_UnbalancedConditional_ReturnsError()
    {
        var (isValid, errors) = _engine.ValidateTemplate("{{#if Status == \"ok\"}}text with no close");
        isValid.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Unbalanced"));
    }

    [Fact]
    public void ValidateTemplate_EmptyTemplate_IsValid()
    {
        var (isValid, errors) = _engine.ValidateTemplate(string.Empty);
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    // ─── LoadPresets ────────────────────────────────────────────────────────

    [Fact]
    public void LoadPresets_RegistersBuiltinTemplates()
    {
        _engine.LoadPresets();

        _engine.ListTemplates().Should().NotBeEmpty();
        _engine.GetTemplate("SuccessAlert").Should().NotBeNull();
        _engine.GetTemplate("FailureAlert").Should().NotBeNull();
    }

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
