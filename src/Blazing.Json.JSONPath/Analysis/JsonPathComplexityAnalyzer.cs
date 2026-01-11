namespace Blazing.Json.JSONPath.Analysis;

/// <summary>
/// Analyzes JSONPath queries to determine if they can use fast-path execution
/// or require the full RFC 9535 parser. This ensures zero performance regression
/// for existing simple path queries.
/// </summary>
public static class JsonPathComplexityAnalyzer
{
    /// <summary>
    /// Analyzes a JSONPath query to determine its complexity level.
    /// </summary>
    /// <param name="jsonPath">The JSONPath query string to analyze.</param>
    /// <returns>
    /// <see cref="JsonPathComplexity.Simple"/> if the path can use fast-path execution,
    /// <see cref="JsonPathComplexity.Complex"/> if it requires RFC 9535 parser.
    /// </returns>
    /// <remarks>
    /// Simple paths are those that:
    /// - Start with $ (root identifier)
    /// - Use only dot notation (.) and array wildcard ([*])
    /// - No descendant segments (..)
    /// - No filter expressions ([?...])
    /// - No array slicing ([start:end:step])
    /// - No negative indices ([-1])
    /// - No functions
    /// - No index selectors ([0], [1,2])
    /// 
    /// Examples of simple paths:
    /// - $.data[*]
    /// - $.result[*].customer
    /// - $.users[*].orders[*]
    /// 
    /// Examples of complex paths:
    /// - $..author (recursive descent)
    /// - $.book[?@.price&lt;10] (filter expression)
    /// - $.arr[-1] (negative index)
    /// - $.arr[1:5:2] (array slice)
    /// - $.book[0,1] (index selection)
    /// </remarks>
    public static JsonPathComplexity Analyze(string jsonPath)
    {
        return Analyze(jsonPath.AsSpan());
    }

    /// <summary>
    /// Analyzes a JSONPath query span to determine its complexity level.
    /// Zero-allocation implementation using ReadOnlySpan.
    /// </summary>
    /// <param name="jsonPath">The JSONPath query span to analyze.</param>
    /// <returns>The complexity level of the query.</returns>
    public static JsonPathComplexity Analyze(ReadOnlySpan<char> jsonPath)
    {
        return IsSimplePath(jsonPath)
            ? JsonPathComplexity.Simple
            : JsonPathComplexity.Complex;
    }

    /// <summary>
    /// Determines if a JSONPath query is a simple path (fast-path eligible).
    /// </summary>
    /// <param name="jsonPath">The JSONPath query span.</param>
    /// <returns>True if the path is simple, false otherwise.</returns>
    internal static bool IsSimplePath(ReadOnlySpan<char> jsonPath)
    {
        // Empty or too short
        if (jsonPath.Length < 2)
            return false;

        // Must start with $
        if (jsonPath[0] != '$')
            return false;

        // Check for complex features
        for (int i = 1; i < jsonPath.Length; i++)
        {
            char current = jsonPath[i];

            switch (current)
            {
                case '.':
                    // Check for descendant segment (..)
                    if (i + 1 < jsonPath.Length && jsonPath[i + 1] == '.')
                        return false; // Complex: recursive descent
                    break;

                case '[':
                    // Scan the bracket content
                    int bracketEnd = FindClosingBracket(jsonPath, i);
                    if (bracketEnd == -1)
                        return false; // Malformed query

                    ReadOnlySpan<char> bracketContent = jsonPath.Slice(i + 1, bracketEnd - i - 1);

                    // Only [*] is allowed for simple paths
                    if (bracketContent.Length == 1 && bracketContent[0] == '*')
                    {
                        i = bracketEnd; // Skip to closing bracket
                        continue;
                    }

                    // Any other bracket content is complex
                    // This includes:
                    // - Quoted strings: ['name'], ["name"]
                    // - Indices: [0], [1]
                    // - Negative indices: [-1]
                    // - Slices: [1:5], [::2]
                    // - Filters: [?@.price<10]
                    // - Multiple selectors: [0,1,2]
                    return false;

                case '?':
                    // Filter expression
                    return false;

                case '@':
                    // Current node (only in filters)
                    return false;

                case '(':
                    // Function call
                    return false;

                default:
                    // Check for reserved characters that indicate complexity
                    if (current == '\'' || current == '"' || current == '?' || 
                        current == '!' || current == '<' || current == '>' ||
                        current == '=' || current == '|' || current == '&')
                        return false;
                    break;
            }
        }

        return true;
    }

    /// <summary>
    /// Finds the index of the closing bracket matching an opening bracket.
    /// </summary>
    /// <param name="jsonPath">The JSONPath query span.</param>
    /// <param name="openIndex">The index of the opening bracket.</param>
    /// <returns>The index of the closing bracket, or -1 if not found.</returns>
    private static int FindClosingBracket(ReadOnlySpan<char> jsonPath, int openIndex)
    {
        int depth = 1;
        bool inString = false;
        char stringQuote = '\0';

        for (int i = openIndex + 1; i < jsonPath.Length; i++)
        {
            char c = jsonPath[i];

            // Handle string literals (escaped quotes handled simply)
            if (inString)
            {
                if (c == '\\' && i + 1 < jsonPath.Length)
                {
                    i++; // Skip escaped character
                    continue;
                }
                if (c == stringQuote)
                {
                    inString = false;
                }
                continue;
            }

            if (c == '\'' || c == '"')
            {
                inString = true;
                stringQuote = c;
                continue;
            }

            if (c == '[')
            {
                depth++;
            }
            else if (c == ']')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1; // No matching closing bracket
    }
}
