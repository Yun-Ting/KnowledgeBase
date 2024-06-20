1. Identify all the `ILogger` API logger calls that are NOT using compile-time-source generation from the input.
        
2. For each identified call, do the following:
    - First, define a static partial class which has the same name as the class name with the word "LoggingExtensions" as its suffix.
    - Second, inside the class created at the first step, define the extension methods, with the `this` keyword for the logger as the first parameter. 
    - Third, use `LoggerMessageAttribute` on each static method to define the log level, and message template.
    - Fourth, do not pass in an explicit EventId while using LoggerMessageAttribute, because durable EventId will be automatically assigned based on the hash of the method name during code generation.
    - Fifth, for each interpolated parameter in the original `ILogger` API call, matching the original call for proper structured logging.
        
3. Once all logger calls have been refactored, the helper class will contain several such static logger methods, each attributed with LoggerMessageAttribute, corresponding to the log messages that were not using compile-time source generation. Replace the original logger call with an invocation of the new static logger extension method, passing in the necessary arguments.