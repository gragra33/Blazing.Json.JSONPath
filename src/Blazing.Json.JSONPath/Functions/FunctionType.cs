namespace Blazing.Json.JSONPath.Functions;

/// <summary>
/// Defines the RFC 9535 type system for function expressions.
/// Functions can accept and return values of these types.
/// </summary>
/// <remarks>
/// RFC 9535 Section 2.4 defines three type domains:
/// - ValueType: JSON values or the special value Nothing
/// - NodesType: Nodelists (results of queries)
/// - LogicalType: LogicalTrue or LogicalFalse (boolean values)
/// </remarks>
public enum FunctionType
{
    /// <summary>
    /// Represents JSON values or the special value Nothing.
    /// This includes strings, numbers, booleans, null, objects, and arrays.
    /// </summary>
    ValueType,

    /// <summary>
    /// Represents nodelists (results of queries).
    /// A nodelist is a sequence of nodes, each consisting of a value and its location.
    /// </summary>
    NodesType,

    /// <summary>
    /// Represents logical values (LogicalTrue or LogicalFalse).
    /// Used for boolean operations and filter expressions.
    /// </summary>
    LogicalType
}
