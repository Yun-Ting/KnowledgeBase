# .NET ILogger logging with string interpolation using the $ symbol to compile-time logging source generation migration guide

This article shows how to migrate .NET `ILogger` logging API calls from using `string interpolation using the $ symbol` to `compile-time logging source generation`.

1.Identify the .NET `public interface ILogger` logging API calls that are using `string interpolation using the $ symbol` to do logging.

Note: see the `String interpolation using the $ symbol` part to understand the patterns.

2.For each identified call, migrate the code with following steps:

- First: define partial methods for logging in your `Class`, using the LoggerMessage attribute. The attribute includes properties for `LogLevel`, and `Message`.
- Second: modify the `Class` to use the generated logging methods. Replace the existing logging calls with calls to the generated partial methods. These methods are defined as `static partial` and will be implemented by the source generator at compile time.
- Third: extract the parameters in the pattern of `string interpolation using the $ symbols` . Then, update the the parameters of `LoggerMessageAttribute` to match the original parameters when doing code migration. Do not pass in an explicit `EventId` while using `LoggerMessageAttribute`.

3.Once all `ILogger` logging API calls have been refactored, the helper class will contain several such static extension methods, each attributed with `LoggerMessageAttribute`.

See `Compile-time logging source generation` to learn more about how to migrate.

## ILogger interface

The .NET ILogger interface is a part of the `Microsoft.Extensions.Logging` namespace and is used for logging within .NET applications. It provides a standardized way to log messages, exceptions, and other information in a structured manner, which can then be processed by various logging providers like console, file, and third-party logging services.

## Example

Assuming the following variables were initialized:

```csharp
static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); });
static readonly ILogger MyLogger = MyLoggerFactory.CreateLogger<Program>();
static readonly string FoodName = "tomato";
const double FoodPrice = 2.99;
```

- Before (using string interpolation):

```csharp
MyLogger.LogInformation($"Hello from {FoodName} {FoodPrice}.");
```

- After (using compile-time logging source generation):

```csharp
MyLogger.SayHello(FoodName, FoodPrice);
internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Hello from {food} {price}.")]
    public static partial void SayHello(this ILogger logger, string food, double price);
}
```
