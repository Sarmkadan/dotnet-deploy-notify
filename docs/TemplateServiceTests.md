# TemplateServiceTests
The `TemplateServiceTests` class is a test suite designed to validate the functionality of the `TemplateService` class, which is responsible for rendering templates with variables replaced by notification values. This test class ensures that the `TemplateService` correctly handles various scenarios, including rendering templates with different types of variables, such as project name, version, status, and more.

## API
The `TemplateServiceTests` class contains a set of test methods that cover different aspects of the `TemplateService` class. Each method tests a specific scenario, including:
* `RenderTemplate_WithProjectNameVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the project name variable with the notification value.
* `RenderTemplate_WithVersionVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the version variable with the notification value.
* `RenderTemplate_WithStatusVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the status variable with the notification value.
* `RenderTemplate_WithMultipleVariables_ReplacesAllVariables`: Tests that the `TemplateService` replaces all variables in a template.
* `RenderTemplate_WithUnknownVariable_LeavesVariableUnchanged`: Tests that the `TemplateService` leaves unknown variables unchanged.
* `RenderTemplate_WithEnvironmentVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the environment variable with the notification value.
* `RenderTemplate_WithCommitHashVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the commit hash variable with the notification value.
* `RenderTemplate_WithCommitHashShortVariable_Returns7CharHash`: Tests that the `TemplateService` returns a 7-character commit hash.
* `RenderTemplate_WithCommitHashShortVariable_WithShortHash_ReturnsFullHash`: Tests that the `TemplateService` returns the full commit hash when the short hash is provided.
* `RenderTemplate_WithCommitAuthorVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the commit author variable with the notification value.
* `RenderTemplate_WithRepositoryUrlVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the repository URL variable with the notification value.
* `RenderTemplate_WithBuildUrlVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the build URL variable with the notification value.
* `RenderTemplate_WithDurationVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the duration variable with the notification value.
* `RenderTemplate_WithDurationVariable_AndNullDuration_ReturnsNA`: Tests that the `TemplateService` returns "NA" when the duration is null.
* `RenderTemplate_WithPriorityVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the priority variable with the notification value.
* `RenderTemplate_WithMessageVariable_ReplacesWithNotificationValue`: Tests that the `TemplateService` replaces the message variable with the notification value.
* `RenderTemplate_WithEmptyTemplate_ReturnsEmptyString`: Tests that the `TemplateService` returns an empty string when the template is empty.
* `RenderTemplate_WithNullTemplate_ReturnsEmptyString`: Tests that the `TemplateService` returns an empty string when the template is null.
* `RenderTemplate_WithNoVariables_ReturnsTemplateUnchanged`: Tests that the `TemplateService` returns the template unchanged when there are no variables.

## Usage
Here are two examples of using the `TemplateServiceTests` class:
```csharp
// Example 1: Testing the RenderTemplate method with a project name variable
[TestMethod]
public void RenderTemplate_WithProjectNameVariable_ReplacesWithNotificationValue()
{
    // Arrange
    var templateService = new TemplateService();
    var template = "Project: {{projectName}}";
    var notification = new Notification { ProjectName = "My Project" };

    // Act
    var result = templateService.RenderTemplate(template, notification);

    // Assert
    Assert.AreEqual("Project: My Project", result);
}

// Example 2: Testing the RenderTemplate method with multiple variables
[TestMethod]
public void RenderTemplate_WithMultipleVariables_ReplacesAllVariables()
{
    // Arrange
    var templateService = new TemplateService();
    var template = "Project: {{projectName}}, Version: {{version}}, Status: {{status}}";
    var notification = new Notification { ProjectName = "My Project", Version = "1.0", Status = "Success" };

    // Act
    var result = templateService.RenderTemplate(template, notification);

    // Assert
    Assert.AreEqual("Project: My Project, Version: 1.0, Status: Success", result);
}
```

## Notes
The `TemplateServiceTests` class is designed to be thread-safe, as each test method creates a new instance of the `TemplateService` class. However, it is essential to note that the `TemplateService` class itself may not be thread-safe, depending on its implementation. Additionally, the test class does not handle any exceptions that may be thrown by the `TemplateService` class, so it is crucial to ensure that the `TemplateService` class is properly exception-handled in a production environment. Edge cases, such as null or empty templates, are also handled by the test class to ensure that the `TemplateService` class behaves correctly in these scenarios.
