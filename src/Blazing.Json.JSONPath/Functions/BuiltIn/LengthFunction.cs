using System.Text.Json;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions.BuiltIn;

/// <summary>
/// Implements the length() function per RFC 9535 Section 2.4.4.
/// Returns the length of a value:
/// - For strings: number of Unicode scalar values
/// - For arrays: number of elements
/// - For objects: number of members
/// - For other types: returns Nothing
/// </summary>
public sealed class LengthFunction : IFunctionExtension
{
    /// <inheritdoc/>
    public string Name => "length";

    /// <inheritdoc/>
    public FunctionType ResultType => FunctionType.ValueType;

    /// <inheritdoc/>
    public IReadOnlyList<FunctionType> ParameterTypes { get; } =
        new[] { FunctionType.ValueType };

    /// <inheritdoc/>
    public FunctionResult Execute(IReadOnlyList<FunctionArgument> arguments, EvaluationContext context)
    {
        // Handle both ValueArgument and NodesArgument (with value() conversion)
        JsonElement? value = arguments[0] switch
        {
            ValueArgument valueArg => valueArg.IsNothing ? null : valueArg.Value!.Value,
            NodesArgument nodesArg => ConvertNodesToValue(nodesArg),
            _ => throw new JsonPathEvaluationException($"Function '{Name}' expects a ValueType argument.")
        };

        // Nothing input produces Nothing output
        if (value is null)
        {
            return FunctionResult.Nothing;
        }

        return value.Value.ValueKind switch
        {
            // String: count Unicode scalar values
            JsonValueKind.String => CountStringLength(value.Value),

            // Array: count elements
            JsonValueKind.Array =>
                FunctionResult.FromValue(JsonDocument.Parse(value.Value.GetArrayLength().ToString()).RootElement),

            // Object: count members
            JsonValueKind.Object =>
                FunctionResult.FromValue(JsonDocument.Parse(CountObjectMembers(value.Value).ToString()).RootElement),

            // Other types: Nothing
            _ => FunctionResult.Nothing
        };
    }

    /// <summary>
    /// Converts a NodesArgument to a single value using value() function semantics.
    /// Returns null if the nodelist is empty or has multiple elements.
    /// </summary>
    private static JsonElement? ConvertNodesToValue(NodesArgument nodesArg)
    {
        if (nodesArg.Nodes.Count == 1)
        {
            return nodesArg.Nodes[0].Value;
        }
        return null; // Empty or multiple nodes = Nothing
    }

    /// <summary>
    /// Counts the number of Unicode scalar values in a string.
    /// </summary>
    private static FunctionResult CountStringLength(JsonElement stringValue)
    {
        var str = stringValue.GetString()!;
        
        // Count Unicode scalar values using Rune enumeration
        // This properly handles all Unicode including emojis and surrogate pairs
        var length = str.EnumerateRunes().Count();

        return FunctionResult.FromValue(JsonDocument.Parse(length.ToString()).RootElement);
    }

    /// <summary>
    /// Counts the number of members in an object.
    /// </summary>
    private static int CountObjectMembers(JsonElement objectValue)
    {
        var count = 0;
        foreach (var _ in objectValue.EnumerateObject())
        {
            count++;
        }
        return count;
    }
}
