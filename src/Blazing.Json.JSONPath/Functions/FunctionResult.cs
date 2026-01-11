using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;

namespace Blazing.Json.JSONPath.Functions;

/// <summary>
/// Represents the result of a function call.
/// Can contain a value, a nodelist, or a logical value depending on the function's return type.
/// </summary>
public readonly struct FunctionResult
{
    /// <summary>
    /// Gets the type of this result.
    /// </summary>
    public FunctionType Type { get; init; }

    /// <summary>
    /// Gets the JSON value for ValueType results, or null for Nothing.
    /// </summary>
    public JsonElement? Value { get; init; }

    /// <summary>
    /// Gets the nodelist for NodesType results.
    /// </summary>
    public Nodelist? Nodes { get; init; }

    /// <summary>
    /// Gets the logical value for LogicalType results.
    /// </summary>
    public bool? Logical { get; init; }

    /// <summary>
    /// Gets a value indicating whether this result represents Nothing (ValueType with no value).
    /// </summary>
    public bool IsNothing => Type == FunctionType.ValueType && Value is null;

    /// <summary>
    /// Creates a ValueType result with a JSON value.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    /// <returns>A FunctionResult containing the value.</returns>
    public static FunctionResult FromValue(JsonElement value) =>
        new() { Type = FunctionType.ValueType, Value = value };

    /// <summary>
    /// Creates a NodesType result with a nodelist.
    /// </summary>
    /// <param name="nodes">The nodelist.</param>
    /// <returns>A FunctionResult containing the nodelist.</returns>
    public static FunctionResult FromNodes(Nodelist nodes) =>
        new() { Type = FunctionType.NodesType, Nodes = nodes };

    /// <summary>
    /// Creates a LogicalType result with a boolean value.
    /// </summary>
    /// <param name="value">The logical value (true or false).</param>
    /// <returns>A FunctionResult containing the logical value.</returns>
    public static FunctionResult FromLogical(bool value) =>
        new() { Type = FunctionType.LogicalType, Logical = value };

    /// <summary>
    /// Creates a Nothing result (ValueType with no value).
    /// </summary>
    /// <returns>A FunctionResult representing Nothing.</returns>
    public static FunctionResult Nothing =>
        new() { Type = FunctionType.ValueType, Value = null };

    /// <summary>
    /// Converts this result to a logical value.
    /// For NodesType: empty nodelist ? LogicalFalse, non-empty ? LogicalTrue (RFC Section 2.4.2).
    /// For LogicalType: returns the logical value directly.
    /// For ValueType: Nothing ? LogicalFalse, any value ? LogicalTrue.
    /// </summary>
    /// <returns>The logical value.</returns>
    public bool ToLogical()
    {
        return Type switch
        {
            FunctionType.LogicalType => Logical!.Value,
            FunctionType.NodesType => Nodes!.ToLogical(),
            FunctionType.ValueType => Value.HasValue,
            _ => false
        };
    }

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    public override string ToString()
    {
        return Type switch
        {
            FunctionType.ValueType => Value.HasValue ? $"Value({Value.Value})" : "Nothing",
            FunctionType.NodesType => $"Nodes({Nodes!.Count} items)",
            FunctionType.LogicalType => $"Logical({Logical!.Value})",
            _ => "Unknown"
        };
    }
}
