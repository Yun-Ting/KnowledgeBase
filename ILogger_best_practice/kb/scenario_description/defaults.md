# Defaults

## String interpolation using the $ symbol

String interpolation in C# is a feature that allows for the inclusion of variable values directly within a string literal. This is achieved using the $ symbol followed by a string enclosed in double quotes. Inside this string, expressions can be embedded within curly braces {}, and these expressions are evaluated and their values are included in the resulting string.

### Syntax

The basic syntax of string interpolation using the $ symbol is as follows:

```csharp
string interpolatedString = $"This is a string with an embedded value: {expression}";
```

### Examples

#### Basic Example

```csharp
int age = 25;
string name = "Alice";
string message = $"My name is {name} and I am {age} years old.";
Console.WriteLine(message);
// Output: My name is Alice and I am 25 years old.
```

#### Expressions within Interpolations

```csharp
int a = 5;
int b = 10;
string result = $"The sum of {a} and {b} is {a + b}.";
Console.WriteLine(result);
// Output: The sum of 5 and 10 is 15.
```

#### Calling Methods and Properties

```csharp
DateTime now = DateTime.Now;
string formattedDate = $"Today's date is {now:MMMM dd, yyyy}.";
Console.WriteLine(formattedDate);
// Output: Today's date is (current date in "MMMM dd, yyyy" format).
```

#### Using with Object Properties

```csharp
var person = new { Name = "John", Age = 30 };
string introduction = $"Hello, my name is {person.Name} and I am {person.Age} years old.";
Console.WriteLine(introduction);
// Output: Hello, my name is John and I am 30 years old.
```

## Compile-time logging source generation

.NET 6 introduces the LoggerMessageAttribute type. This attribute is part of the `Microsoft.Extensions.Logging` namespace, and when used, it source-generates performant logging APIs.
The source-generation logging support is designed to deliver a highly usable and highly performant logging solution for modern .NET applications. The auto-generated source code relies on the `Microsoft.Extensions.Logging.ILogger` interface.

The source generator is triggered when LoggerMessageAttribute is used on `partial` logging methods. When triggered, it is either able to autogenerate the implementation of the partial methods it's decorating, or produce compile-time diagnostics with hints about proper usage. The compile-time logging solution is typically considerably faster at run time than existing logging approaches. It achieves this by eliminating boxing, temporary allocations, and copies to the maximum extent possible.

## Basic usage

To use the `LoggerMessageAttribute`, the consuming class and method need to be `partial`. The code generator is triggered at compile time, and generates an implementation of the `partial` method.

```csharp
public static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Could not open socket to `{HostName}`")]
    public static partial void CouldNotOpenSocket(
        ILogger logger, string hostName);
}
```

In the preceding example, the logging method is `static` and the log level is specified in the attribute definition. When using the attribute in a static context, either the `ILogger` instance is required as a parameter, or modify the definition to use the `this` keyword to define the method as an extension method.

```csharp
public static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Could not open socket to `{HostName}`")]
    public static partial void CouldNotOpenSocket(
        this ILogger logger, string hostName);
}
```

You may choose to use the attribute in a non-static context as well. Consider the following example where the logging method is declared as an instance method. In this context, the logging method gets the logger by accessing an `ILogger` field in the containing class.

```csharp
public partial class InstanceLoggingExample
{
    private readonly ILogger _logger;

    public InstanceLoggingExample(ILogger logger)
    {
        _logger = logger;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Could not open socket to `{HostName}`")]
    public partial void CouldNotOpenSocket(string hostName);
}
```

Sometimes, the log level needs to be dynamic rather than statically built into the code. You can do this by omitting the log level from the attribute and instead requiring it as a parameter to the logging method.

```csharp
public static partial class Log
{
    [LoggerMessage(
        Message = "Could not open socket to `{HostName}`")]
    public static partial void CouldNotOpenSocket(
        ILogger logger,
        LogLevel level, /* Dynamic log level as parameter, rather than defined in attribute. */
        string hostName);
}
```

You can omit the logging message and `System.String.Empty` will be provided for the message. The state will contain the arguments, formatted as key-value pairs.

Consider the example logging output when using the `JsonConsole` formatter.

```json
{
  "LogLevel": "Information",
  "Category": "\u003CProgram\u003EF...9CB42__SampleObject",
  "Message": "Liana lives in Seattle.",
  "State": {
    "Message": "Liana lives in Seattle.",
    "name": "Liana",
    "city": "Seattle",
    "{OriginalFormat}": "{Name} lives in {City}."
  }
}
```

## Log method anatomy

The `Microsoft.Extensions.Logging.ILogger.Log` signature accepts the `Microsoft.Extensions.Logging.LogLevel` and optionally an `System.Exception`, as shown below.

```csharp
public interface ILogger
{
    void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        TState state,
        System.Exception? exception,
        Func<TState, System.Exception?, string> formatter);
}
```

As a general rule, the first instance of `ILogger`, `LogLevel`, and `Exception` are treated specially in the log method signature of the source generator. Subsequent instances are treated like normal parameters to the message template:

```csharp
// This is a valid attribute usage
[LoggerMessage(
    Level = LogLevel.Debug, Message = "M1 {Ex3} {Ex2}")]
public static partial void ValidLogMethod(
    ILogger logger,
    Exception ex,
    Exception ex2,
    Exception ex3);

// This causes a warning
[LoggerMessage(
    Level = LogLevel.Debug, Message = "M1 {Ex} {Ex2}")]
public static partial void WarningLogMethod(
    ILogger logger,
    Exception ex,
    Exception ex2);
```

> [!IMPORTANT]
> The warnings emitted provide details as to the correct usage of the `LoggerMessageAttribute`. In the preceding example, the `WarningLogMethod` will report a `DiagnosticSeverity.Warning` of `SYSLIB0025`.
>
> ```console
> Don't include a template for `ex` in the logging message since it is implicitly taken care of.
> ```
