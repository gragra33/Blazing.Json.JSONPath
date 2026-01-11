using System.Text.Json;
using System.Text.RegularExpressions;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Functions.BuiltIn;

/// <summary>
/// Implements the search() function per RFC 9535 Section 2.4.7.
/// Searches for a regular expression pattern within a string.
/// Returns LogicalTrue if the pattern is found anywhere in the string, LogicalFalse otherwise.
/// </summary>
/// <remarks>
/// The difference between match() and search():
/// - match(): Tests if the entire string matches the pattern
/// - search(): Tests if the pattern is found anywhere in the string (substring match)
/// </remarks>
public sealed class SearchFunction : IFunctionExtension
{
    /// <inheritdoc/>
    public string Name => "search";

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
            // RFC 9535: Uses I-Regexp (RFC 9485)
            // search() looks for the pattern anywhere in the string (substring match)
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            var isMatch = regex.IsMatch(str);

            return FunctionResult.FromLogical(isMatch);
        }
        catch (RegexMatchTimeoutException)
        {
            throw new JsonPathEvaluationException($"Regex pattern '{pattern}' timed out during search.");
        }
        catch (ArgumentException ex)
        {
            throw new JsonPathEvaluationException($"Invalid regex pattern '{pattern}': {ex.Message}", ex);
        }
    }
}
