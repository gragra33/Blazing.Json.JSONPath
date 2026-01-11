namespace Blazing.Json.JSONPath.Analysis;

/// <summary>
/// Represents the complexity level of a JSONPath query for routing purposes.
/// </summary>
public enum JsonPathComplexity
{
    /// <summary>
    /// Simple path that can use fast-path execution (e.g., $.data[*], $.result[*].customer).
    /// </summary>
    Simple,

    /// <summary>
    /// Complex path requiring RFC 9535 parser (e.g., $..author, filters, slices, functions).
    /// </summary>
    Complex
}
