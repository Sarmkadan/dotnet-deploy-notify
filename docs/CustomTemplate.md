# CustomTemplate

The `CustomTemplate` type represents a customizable deployment notification template used within the `dotnet-deploy-notify` project. It encapsulates metadata and content for templates that can be dynamically selected and rendered during deployment notifications, supporting categorization, activation states, and lifecycle tracking via creation and update timestamps.

## API

### `Id`
A unique identifier for the template. This value is used to reference the template in the system and must be unique across all templates.

- **Type:** `string`
- **Constraints:** Non-null, non-empty string.
- **Throws:** `ArgumentNullException` or `ArgumentException` if set to `null` or empty.

### `Name`
A human-readable name for the template, typically used in UI or logs to identify the template.

- **Type:** `string`
- **Constraints:** Non-null, non-empty string.
- **Throws:** `ArgumentNullException` or `ArgumentException` if set to `null` or empty.

### `Description`
A detailed explanation of the template’s purpose, usage context, or behavior.

- **Type:** `string`
- **Constraints:** Optional; may be `null` or empty.

### `Content`
The raw template content used to generate deployment notifications. This may include placeholders or formatting directives resolved at render time.

- **Type:** `string`
- **Constraints:** Non-null; may be empty.
- **Throws:** `ArgumentNullException` if set to `null`.

### `CreatedAt`
The timestamp indicating when the template was first created in the system.

- **Type:** `DateTime`
- **Constraints:** Read-only; set once during initialization.
- **Throws:** Not applicable.

### `UpdatedAt`
The timestamp indicating the last time the template was modified.

- **Type:** `DateTime`
- **Constraints:** Read-only; updated automatically by `Touch()`.
- **Throws:** Not applicable.

### `Category`
A classification or grouping for the template, enabling filtering or organization in template selection logic.

- **Type:** `string`
- **Constraints:** Optional; may be `null` or empty.

### `IsActive`
A flag indicating whether the template is currently available for use in deployment notifications.

- **Type:** `bool`
- **Default:** `true`
- **Throws:** Not applicable.

### `Touch()`
Updates the `UpdatedAt` timestamp to the current UTC time, reflecting that the template has been modified.

- **Parameters:** None.
- **Return Value:** `void`
- **Throws:** Not applicable.
- **Side Effects:** Modifies the `UpdatedAt` property.

## Usage
