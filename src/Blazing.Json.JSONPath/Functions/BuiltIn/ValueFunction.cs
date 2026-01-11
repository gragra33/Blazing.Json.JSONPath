using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions.BuiltIn;

/// <summary>
/// Implements the value() function per RFC 9535 Section 2.4.8.
/// Converts a nodelist to a value:
/// - If the nodelist contains exactly one node, returns that node's value
/// - Otherwise, returns Nothing
/// </summary>
/// <remarks>
/// This function is used to extract a singular value from a nodelist,
/// which is required for certain operations like comparisons.
/// </remarks>
public sealed class ValueFunction : IFunctionExtension
{
    /// <inheritdoc/>
    public string Name => "value";

    /// <inheritdoc/>
    public FunctionType ResultType => FunctionType.ValueType;

    /// <inheritdoc/>
    public IReadOnlyList<FunctionType> ParameterTypes { get; } =
        new[] { FunctionType.NodesType };

    /// <inheritdoc/>
    public FunctionResult Execute(IReadOnlyList<FunctionArgument> arguments, EvaluationContext context)
    {
        if (arguments[0] is not NodesArgument nodesArg)
        {
            throw new JsonPathEvaluationException($"Function '{Name}' expects a NodesType argument.");
        }

        var nodelist = nodesArg.Nodes;

        // Singular nodelist: return the value
        if (nodelist.Count == 1)
        {
            return FunctionResult.FromValue(nodelist[0].Value);
        }

        // Empty or multiple nodes: return Nothing
        return FunctionResult.Nothing;
    }
}
