namespace Blazing.Json.JSONPath.Exceptions;

/// <summary>
/// Exception thrown when a JSONPath query has invalid syntax.
/// </summary>
public class JsonPathSyntaxException : JsonPathException
{
    /// <summary>
    /// Gets the position in the query where the syntax error occurred.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathSyntaxException"/> class.
    /// </summary>
    public JsonPathSyntaxException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathSyntaxException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JsonPathSyntaxException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathSyntaxException"/> class with a specified error message
    /// and position.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="position">The position in the query where the error occurred.</param>
    public JsonPathSyntaxException(string message, int position)
        : base($"{message} at position {position}")
    {
        Position = position;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathSyntaxException"/> class with a specified error message
    /// and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public JsonPathSyntaxException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
