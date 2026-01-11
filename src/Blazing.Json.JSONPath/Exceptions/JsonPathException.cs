namespace Blazing.Json.JSONPath.Exceptions;

/// <summary>
/// Base exception for JSONPath-related errors.
/// </summary>
public class JsonPathException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathException"/> class.
    /// </summary>
    public JsonPathException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public JsonPathException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public JsonPathException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
