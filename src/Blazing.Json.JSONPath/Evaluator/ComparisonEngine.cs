using System.Text.Json;
using Blazing.Json.JSONPath.Parser.Nodes;

namespace Blazing.Json.JSONPath.Evaluator;

/// <summary>
/// Implements RFC 9535 Section 2.3.5.2.2 comparison semantics (Table 11).
/// Handles comparison operations between comparable values with proper type checking.
/// </summary>
public static class ComparisonEngine
{
    /// <summary>
    /// Compares two comparable values according to RFC 9535 rules.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>True if the comparison holds, false otherwise.</returns>
    public static bool Compare(ComparableValue left, ComparisonOperator @operator, ComparableValue right)
    {
        // Handle Nothing cases (RFC Table 11)
        if (left.IsNothing || right.IsNothing)
        {
            return @operator switch
            {
                ComparisonOperator.Equal => left.IsNothing && right.IsNothing,
                ComparisonOperator.NotEqual => !(left.IsNothing && right.IsNothing),
                ComparisonOperator.Less => false,
                ComparisonOperator.LessEqual => left.IsNothing && right.IsNothing,
                ComparisonOperator.Greater => false,
                ComparisonOperator.GreaterEqual => left.IsNothing && right.IsNothing,
                _ => false
            };
        }

        // Both values are present
        var leftElement = left.Value!.Value;
        var rightElement = right.Value!.Value;

        // Type-specific comparison
        return (leftElement.ValueKind, rightElement.ValueKind) switch
        {
            // Numbers (including integers and doubles)
            (JsonValueKind.Number, JsonValueKind.Number) =>
                CompareNumbers(leftElement, @operator, rightElement),

            // Strings
            (JsonValueKind.String, JsonValueKind.String) =>
                CompareStrings(leftElement, @operator, rightElement),

            // Booleans
            (JsonValueKind.True or JsonValueKind.False, JsonValueKind.True or JsonValueKind.False) =>
                CompareBooleans(leftElement, @operator, rightElement),

            // Nulls
            (JsonValueKind.Null, JsonValueKind.Null) =>
                @operator is ComparisonOperator.Equal or ComparisonOperator.LessEqual or ComparisonOperator.GreaterEqual,

            // Type mismatch - only != can be true
            _ => @operator == ComparisonOperator.NotEqual
        };
    }

    /// <summary>
    /// Compares two numeric JSON values.
    /// </summary>
    private static bool CompareNumbers(JsonElement left, ComparisonOperator @operator, JsonElement right)
    {
        // Get numeric values as double for comparison
        var leftNum = left.GetDouble();
        var rightNum = right.GetDouble();

        // Calculate equal and less (RFC derives all operators from these two)
        var equal = Math.Abs(leftNum - rightNum) < double.Epsilon;
        var less = leftNum < rightNum;

        return DeriveFromEqualAndLess(@operator, equal, less);
    }

    /// <summary>
    /// Compares two string JSON values using Unicode scalar value comparison.
    /// </summary>
    private static bool CompareStrings(JsonElement left, ComparisonOperator @operator, JsonElement right)
    {
        var leftStr = left.GetString()!;
        var rightStr = right.GetString()!;

        // RFC: Unicode scalar value comparison (ordinal)
        var comparison = string.CompareOrdinal(leftStr, rightStr);
        var equal = comparison == 0;
        var less = comparison < 0;

        return DeriveFromEqualAndLess(@operator, equal, less);
    }

    /// <summary>
    /// Compares two boolean JSON values.
    /// </summary>
    private static bool CompareBooleans(JsonElement left, ComparisonOperator @operator, JsonElement right)
    {
        var leftBool = left.GetBoolean();
        var rightBool = right.GetBoolean();

        var equal = leftBool == rightBool;
        var less = !leftBool && rightBool; // false < true

        return DeriveFromEqualAndLess(@operator, equal, less);
    }

    /// <summary>
    /// Derives all comparison operators from == and &lt; (RFC Section 2.3.5.2.2).
    /// This ensures consistent comparison semantics across all types.
    /// </summary>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="equal">Whether the values are equal.</param>
    /// <param name="less">Whether the left value is less than the right.</param>
    /// <returns>The result of the comparison.</returns>
    private static bool DeriveFromEqualAndLess(ComparisonOperator @operator, bool equal, bool less)
    {
        return @operator switch
        {
            ComparisonOperator.Equal => equal,
            ComparisonOperator.NotEqual => !equal,
            ComparisonOperator.Less => less,
            ComparisonOperator.LessEqual => less || equal,
            ComparisonOperator.Greater => !less && !equal,
            ComparisonOperator.GreaterEqual => !less,
            _ => throw new NotSupportedException($"Unknown comparison operator: {@operator}")
        };
    }
}
