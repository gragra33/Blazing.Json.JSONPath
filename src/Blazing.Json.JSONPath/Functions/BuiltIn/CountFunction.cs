using System.Text.Json;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions.BuiltIn;

/// <summary>
/// Implements the count() function per RFC 9535 Section 2.4.5.
/// Returns the number of nodes in a nodelist.
/// </summary>
public sealed class CountFunction : IFunctionExtension
{
    /// <inheritdoc/>
    public string Name => "count";

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

        var count = nodesArg.Nodes.Count;

        // Return the count as a JSON number
        return FunctionResult.FromValue(JsonDocument.Parse(count.ToString()).RootElement);
    }
}
