using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions;

/// <summary>
/// Provides context for function evaluation.
/// Contains references to the current node and root node for query evaluation.
/// </summary>
public readonly struct EvaluationContext
{
    /// <summary>
    /// Gets the current node (@ in queries).
    /// </summary>
    public JsonElement CurrentNode { get; init; }

    /// <summary>
    /// Gets the root node ($ in queries).
    /// </summary>
    public JsonElement RootNode { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationContext"/> struct.
    /// </summary>
    /// <param name="currentNode">The current node.</param>
    /// <param name="rootNode">The root node.</param>
    public EvaluationContext(JsonElement currentNode, JsonElement rootNode)
    {
        CurrentNode = currentNode;
        RootNode = rootNode;
    }
}

/// <summary>
/// Defines the interface for JSONPath function extensions.
/// Functions can be built-in (defined by RFC 9535) or custom extensions.
/// </summary>
public interface IFunctionExtension
{
    /// <summary>
    /// Gets the name of the function.
    /// Must match the pattern: [a-z][a-z0-9_]*
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the return type of this function.
    /// </summary>
    FunctionType ResultType { get; }

    /// <summary>
    /// Gets the parameter types this function expects.
    /// The number and types of parameters define the function's signature.
    /// </summary>
    IReadOnlyList<FunctionType> ParameterTypes { get; }

    /// <summary>
    /// Executes the function with the provided arguments.
    /// </summary>
    /// <param name="arguments">The arguments to the function (already type-checked).</param>
    /// <param name="context">The evaluation context providing access to current and root nodes.</param>
    /// <returns>The result of the function execution.</returns>
    /// <exception cref="Exceptions.JsonPathEvaluationException">Thrown when the function execution fails.</exception>
    FunctionResult Execute(IReadOnlyList<FunctionArgument> arguments, EvaluationContext context);
}

/// <summary>
/// Base class for function arguments.
/// Arguments can be values, nodelists, or queries.
/// </summary>
public abstract class FunctionArgument
{
    /// <summary>
    /// Gets the type of this argument.
    /// </summary>
    public abstract FunctionType Type { get; }
}

/// <summary>
/// Represents a value argument to a function.
/// </summary>
public sealed class ValueArgument : FunctionArgument
{
    /// <summary>
    /// Gets the JSON value, or null for Nothing.
    /// </summary>
    public JsonElement? Value { get; init; }

    /// <summary>
    /// Gets a value indicating whether this represents Nothing.
    /// </summary>
    public bool IsNothing => Value is null;

    /// <inheritdoc/>
    public override FunctionType Type => FunctionType.ValueType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueArgument"/> class.
    /// </summary>
    /// <param name="value">The JSON value.</param>
    public ValueArgument(JsonElement? value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Nothing value argument.
    /// </summary>
    public static ValueArgument Nothing => new(null);
}

/// <summary>
/// Represents a nodelist argument to a function.
/// </summary>
public sealed class NodesArgument : FunctionArgument
{
    /// <summary>
    /// Gets the nodelist.
    /// </summary>
    public Nodelist Nodes { get; init; }

    /// <inheritdoc/>
    public override FunctionType Type => FunctionType.NodesType;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodesArgument"/> class.
    /// </summary>
    /// <param name="nodes">The nodelist.</param>
    public NodesArgument(Nodelist nodes)
    {
        Nodes = nodes;
    }
}

/// <summary>
/// Represents a logical argument to a function.
/// </summary>
public sealed class LogicalArgument : FunctionArgument
{
    /// <summary>
    /// Gets the logical value.
    /// </summary>
    public bool Value { get; init; }

    /// <inheritdoc/>
    public override FunctionType Type => FunctionType.LogicalType;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogicalArgument"/> class.
    /// </summary>
    /// <param name="value">The logical value.</param>
    public LogicalArgument(bool value)
    {
        Value = value;
    }
}
