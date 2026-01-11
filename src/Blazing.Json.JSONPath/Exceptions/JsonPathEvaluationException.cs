namespace Blazing.Json.JSONPath.Exceptions;

/// <summary>
/// Exception thrown when a JSONPath query evaluation fails.
/// This includes errors during query execution, function calls, or filter evaluation.
/// </summary>
public sealed class JsonPathEvaluationException : JsonPathException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathEvaluationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public JsonPathEvaluationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathEvaluationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public JsonPathEvaluationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
