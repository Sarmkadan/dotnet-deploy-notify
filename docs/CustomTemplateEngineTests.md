# CustomTemplateEngineTests
The `CustomTemplateEngineTests` class is a test suite designed to validate the functionality of a custom template engine. This engine is responsible for managing and rendering templates with variable replacements, filters, and other features. The tests in this class cover various scenarios, including template registration, retrieval, deletion, and rendering, to ensure the engine behaves as expected.

## API
The `CustomTemplateEngineTests` class contains the following public members:
* `public CustomTemplateEngineTests`: The constructor for the test class.
* `public void RegisterTemplate_StoresTemplate`: Tests that registering a template stores it correctly.
* `public void RegisterTemplate_OverwritesExisting`: Tests that registering a template with an existing name overwrites the previous template.
* `public void RegisterTemplate_WithNullTemplate_ThrowsArgumentNullException`: Tests that registering a template with a null template throws an `ArgumentNullException`.
* `public void RegisterTemplate_WithEmptyName_ThrowsArgumentException`: Tests that registering a template with an empty name throws an `ArgumentException`.
* `public void GetTemplate_UnknownName_ReturnsNull`: Tests that retrieving a template with an unknown name returns null.
* `public void GetTemplate_IsCaseInsensitive`: Tests that retrieving a template is case-insensitive.
* `public void ListTemplates_ReturnsAllActive`: Tests that listing templates returns all active templates.
* `public void ListTemplates_ExcludesDeletedTemplates`: Tests that listing templates excludes deleted templates.
* `public void DeleteTemplate_ReturnsTrueForExisting`: Tests that deleting a template returns true for existing templates.
* `public void DeleteTemplate_ReturnsFalseForUnknown`: Tests that deleting a template returns false for unknown templates.
* `public void RenderInline_ReplacesProjectName`: Tests that rendering a template inline replaces the project name variable.
* `public void RenderInline_ReplacesVersion`: Tests that rendering a template inline replaces the version variable.
* `public void RenderInline_ReplacesMultipleVariables`: Tests that rendering a template inline replaces multiple variables.
* `public void RenderInline_LeavesUnknownVariablesUnchanged`: Tests that rendering a template inline leaves unknown variables unchanged.
* `public void RenderInline_WithEmptyTemplate_ReturnsEmpty`: Tests that rendering a template inline with an empty template returns an empty string.
* `public void RenderInline_UpperFilter_ConvertsToUpperCase`: Tests that rendering a template inline with the upper filter converts the result to uppercase.
* `public void RenderInline_LowerFilter_ConvertsToLowerCase`: Tests that rendering a template inline with the lower filter converts the result to lowercase.
* `public void RenderInline_TrimFilter_TrimsWhitespace`: Tests that rendering a template inline with the trim filter trims whitespace.
* `public void RenderInline_CustomVariable_OverridesBuiltin`: Tests that rendering a template inline with a custom variable overrides the built-in variable.

## Usage
Here are two examples of using the `CustomTemplateEngineTests` class:
```csharp
// Example 1: Registering and rendering a template
var templateEngine = new CustomTemplateEngine();
templateEngine.RegisterTemplate("myTemplate", "Hello, {{ name }}!");
var renderedTemplate = templateEngine.RenderInline("myTemplate", new { name = "John" });
Console.WriteLine(renderedTemplate); // Output: Hello, John!

// Example 2: Listing and deleting templates
var templateEngine = new CustomTemplateEngine();
templateEngine.RegisterTemplate("template1", "Template 1");
templateEngine.RegisterTemplate("template2", "Template 2");
var templates = templateEngine.ListTemplates();
Console.WriteLine(templates.Count); // Output: 2
templateEngine.DeleteTemplate("template1");
templates = templateEngine.ListTemplates();
Console.WriteLine(templates.Count); // Output: 1
```

## Notes
The `CustomTemplateEngineTests` class is designed to be thread-safe, as it does not maintain any internal state between test runs. However, the custom template engine itself may not be thread-safe, depending on its implementation. When using the template engine in a multi-threaded environment, it is recommended to synchronize access to the engine to prevent concurrent modifications.

Additionally, the `RenderInline` method may throw exceptions if the template contains invalid syntax or if the variables are not properly replaced. It is recommended to handle these exceptions accordingly in production code.
