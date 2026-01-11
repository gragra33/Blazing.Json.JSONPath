using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser;
using Blazing.Json.JSONPath.Parser.Nodes;

namespace Blazing.Json.JSONPath.Benchmarks;

/// <summary>
/// Performance benchmarks for RFC 9535 JSONPath implementation.
/// Targets: Simple query parsing < 5 μs, evaluation < 500 μs (1K nodes)
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class JsonPathBenchmarks
{
    private JsonElement _smallJson;
    private JsonElement _mediumJson;
    private JsonElement _largeJson;
    private JsonPathQuery _simpleQuery = null!;
    private JsonPathQuery _filterQuery = null!;
    private JsonPathQuery _recursiveQuery = null!;
    private JsonPathQuery _sliceQuery = null!;
    private JsonPathEvaluator _evaluator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _evaluator = new JsonPathEvaluator();

        // Small JSON (10 items)
        _smallJson = JsonDocument.Parse(GenerateTestData(10)).RootElement;

        // Medium JSON (100 items)
        _mediumJson = JsonDocument.Parse(GenerateTestData(100)).RootElement;

        // Large JSON (1000 items)
        _largeJson = JsonDocument.Parse(GenerateTestData(1000)).RootElement;

        // Pre-parse queries
        _simpleQuery = JsonPathParser.Parse("$.data[*].name");
        _filterQuery = JsonPathParser.Parse("$.data[?@.price < 50]");
        _recursiveQuery = JsonPathParser.Parse("$..price");
        _sliceQuery = JsonPathParser.Parse("$.data[10:20]");
    }

    private static string GenerateTestData(int count)
    {
        var items = new List<object>();
        for (int i = 0; i < count; i++)
        {
            items.Add(new
            {
                id = i,
                name = $"Item{i}",
                price = (i % 100) + 10.5,
                category = i % 2 == 0 ? "A" : "B",
                inStock = i % 3 == 0,
                tags = new[] { $"tag{i % 5}", $"tag{i % 3}" }
            });
        }
        return JsonSerializer.Serialize(new { data = items });
    }

    #region Parsing Benchmarks

    [Benchmark]
    [BenchmarkCategory("Parsing")]
    public JsonPathQuery Parse_SimpleQuery()
    {
        return JsonPathParser.Parse("$.data[*].name");
    }

    [Benchmark]
    [BenchmarkCategory("Parsing")]
    public JsonPathQuery Parse_FilterQuery()
    {
        return JsonPathParser.Parse("$.data[?@.price < 50]");
    }

    [Benchmark]
    [BenchmarkCategory("Parsing")]
    public JsonPathQuery Parse_RecursiveQuery()
    {
        return JsonPathParser.Parse("$..price");
    }

    [Benchmark]
    [BenchmarkCategory("Parsing")]
    public JsonPathQuery Parse_SliceQuery()
    {
        return JsonPathParser.Parse("$.data[10:20:2]");
    }

    [Benchmark]
    [BenchmarkCategory("Parsing")]
    public JsonPathQuery Parse_ComplexFilter()
    {
        return JsonPathParser.Parse("$.data[?@.category == 'A' && @.price < 50 && @.inStock]");
    }

    #endregion

    #region Evaluation Benchmarks - Small Dataset (10 items)

    [Benchmark]
    [BenchmarkCategory("Evaluation-Small")]
    public Nodelist Evaluate_Simple_Small()
    {
        return _evaluator.Evaluate(_simpleQuery, _smallJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Small")]
    public Nodelist Evaluate_Filter_Small()
    {
        return _evaluator.Evaluate(_filterQuery, _smallJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Small")]
    public Nodelist Evaluate_Recursive_Small()
    {
        return _evaluator.Evaluate(_recursiveQuery, _smallJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Small")]
    public Nodelist Evaluate_Slice_Small()
    {
        return _evaluator.Evaluate(_sliceQuery, _smallJson);
    }

    #endregion

    #region Evaluation Benchmarks - Medium Dataset (100 items)

    [Benchmark]
    [BenchmarkCategory("Evaluation-Medium")]
    public Nodelist Evaluate_Simple_Medium()
    {
        return _evaluator.Evaluate(_simpleQuery, _mediumJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Medium")]
    public Nodelist Evaluate_Filter_Medium()
    {
        return _evaluator.Evaluate(_filterQuery, _mediumJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Medium")]
    public Nodelist Evaluate_Recursive_Medium()
    {
        return _evaluator.Evaluate(_recursiveQuery, _mediumJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Medium")]
    public Nodelist Evaluate_Slice_Medium()
    {
        return _evaluator.Evaluate(_sliceQuery, _mediumJson);
    }

    #endregion

    #region Evaluation Benchmarks - Large Dataset (1000 items)

    [Benchmark]
    [BenchmarkCategory("Evaluation-Large")]
    public Nodelist Evaluate_Simple_Large()
    {
        return _evaluator.Evaluate(_simpleQuery, _largeJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Large")]
    public Nodelist Evaluate_Filter_Large()
    {
        return _evaluator.Evaluate(_filterQuery, _largeJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Large")]
    public Nodelist Evaluate_Recursive_Large()
    {
        return _evaluator.Evaluate(_recursiveQuery, _largeJson);
    }

    [Benchmark]
    [BenchmarkCategory("Evaluation-Large")]
    public Nodelist Evaluate_Slice_Large()
    {
        return _evaluator.Evaluate(_sliceQuery, _largeJson);
    }

    #endregion

    #region End-to-End Benchmarks

    [Benchmark(Description = "Parse + Evaluate: Simple query on 100 items")]
    [BenchmarkCategory("End-to-End")]
    public Nodelist EndToEnd_Simple()
    {
        var query = JsonPathParser.Parse("$.data[*].name");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    [Benchmark(Description = "Parse + Evaluate: Filter query on 100 items")]
    [BenchmarkCategory("End-to-End")]
    public Nodelist EndToEnd_Filter()
    {
        var query = JsonPathParser.Parse("$.data[?@.price < 50]");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    [Benchmark(Description = "Parse + Evaluate: Recursive descent on 100 items")]
    [BenchmarkCategory("End-to-End")]
    public Nodelist EndToEnd_Recursive()
    {
        var query = JsonPathParser.Parse("$..price");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    #endregion

    #region Function Benchmarks

    [Benchmark]
    [BenchmarkCategory("Functions")]
    public Nodelist Function_Length()
    {
        var query = JsonPathParser.Parse("$.data[?length(@.name) > 5]");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    [Benchmark]
    [BenchmarkCategory("Functions")]
    public Nodelist Function_Count()
    {
        var query = JsonPathParser.Parse("$.data[?count(@.tags[*]) > 1]");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    [Benchmark]
    [BenchmarkCategory("Functions")]
    public Nodelist Function_Match()
    {
        var query = JsonPathParser.Parse(@"$.data[?match(@.name, 'Item[0-9]+')]");
        return _evaluator.Evaluate(query, _mediumJson);
    }

    #endregion

    #region Memory Allocation Benchmarks

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public Nodelist Memory_Nodelist_Build()
    {
        using var builder = new NodelistBuilder();
        for (int i = 0; i < 100; i++)
        {
            builder.Add(new JsonNode(_mediumJson, $"$[{i}]"));
        }
        return builder.ToNodelist();
    }

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public Nodelist Memory_Nodelist_WithDeduplication()
    {
        using var builder = new NodelistBuilder();
        for (int i = 0; i < 100; i++)
        {
            builder.Add(new JsonNode(_mediumJson, "$[0]")); // All same path
        }
        return builder.ToNodelist(deduplicate: true);
    }

    #endregion
}
