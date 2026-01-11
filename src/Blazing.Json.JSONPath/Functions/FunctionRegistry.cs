using Blazing.Json.JSONPath.Exceptions;
using Blazing.Json.JSONPath.Functions.BuiltIn;

namespace Blazing.Json.JSONPath.Functions;

/// <summary>
/// Manages registration and lookup of JSONPath function extensions.
/// Provides built-in RFC 9535 functions and supports custom function registration.
/// </summary>
public sealed class FunctionRegistry
{
    private readonly Dictionary<string, IFunctionExtension> _functions;

    /// <summary>
    /// Gets the singleton instance of the function registry with built-in functions.
    /// </summary>
    public static FunctionRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionRegistry"/> class.
    /// </summary>
    public FunctionRegistry()
    {
        _functions = new Dictionary<string, IFunctionExtension>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates the default registry with all RFC 9535 built-in functions.
    /// </summary>
    private static FunctionRegistry CreateDefault()
    {
        var registry = new FunctionRegistry();

        // Register built-in functions (RFC 9535 Section 2.4)
        registry.Register(new LengthFunction());
        registry.Register(new CountFunction());
        registry.Register(new MatchFunction());
        registry.Register(new SearchFunction());
        registry.Register(new ValueFunction());

        return registry;
    }

    /// <summary>
    /// Registers a function extension.
    /// </summary>
    /// <param name="function">The function to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when function is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a function with the same name is already registered.</exception>
    public void Register(IFunctionExtension function)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (_functions.ContainsKey(function.Name))
        {
            throw new ArgumentException($"Function '{function.Name}' is already registered.", nameof(function));
        }

        _functions[function.Name] = function;
    }

    /// <summary>
    /// Tries to get a function by name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="function">The function, if found.</param>
    /// <returns>True if the function was found, false otherwise.</returns>
    public bool TryGetFunction(string name, out IFunctionExtension? function)
    {
        return _functions.TryGetValue(name, out function);
    }

    /// <summary>
    /// Gets a function by name.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>The function extension.</returns>
    /// <exception cref="JsonPathEvaluationException">Thrown when the function is not found.</exception>
    public IFunctionExtension GetFunction(string name)
    {
        if (!TryGetFunction(name, out var function))
        {
            throw new JsonPathEvaluationException($"Unknown function: {name}");
        }

        return function!;
    }

    /// <summary>
    /// Validates that function arguments match the expected parameter types.
    /// Performs well-typedness checking per RFC 9535.
    /// </summary>
    /// <param name="function">The function being called.</param>
    /// <param name="arguments">The arguments provided.</param>
    /// <exception cref="JsonPathEvaluationException">Thrown when arguments don't match parameter types.</exception>
    public static void ValidateArguments(IFunctionExtension function, IReadOnlyList<FunctionArgument> arguments)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(arguments);

        // Check argument count
        if (arguments.Count != function.ParameterTypes.Count)
        {
            throw new JsonPathEvaluationException(
                $"Function '{function.Name}' expects {function.ParameterTypes.Count} argument(s), " +
                $"but {arguments.Count} were provided.");
        }

        // Check argument types (with allowed conversions)
        for (int i = 0; i < arguments.Count; i++)
        {
            var expected = function.ParameterTypes[i];
            var actual = arguments[i].Type;

            if (!AreTypesCompatible(expected, actual))
            {
                throw new JsonPathEvaluationException(
                    $"Function '{function.Name}' argument {i + 1}: " +
                    $"expected {expected}, but got {actual}.");
            }
        }
    }

    /// <summary>
    /// Checks if an actual type is compatible with an expected type.
    /// Implements RFC 9535 Section 2.4.2 type conversion rules.
    /// </summary>
    private static bool AreTypesCompatible(FunctionType expected, FunctionType actual)
    {
        // Exact match
        if (expected == actual)
            return true;

        // NodesType can be converted to LogicalType (RFC 2.4.2)
        if (expected == FunctionType.LogicalType && actual == FunctionType.NodesType)
            return true;

        // NodesType can be converted to ValueType using value() semantics (RFC 2.4.2)
        // If nodelist has exactly one element, use that element; otherwise Nothing
        if (expected == FunctionType.ValueType && actual == FunctionType.NodesType)
            return true;

        return false;
    }
}
