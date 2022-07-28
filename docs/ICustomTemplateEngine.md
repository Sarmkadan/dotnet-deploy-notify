# ICustomTemplateEngine

The `ICustomTemplateEngine` interface defines a contract for registering, retrieving, rendering, and managing custom templates used in deployment notification systems. It provides methods to handle template lifecycle operations including registration, validation, rendering, and cleanup, while supporting both inline and file-based template sources.

## API

### `CustomTemplateEngine`

The default implementation of `ICustomTemplateEngine` that handles template storage, rendering, and validation.

### `void RegisterTemplate(CustomTemplate template)`

Registers a new template with the engine.

- **Parameters**
  - `template`: The `CustomTemplate` instance to register. Must not be `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `template` is `null`.
  - Throws `InvalidOperationException` if a template with the same name already exists.

### `CustomTemplate? GetTemplate(string name)`

Retrieves a registered template by name.

- **Parameters**
  - `name`: The name of the template to retrieve.
- **Return Value**
  - The `CustomTemplate` if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `name` is `null`.

### `IReadOnlyList<CustomTemplate> ListTemplates()`

Returns an immutable list of all registered templates.

- **Return Value**
  - A read-only list of `CustomTemplate` instances.

### `bool DeleteTemplate(string name)`

Removes a registered template by name.

- **Parameters**
  - `name`: The name of the template to remove.
- **Return Value**
  - `true` if the template was found and removed; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `name` is `null`.

### `string Render(string name, object? model = null)`

Renders a registered template by name with an optional model.

- **Parameters**
  - `name`: The name of the template to render.
  - `model`: An optional model object to use during rendering.
- **Return Value**
  - The rendered template output as a string.
- **Exceptions**
  - Throws `ArgumentNullException` if `name` is `null`.
  - Throws `KeyNotFoundException` if no template with the given `name` exists.

### `string RenderInline(string templateText, object? model = null)`

Renders an inline template string with an optional model.

- **Parameters**
  - `templateText`: The template text to render.
  - `model`: An optional model object to use during rendering.
- **Return Value**
  - The rendered output as a string.
- **Exceptions**
  - Throws `ArgumentNullException` if `templateText` is `null`.

### `(bool IsValid, List<string> Errors) ValidateTemplate(string name)`

Validates a registered template by name.

- **Parameters**
  - `name`: The name of the template to validate.
- **Return Value**
  - A tuple where `IsValid` indicates whether the template is valid, and `Errors` contains a list of validation error messages (empty if valid).
- **Exceptions**
  - Throws `ArgumentNullException` if `name` is `null`.

### `void LoadPresets()`

Loads a set of predefined templates from the engine's preset library.

- **Exceptions**
  - May throw `IOException` or other I/O-related exceptions if preset files cannot be read.

## Usage

### Registering and Rendering a Template
