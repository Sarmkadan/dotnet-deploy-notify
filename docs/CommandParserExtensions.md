# CommandParserExtensions

The `CommandParserExtensions` static class provides a set of utility methods for inspecting and extracting typed values from a `ParsedCommand` object. These methods simplify common tasks such as checking for the presence of parameters and options, retrieving their names, and converting values to `int` or `bool`. The class is designed to be used after a command string has been parsed by `TryParse`, enabling clean and readable access to the parsed command structure.

## API

### `TryParse`
```csharp
public static ParsedCommand TryParse(string[] args)
```
Parses an array of command-line arguments into a `ParsedCommand` instance.  
- **Parameters**: `args` – the raw argument strings, typically from `Environment.GetCommandLineArgs()`.  
- **Returns**: A `ParsedCommand` object representing the parsed command, or `null` if parsing fails (e.g., invalid syntax).  
- **Throws**: `ArgumentNullException` if `args` is `null`.

### `GetParameterNames`
```csharp
public static IReadOnlyList<string> GetParameterNames(this ParsedCommand command)
```
Returns the names of all positional parameters present in the parsed command.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: A read-only list of parameter name strings. The list is empty if no parameters exist.  
- **Throws**: `ArgumentNullException` if `command` is `null`.

### `GetOptionNames`
```csharp
public static IReadOnlyList<string> GetOptionNames(this ParsedCommand command)
```
Returns the names of all named options (flags or key-value options) present in the parsed command.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: A read-only list of option name strings. The list is empty if no options exist.  
- **Throws**: `ArgumentNullException` if `command` is `null`.

### `GetParameterAsInt`
```csharp
public static int? GetParameterAsInt(this ParsedCommand command, string parameterName)
```
Attempts to retrieve the value of a positional parameter as an integer.  
- **Parameters**:  
  - `command` – a non-null `ParsedCommand` instance.  
  - `parameterName` – the name of the parameter to query.  
- **Returns**: The integer value if the parameter exists and its string representation can be parsed as an `int`; otherwise `null`.  
- **Throws**: `ArgumentNullException` if `command` or `parameterName` is `null`.

### `GetOptionAsBoolean`
```csharp
public static bool GetOptionAsBoolean(this ParsedCommand command, string optionName)
```
Returns the boolean value of a named option.  
- **Parameters**:  
  - `command` – a non-null `ParsedCommand` instance.  
  - `optionName` – the name of the option to query.  
- **Returns**: `true` if the option is present and its value (or presence) indicates truth; `false` otherwise. The exact semantics depend on the parser implementation (e.g., a flag option without a value is typically `true` when present).  
- **Throws**: `ArgumentNullException` if `command` or `optionName` is `null`.

### `GetOptionAsInt`
```csharp
public static int? GetOptionAsInt(this ParsedCommand command, string optionName)
```
Attempts to retrieve the value of a named option as an integer.  
- **Parameters**:  
  - `command` – a non-null `ParsedCommand` instance.  
  - `optionName` – the name of the option to query.  
- **Returns**: The integer value if the option exists and its value can be parsed as an `int`; otherwise `null`.  
- **Throws**: `ArgumentNullException` if `command` or `optionName` is `null`.

### `HasParameters`
```csharp
public static bool HasParameters(this ParsedCommand command)
```
Indicates whether the parsed command contains any positional parameters.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: `true` if at least one parameter exists; otherwise `false`.  
- **Throws**: `ArgumentNullException` if `command` is `null`.

### `HasOptions`
```csharp
public static bool HasOptions(this ParsedCommand command)
```
Indicates whether the parsed command contains any named options.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: `true` if at least one option exists; otherwise `false`.  
- **Throws**: `ArgumentNullException` if `command` is `null`.

### `GetParameterCount`
```csharp
public static int GetParameterCount(this ParsedCommand command)
```
Returns the number of positional parameters in the parsed command.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: The count of parameters (zero or more).  
- **Throws**: `ArgumentNullException` if `command` is `null`.

### `GetOptionCount`
```csharp
public static int GetOptionCount(this ParsedCommand command)
```
Returns the number of named options in the parsed command.  
- **Parameters**: `command` – a non-null `ParsedCommand` instance.  
- **Returns**: The count of options (zero or more).  
- **Throws**: `ArgumentNullException` if `command` is `null`.

## Usage

### Example 1: Parsing a deployment command with parameters and options

```csharp
string[] args = { "deploy", "--environment", "staging", "--dry-run", "app-name" };
ParsedCommand command = CommandParserExtensions.TryParse(args);
if (command == null)
{
    Console.Error.WriteLine("Invalid command syntax.");
    return;
}

if (CommandParserExtensions.HasParameters(command))
{
    string appName = CommandParserExtensions.GetParameterNames(command)[0];
    Console.WriteLine($"Deploying application: {appName}");
}

if (CommandParserExtensions.GetOptionAsBoolean(command, "dry-run"))
{
    Console.WriteLine("Dry-run mode enabled.");
}

int? environmentId = CommandParserExtensions.GetOptionAsInt(command, "environment");
if (environmentId.HasValue)
{
    Console.WriteLine($"Environment ID: {environmentId.Value}");
}
```

### Example 2: Validating required parameters and options

```csharp
string[] args = { "notify", "--channel", "slack", "--timeout", "30" };
ParsedCommand cmd = CommandParserExtensions.TryParse(args);
if (cmd == null) throw new InvalidOperationException("Invalid arguments.");

if (CommandParserExtensions.GetParameterCount(cmd) < 1)
{
    Console.Error.WriteLine("Missing required parameter: message.");
    return;
}

if (!CommandParserExtensions.HasOptions(cmd) ||
    !CommandParserExtensions.GetOptionNames(cmd).Contains("channel"))
{
    Console.Error.WriteLine("Option --channel is required.");
    return;
}

int? timeout = CommandParserExtensions.GetOptionAsInt(cmd, "timeout");
int effectiveTimeout = timeout ?? 10; // default 10 seconds
Console.WriteLine($"Sending notification with timeout {effectiveTimeout}s.");
```

## Notes

- **Null handling**: All methods that accept a `ParsedCommand` instance throw `ArgumentNullException` when that argument is `null`. Always validate the result of `TryParse` before calling other extension methods.
- **Type conversion**: `GetParameterAsInt` and `GetOptionAsInt` return `null` when the value cannot be parsed as an integer (e.g., non-numeric strings, missing values). They do not throw on parse failure.
- **Boolean options**: `GetOptionAsBoolean` returns `false` if the option is absent. For flag-style options (no value), the method typically returns `true` when the option is present; check the parser’s documentation for exact behavior.
- **Thread safety**: `ParsedCommand` instances are immutable after creation. All extension methods are read-only and do not modify the underlying object. Therefore, concurrent calls on the same `ParsedCommand` instance are safe.
- **Empty results**: `GetParameterNames` and `GetOptionNames` return an empty list (not `null`) when no parameters or options exist. `HasParameters` and `HasOptions` are the recommended guards before accessing list elements.
- **Case sensitivity**: Parameter and option name comparisons are case-sensitive by default. Ensure the names passed to `GetParameterAsInt`, `GetOptionAsBoolean`, and `GetOptionAsInt` match the casing used in the original command.
