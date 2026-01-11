using System.Text.RegularExpressions;

namespace Blazing.Json.JSONPath.Utilities;

/// <summary>
/// Helper methods for JSONPath query analysis.
/// Optimized with source-generated regex and ReadOnlySpan for high performance.
/// </summary>
public static partial class JsonPathHelper
{
    // Source-generated regex patterns for optimal performance
    
    /// <summary>
    /// Matches filter selectors: [?expression]
    /// </summary>
    [GeneratedRegex(@"\[\?", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FilterSelectorPattern();

    /// <summary>
    /// Matches any RFC 9535 built-in function call.
    /// Matches: length(, count(, match(, search(, value(
    /// </summary>
    [GeneratedRegex(@"\b(?:length|count|match|search|value)\s*\(", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FunctionCallPattern();

    /// <summary>
    /// Matches recursive descent operator: ..
    /// </summary>
    [GeneratedRegex(@"\.\.", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex RecursiveDescentPattern();

    /// <summary>
    /// Determines if the JSONPath expression contains RFC 9535 features.
    /// RFC 9535 features include: filter selectors, functions, slice selectors, 
    /// logical operators, and recursive descent.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression to analyze.</param>
    /// <returns>True if the expression uses RFC 9535 features; false otherwise.</returns>
    public static bool HasFeatures(string jsonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        return HasFeatures(jsonPath.AsSpan());
    }

    /// <summary>
    /// Determines if the JSONPath expression contains RFC 9535 features.
    /// RFC 9535 features include: filter selectors, functions, slice selectors, 
    /// logical operators, and recursive descent.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression to analyze.</param>
    /// <returns>True if the expression uses RFC 9535 features; false otherwise.</returns>
    public static bool HasFeatures(ReadOnlySpan<char> jsonPath)
    {
        // RFC 9535 indicators (optimized with early returns):
        // 1. Filter expressions: [?@.field op value] or [?expression]
        // 2. Functions: length(), count(), match(), search(), value()
        // 3. Slice selectors: [start:end] or [start:end:step]
        // 4. Recursive descent: ..
        
        return ContainsFilterSelector(jsonPath) ||
               ContainsFunctions(jsonPath) ||
               ContainsSliceSelector(jsonPath) ||
               ContainsRecursiveDescent(jsonPath);
    }

    /// <summary>
    /// Checks if the JSONPath contains a filter selector [?expression].
    /// Optimized with IndexOf for zero-allocation scanning.
    /// </summary>
    private static bool ContainsFilterSelector(ReadOnlySpan<char> jsonPath)
    {
        return jsonPath.IndexOf("[?".AsSpan(), StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Checks if the JSONPath contains any RFC 9535 built-in functions.
    /// Functions must be followed by '(' to be valid.
    /// Optimized with sequential IndexOf calls and early returns.
    /// </summary>
    private static bool ContainsFunctions(ReadOnlySpan<char> jsonPath)
    {
        // Early return optimization: check for '(' first (common in all functions)
        if (jsonPath.IndexOf('(') < 0)
        {
            return false;
        }

        // Use IndexOf for zero-allocation checks
        return jsonPath.IndexOf("length(".AsSpan(), StringComparison.Ordinal) >= 0 ||
               jsonPath.IndexOf("count(".AsSpan(), StringComparison.Ordinal) >= 0 ||
               jsonPath.IndexOf("match(".AsSpan(), StringComparison.Ordinal) >= 0 ||
               jsonPath.IndexOf("search(".AsSpan(), StringComparison.Ordinal) >= 0 ||
               jsonPath.IndexOf("value(".AsSpan(), StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Checks if the JSONPath contains a slice selector [start:end] or [start:end:step].
    /// Optimized with early bracket detection and span-based scanning.
    /// </summary>
    private static bool ContainsSliceSelector(ReadOnlySpan<char> jsonPath)
    {
        // Early return: no brackets means no slicing
        if (jsonPath.IndexOf('[') < 0 || jsonPath.IndexOf(':') < 0)
        {
            return false;
        }

        // Check for colons within brackets (array slice syntax)
        return HasColonInBrackets(jsonPath);
    }

    /// <summary>
    /// Checks if the JSONPath contains recursive descent (..) operator.
    /// Optimized with IndexOf for zero-allocation scanning.
    /// </summary>
    private static bool ContainsRecursiveDescent(ReadOnlySpan<char> jsonPath)
    {
        return jsonPath.IndexOf("..".AsSpan(), StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Checks if there are colons within brackets (slice detection).
    /// Optimized with ReadOnlySpan and minimal allocations.
    /// Handles string literals to avoid false positives.
    /// </summary>
    private static bool HasColonInBrackets(ReadOnlySpan<char> jsonPath)
    {
        bool inBrackets = false;
        bool inString = false;
        char stringDelimiter = '\0';

        for (int i = 0; i < jsonPath.Length; i++)
        {
            char c = jsonPath[i];

            // Handle string literals to avoid counting colons in strings
            if ((c == '\'' || c == '"') && (i == 0 || jsonPath[i - 1] != '\\'))
            {
                if (!inString)
                {
                    inString = true;
                    stringDelimiter = c;
                }
                else if (c == stringDelimiter)
                {
                    inString = false;
                    stringDelimiter = '\0';
                }
                continue;
            }

            // Skip characters in strings
            if (inString)
            {
                continue;
            }

            // Track bracket depth
            if (c == '[')
            {
                inBrackets = true;
            }
            else if (c == ']')
            {
                inBrackets = false;
            }
            else if (c == ':' && inBrackets)
            {
                return true; // Early return on first colon found in brackets
            }
        }

        return false;
    }

    /// <summary>
    /// Categorizes the complexity of a JSONPath expression.
    /// </summary>
    public enum QueryComplexity
    {
        /// <summary>
        /// Simple path navigation only (e.g., $.store.book[0])
        /// </summary>
        Simple,

        /// <summary>
        /// Contains basic RFC features (slicing, recursive descent)
        /// </summary>
        Moderate,

        /// <summary>
        /// Contains advanced features (filters, functions, logical operators)
        /// </summary>
        Complex
    }

    /// <summary>
    /// Analyzes the complexity of a JSONPath expression.
    /// Optimized with ReadOnlySpan and early returns.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression to analyze.</param>
    /// <returns>The complexity level of the expression.</returns>
    public static QueryComplexity AnalyzeComplexity(string jsonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        return AnalyzeComplexity(jsonPath.AsSpan());
    }

    /// <summary>
    /// Analyzes the complexity of a JSONPath expression.
    /// Optimized with ReadOnlySpan and early returns.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression to analyze.</param>
    /// <returns>The complexity level of the expression.</returns>
    public static QueryComplexity AnalyzeComplexity(ReadOnlySpan<char> jsonPath)
    {
        // Complex: Contains filters or functions (early return for best performance)
        if (ContainsFilterSelector(jsonPath) || ContainsFunctions(jsonPath))
        {
            return QueryComplexity.Complex;
        }

        // Moderate: Contains slicing or recursive descent
        if (ContainsSliceSelector(jsonPath) || ContainsRecursiveDescent(jsonPath))
        {
            return QueryComplexity.Moderate;
        }

        // Simple: Basic navigation only
        return QueryComplexity.Simple;
    }
}
