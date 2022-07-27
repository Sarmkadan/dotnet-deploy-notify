# CommandParser

The `CommandParser` class provides a mechanism for defining, registering, and parsing command-line arguments within the `dotnet-deploy-notify` application. It supports the registration of commands with specific parameters and options, handles the parsing logic to populate these values from input strings, and generates formatted help text for both the overall parser and individual commands. The class maintains state regarding the success of the last parse operation, any resulting errors, and the extracted output, parameters, and options.

## API

### Constructors

#### `public CommandParser()`
Initializes a new instance of the `CommandParser` class.

### Methods

#### `public void RegisterCommand(string name, string description, List<ParameterDefinition> parameters, List<OptionDefinition> options)`
Registers a new command with the parser.
*   **Parameters**:
    *   `name`: The unique identifier for the command.
    *   `description`: A brief description of what the command does.
    *   `parameters`: A list of `ParameterDefinition` objects defining required positional arguments.
    *   `options`: A list of `OptionDefinition` objects defining optional flags or key-value pairs.
*   **Returns**: `void`.
*   **Throws**: May throw an exception if a command with the same `name` is already registered or if definitions are invalid.

#### `public ParsedCommand Parse(string[] args)`
Parses the provided command-line arguments against the registered commands.
*   **Parameters**:
    *   `args`: The array of string arguments to parse (typically `Environment.GetCommandLineArgs()` excluding the executable path).
*   **Returns**: A `ParsedCommand` object representing the matched command and its parsed data.
*   **Throws**: May throw if the arguments do not match any registered command structure and strict mode is enforced, though typically errors are reported via the `Error` property.

#### `public string GetHelpText()`
Generates a comprehensive help string displaying all registered commands and their usage patterns.
*   **Parameters**: None.
*   **Returns**: A formatted string containing global help information.
*   **Throws**: None.

#### `public string GetCommandHelpText(string commandName)`
Generates a detailed help string for a specific command.
*   **Parameters**:
    *   `commandName`: The name of the command to retrieve help for.
*   **Returns**: A formatted string containing usage, parameters, and options for the specified command.
*   **Throws**: May throw if `commandName` does not correspond to a registered command.

#### `public string? GetParameter(string name)`
Retrieves the value of a specific parameter from the last parse operation.
*   **Parameters**:
    *   `name`: The name of the parameter.
*   **Returns**: The parameter value as a string, or `null` if the parameter was not provided or the parse failed.
*   **Throws**: None.

#### `public string? GetOption(string name)`
Retrieves the value of a specific option from the last parse operation.
*   **Parameters**:
    *   `name`: The name of the option.
*   **Returns**: The option value as a string, or `null` if the option was not provided.
*   **Throws**: None.

#### `public bool HasOption(string name)`
Checks whether a specific option was present in the last parse operation.
*   **Parameters**:
    *   `name`: The name of the option.
*   **Returns**: `true` if the option was provided; otherwise, `false`.
*   **Throws**: None.

### Properties

#### `public string CommandName`
Gets the name of the command identified during the last parse operation. Returns `null` or empty if parsing has not occurred or failed to identify a command.

#### `public bool Success`
Gets a value indicating whether the last parse operation completed successfully without errors.

#### `public string? Error`
Gets the error message generated during the last parse operation, if any. Returns `null` if the operation was successful.

#### `public string? Output`
Gets the standard output content generated during command execution or parsing feedback. Returns `null` if no output was captured.

#### `public Dictionary<string, string> Parameters`
Gets the dictionary of all parameters extracted during the last parse operation, mapping parameter names to their values.

#### `public Dictionary<string, string> Options`
Gets the dictionary of all options extracted during the last parse operation, mapping option names to their values.

#### `public string Name`
Gets the name associated with the current command context or definition.

#### `public string Description`
Gets the description associated with the current command context or definition.

#### `public List<ParameterDefinition> Parameters`
Gets the list of `ParameterDefinition` objects representing the schema of parameters for the current command.

#### `public List<OptionDefinition> Options`
Gets the list of `OptionDefinition` objects representing the schema of options for the current command.

## Usage

### Example 1: Registering and Parsing a Deploy Command
This example demonstrates how to initialize the parser, register a command with required parameters and optional flags, and process user input.

```csharp
var parser = new CommandParser();

// Define parameters and options for the 'deploy' command
var parameters = new List<ParameterDefinition>
{
    new ParameterDefinition("environment", "The target environment (e.g., prod, staging)")
};

var options = new List<OptionDefinition>
{
    new OptionDefinition("verbose", "v", "Enable verbose logging", false),
    new OptionDefinition("config", "c", "Path to configuration file", true)
};

parser.RegisterCommand("deploy", "Deploy the application to a specific environment", parameters, options);

// Simulate command line input: deploy prod --verbose --config ./appsettings.json
var args = new[] { "deploy", "prod", "--verbose", "--config", "./appsettings.json" };
var result = parser.Parse(args);

if (parser.Success)
{
    var env = parser.GetParameter("environment");
    var isVerbose = parser.HasOption("verbose");
    var configPath = parser.GetOption("config");

    Console.WriteLine($"Deploying to {env}");
    if (isVerbose) Console.WriteLine("Verbose mode enabled");
    if (!string.IsNullOrEmpty(configPath)) Console.WriteLine($"Using config: {configPath}");
}
else
{
    Console.WriteLine($"Parse failed: {parser.Error}");
    Console.WriteLine(parser.GetCommandHelpText("deploy"));
}
```

### Example 2: Generating Help Text
This example shows how to retrieve global help information and specific command details when invalid input is provided or when the user requests help.

```csharp
var parser = new CommandParser();
parser.RegisterCommand("notify", "Send a deployment notification", 
    new List<ParameterDefinition> { new ParameterDefinition("message", "Notification content") }, 
    new List<OptionDefinition> { new OptionDefinition("channel", "ch", "Target channel", true) });

parser.RegisterCommand("status", "Check deployment status", 
    new List<ParameterDefinition>(), 
    new List<OptionDefinition> { new OptionDefinition("json", "j", "Output as JSON", false) });

// Display global help
Console.WriteLine(parser.GetHelpText());

// Display specific command help
Console.WriteLine(parser.GetCommandHelpText("notify"));

// Handling a help flag manually if not auto-resolved
var args = new[] { "notify", "--help" };
parser.Parse(args);

if (!parser.Success && parser.Error?.Contains("help") == true)
{
    Console.WriteLine(parser.GetCommandHelpText("notify"));
}
```

## Notes

*   **Statefulness**: The `CommandParser` instance maintains state between calls to `Parse`. Properties such as `Success`, `Error`, `Parameters`, and `Options` reflect the result of the most recent `Parse` invocation. Calling `Parse` overwrites these values.
*   **Nullability**: Properties returning strings (`Error`, `Output`, `GetParameter`, `GetOption`) may return `null`. Consumers must handle null checks appropriately, particularly when `Success` is `false`.
*   **Thread Safety**: The `CommandParser` class is not thread-safe. Registration of commands (`RegisterCommand`) should be completed during application startup before any parsing occurs. Concurrent calls to `Parse` on the same instance may result in race conditions regarding the internal state properties (`Parameters`, `Options`, `Success`, etc.).
*   **Duplicate Definitions**: Attempting to register a command with a `Name` that already exists in the parser's internal registry will likely result in an exception or undefined behavior, depending on the underlying implementation of the registration logic.
*   **Parameter vs. Option**: `Parameters` are typically positional and required, whereas `Options` are named (often prefixed with `--` or `-`) and optional. The `GetParameter` and `GetOption` methods access distinct internal dictionaries populated during parsing.
