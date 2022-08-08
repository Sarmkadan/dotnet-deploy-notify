# ITemplateService

The `ITemplateService` interface defines the contract for managing, validating, and rendering text templates within the `dotnet-deploy-notify` application. It provides functionality to retrieve preset templates, identify available substitution variables, validate template syntax against those variables, and render final output in both raw and HTML-safe formats. This service acts as the central hub for dynamic message generation used in deployment notifications.

## API

### `RenderTemplate`
Renders a specific template string by replacing placeholders with provided variable values.
*   **Purpose**: Generates the final content by merging a template definition with a context of key-value pairs.
*   **Parameters**: Accepts the template string and a dictionary of variables (keys matching placeholder names, values being the replacement text).
*   **Return Value**: Returns a `string` containing the fully rendered content.
*   **Throws**: Throws an exception if a required variable defined in the template is missing from the provided dictionary or if the template syntax is malformed.

### `GetAvailableVariables`
Retrieves a list of all valid placeholder identifiers supported by the current template engine context.
*   **Purpose**: Allows consumers to discover which dynamic fields can be used when constructing custom templates.
*   **Parameters**: None.
*   **Return Value**: Returns a `List<string>` containing the names of available variables.
*   **Throws**: Does not throw under normal operation; returns an empty list if no variables are registered.

### `ValidateTemplate`
Checks a template string for syntax errors and references to undefined variables.
*   **Purpose**: Ensures a template is safe and correct before saving or attempting to render it.
*   **Parameters**: Accepts the template string to validate.
*   **Return Value**: Returns a tuple `(bool IsValid, List<string> Errors)`. `IsValid` is `true` if the template passes all checks; `Errors` contains a list of descriptive error messages if validation fails.
*   **Throws**: Does not throw; errors are returned via the tuple.

### `GetPresetTemplates`
Provides a collection of built-in templates ready for immediate use.
*   **Purpose**: Supplies default notification formats (e.g., "Deployment Success", "Build Failed") so users do not need to write templates from scratch.
*   **Parameters**: None.
*   **Return Value**: Returns a `Dictionary<string, string>` where the key is the template name and the value is the template content.
*   **Throws**: Does not throw.

### `RenderHtmlSafe`
Renders a template and applies HTML encoding to the result to prevent injection attacks.
*   **Purpose**: Generates output suitable for embedding directly into HTML emails or web dashboards, ensuring special characters are escaped.
*   **Parameters**: Accepts the template string and a dictionary of variables.
*   **Return Value**: Returns a `string` containing the rendered and HTML-encoded content.
*   **Throws**: Throws the same exceptions as `RenderTemplate` if variable substitution fails.

### `TemplateService`
*Note: While listed in the public surface, `TemplateService` is the concrete implementation class of this interface.*
*   **Purpose**: The primary instantiation point for the templating logic.
*   **Parameters**: Constructor arguments typically include configuration for variable resolvers or encoding rules.
*   **Return Value**: An instance of the service implementing `ITemplateService`.
*   **Throws**: May throw during initialization if dependencies are misconfigured.

## Usage

### Example 1: Validating and Rendering a Custom Notification
This example demonstrates retrieving available variables, validating a user-provided template, and rendering it for a console output.

```csharp
public void SendCustomNotification(ITemplateService templateService, Dictionary<string, string> context)
{
    string customTemplate = "Deployment to {{Environment}} completed with status: {{Status}}.";

    // Validate before rendering
    var validation = templateService.ValidateTemplate(customTemplate);
    if (!validation.IsValid)
    {
        Console.WriteLine("Template invalid: " + string.Join(", ", validation.Errors));
        return;
    }

    // Render the final message
    try 
    {
        string result = templateService.RenderTemplate(customTemplate, context);
        Console.WriteLine(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Rendering failed: {ex.Message}");
    }
}
```

### Example 2: Generating an HTML-Safe Email Body from a Preset
This example fetches a built-in template and renders it specifically for an HTML email body, ensuring safety against XSS.

```csharp
public string PrepareEmailBody(ITemplateService templateService, Dictionary<string, string> deployData)
{
    var presets = templateService.GetPresetTemplates();
    
    if (!presets.TryGetValue("DeploymentSuccessHtml", out string template))
    {
        throw new InvalidOperationException("Preset template not found.");
    }

    // Render with HTML encoding applied automatically
    return templateService.RenderHtmlSafe(template, deployData);
}
```

## Notes

*   **Variable Consistency**: The `GetAvailableVariables` method reflects the variables supported by the current service configuration. If `RenderTemplate` is called with a placeholder not present in this list (and not handled by a fallback mechanism), it will likely result in a validation error or a runtime exception depending on the strictness of the implementation.
*   **HTML Encoding Scope**: `RenderHtmlSafe` applies encoding to the *final* rendered string. It does not selectively encode only the substituted values; the entire output is treated as unsafe text content. Do not use this method if the template intentionally contains raw HTML tags that should remain unescaped.
*   **Thread Safety**: The interface methods imply stateless operations regarding the input parameters. However, implementations relying on shared internal caches for presets or variable definitions should be verified for thread safety. In a multi-threaded deployment pipeline, it is recommended to treat the `ITemplateService` instance as immutable after construction or ensure the concrete `TemplateService` class handles concurrent read access correctly.
*   **Error Handling**: `ValidateTemplate` is the preferred method for checking correctness as it aggregates all errors into a list rather than failing on the first encounter and throwing an exception. Use `ValidateTemplate` prior to calling `RenderTemplate` in user-facing editors.
