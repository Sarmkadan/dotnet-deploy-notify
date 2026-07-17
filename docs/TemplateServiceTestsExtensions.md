# TemplateServiceTestsExtensions

`TemplateServiceTestsExtensions` is a static utility class designed to support unit testing of the `TemplateService` in the `dotnet-deploy-notify` project. It provides factory methods for generating test instances of `DeploymentNotification` with various configurations, along with helper methods to validate template rendering behavior. This class is exclusively used in test contexts and is not intended for production code.

## API

### `public static DeploymentNotification CreateTestNotification()`

**Purpose:**
Creates a baseline `DeploymentNotification` instance with default values for testing purposes. This notification includes typical properties such as a repository URL, environment, priority, message, and a non-null duration.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance populated with default test values.

**Throws:**
None.

---

### `public static void ShouldRenderTemplateCorrectly(this TemplateService templateService, DeploymentNotification notification, string expectedOutput)`

**Purpose:**
Validates that the provided `TemplateService` renders the given `DeploymentNotification` into the expected output string. This method is typically used to verify template rendering logic in unit tests.

**Parameters:**
- `templateService` (`TemplateService`): The `TemplateService` instance under test.
- `notification` (`DeploymentNotification`): The notification to render.
- `expectedOutput` (`string`): The expected rendered output string.

**Returns:**
Void.

**Throws:**
- `Xunit.Sdk.EqualException`: Thrown if the rendered output does not match the `expectedOutput`.

---

### `public static DeploymentNotification CreateNotificationWithNullDuration()`

**Purpose:**
Creates a `DeploymentNotification` instance where the `Duration` property is explicitly set to `null`. This is useful for testing edge cases in template rendering or notification processing.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance with a `null` duration.

**Throws:**
None.

---

### `public static DeploymentNotification CreateNotificationWithPriority()`

**Purpose:**
Creates a `DeploymentNotification` instance with a predefined priority value. This method is used to test priority-specific logic in templates or notification pipelines.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance with a non-default priority value.

**Throws:**
None.

---

### `public static DeploymentNotification CreateNotificationWithEnvironment()`

**Purpose:**
Creates a `DeploymentNotification` instance with a predefined environment value. This method is used to test environment-specific logic in templates or notification processing.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance with a non-default environment value.

**Throws:**
None.

---

### `public static TemplateService TemplateService`

**Purpose:**
Provides a pre-configured `TemplateService` instance for testing. This property is typically used to avoid repetitive setup code in unit tests.

**Parameters:**
None (property accessor).

**Returns:**
A `TemplateService` instance.

**Throws:**
None.

---

### `public static ILogger<TemplateService> MockLogger`

**Purpose:**
Provides a mocked `ILogger<TemplateService>` instance for testing. This property is used to verify logging behavior or to suppress logging output during tests.

**Parameters:**
None (property accessor).

**Returns:**
A mocked `ILogger<TemplateService>` instance.

**Throws:**
None.

---

### `public static DeploymentNotification CreateNotificationWithMessage()`

**Purpose:**
Creates a `DeploymentNotification` instance with a predefined message value. This method is used to test message-specific logic in templates or notification processing.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance with a non-default message value.

**Throws:**
None.

---

### `public static DeploymentNotification CreateNotificationWithRepositoryUrl()`

**Purpose:**
Creates a `DeploymentNotification` instance with a predefined repository URL value. This method is used to test repository URL-specific logic in templates or notification processing.

**Parameters:**
None.

**Returns:**
A `DeploymentNotification` instance with a non-default repository URL value.

**Throws:**
None.

## Usage

### Example 1: Testing Template Rendering
