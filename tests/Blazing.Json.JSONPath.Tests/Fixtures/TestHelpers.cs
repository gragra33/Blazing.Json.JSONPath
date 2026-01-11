using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;

namespace Blazing.Json.JSONPath.Tests.Fixtures;

/// <summary>
/// Helper methods for JSONPath tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Parse and evaluate a JSONPath query against JSON text.
    /// </summary>
    public static Nodelist QueryJson(string jsonPath, string jsonText)
    {
        var json = JsonDocument.Parse(jsonText).RootElement;
        var query = JsonPathParser.Parse(jsonPath);
        var evaluator = new JsonPathEvaluator();
        return evaluator.Evaluate(query, json);
    }

    /// <summary>
    /// Parse and evaluate a JSONPath query, returning values as strings.
    /// </summary>
    public static IReadOnlyList<string> QueryJsonAsStrings(string jsonPath, string jsonText)
    {
        var result = QueryJson(jsonPath, jsonText);
        return result.GetValues()
            .Select(v => v.ValueKind == JsonValueKind.String 
                ? v.GetString()! 
                : v.ToString())
            .ToList();
    }

    /// <summary>
    /// Parse and evaluate a JSONPath query, returning values as numbers.
    /// </summary>
    public static IReadOnlyList<double> QueryJsonAsNumbers(string jsonPath, string jsonText)
    {
        var result = QueryJson(jsonPath, jsonText);
        return result.GetValues()
            .Select(v => v.GetDouble())
            .ToList();
    }

    /// <summary>
    /// Parse and evaluate a JSONPath query, returning count of matches.
    /// </summary>
    public static int QueryJsonCount(string jsonPath, string jsonText)
    {
        var result = QueryJson(jsonPath, jsonText);
        return result.Count;
    }

    /// <summary>
    /// Get normalized paths from a query result.
    /// </summary>
    public static IReadOnlyList<string> QueryJsonPaths(string jsonPath, string jsonText)
    {
        var result = QueryJson(jsonPath, jsonText);
        return result.GetNormalizedPaths();
    }
}
