using System.Text.Json;
using System.Text.RegularExpressions;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions.BuiltIn;

/// <summary>
/// Implements the match() function per RFC 9535 Section 2.4.6.
/// Tests if a string matches a regular expression pattern.
/// Returns LogicalTrue if the string matches, LogicalFalse otherwise.
/// </summary>
public sealed class MatchFunction : IFunctionExtension
{
    /// <inheritdoc/>
    public string Name => "match";

    /// <inheritdoc/>
    public FunctionType ResultType => FunctionType.LogicalType;

    /// <inheritdoc/>
    public IReadOnlyList<FunctionType> ParameterTypes { get; } =
        new[] { FunctionType.ValueType, FunctionType.ValueType };

    /// <inheritdoc/>
    public FunctionResult Execute(IReadOnlyList<FunctionArgument> arguments, EvaluationContext context)
    {
        if (arguments[0] is not ValueArgument stringArg ||
            arguments[1] is not ValueArgument patternArg)
        {
            throw new JsonPathEvaluationException($"Function '{Name}' expects two ValueType arguments.");
        }

        // Nothing input produces LogicalFalse
        if (stringArg.IsNothing || patternArg.IsNothing)
        {
            return FunctionResult.FromLogical(false);
        }

        var stringValue = stringArg.Value!.Value;
        var patternValue = patternArg.Value!.Value;

        // Both must be strings
        if (stringValue.ValueKind != JsonValueKind.String ||
            patternValue.ValueKind != JsonValueKind.String)
        {
            return FunctionResult.FromLogical(false);
        }

        var str = stringValue.GetString()!;
        var pattern = patternValue.GetString()!;

        try
        {
            // RFC 9535: Uses I-Regexp (RFC 9485) - a simplified, interoperable regex subset
            // For .NET implementation, we use standard .NET regex with appropriate options
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            var isMatch = regex.IsMatch(str);

            return FunctionResult.FromLogical(isMatch);
        }
        catch (RegexMatchTimeoutException)
        {
            throw new JsonPathEvaluationException($"Regex pattern '{pattern}' timed out during matching.");
        }
        catch (ArgumentException ex)
        {
            throw new JsonPathEvaluationException($"Invalid regex pattern '{pattern}': {ex.Message}", ex);
        }
    }
}
