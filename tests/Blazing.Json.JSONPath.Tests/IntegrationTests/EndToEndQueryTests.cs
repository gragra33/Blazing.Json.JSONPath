using System.Text.Json;
using Blazing.Json.JSONPath.Tests.Fixtures;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.IntegrationTests;

/// <summary>
/// End-to-end integration tests for complete JSONPath query workflows.
/// Tests parse → evaluate → result extraction pipelines.
/// </summary>
public class EndToEndQueryTests
{
    [Fact]
    public void SimpleQuery_RootOnly_ReturnsRootDocument()
    {
        var result = TestHelpers.QueryJson("$", RfcTestData.SimpleObjectJson);

        result.Count.ShouldBe(1);
        result[0].NormalizedPath.ShouldBe("$");
        result[0].Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public void SimpleQuery_NameSelector_ReturnsNamedProperty()
    {
        var result = TestHelpers.QueryJson("$.name", RfcTestData.SimpleObjectJson);

        result.Count.ShouldBe(1);
        result[0].Value.GetString().ShouldBe("John Doe");
        result[0].NormalizedPath.ShouldBe("$['name']");
    }

    [Fact]
    public void ComplexQuery_RecursiveDescent_FindsAllNestedValues()
    {
        var result = TestHelpers.QueryJson("$..city", RfcTestData.NestedObjectJson);

        result.Count.ShouldBe(1);
        result[0].Value.GetString().ShouldBe("New York");
    }

    [Fact]
    public void ArrayQuery_Wildcard_SelectsAllElements()
    {
        var result = TestHelpers.QueryJson("$[*]", RfcTestData.SimpleArrayJson);

        result.Count.ShouldBe(4);
        var values = result.GetValues().Select(v => v.GetString()).ToList();
        values.ShouldBe(new[] { "apple", "banana", "cherry", "date" });
    }

    [Fact]
    public void ArrayQuery_Slice_SelectsRange()
    {
        var result = TestHelpers.QueryJson("$[1:3]", RfcTestData.SimpleArrayJson);

        result.Count.ShouldBe(2);
        var values = result.GetValues().Select(v => v.GetString()).ToList();
        values.ShouldBe(new[] { "banana", "cherry" });
    }

    [Fact]
    public void FilterQuery_NumericComparison_FiltersCorrectly()
    {
        var result = TestHelpers.QueryJson("$[?@.score > 80]", RfcTestData.ArrayWithObjectsJson);

        result.Count.ShouldBe(3);
        var names = result.GetValues().Select(v => v.GetProperty("name").GetString()).ToList();
        names.ShouldContain("Alice");
        names.ShouldContain("Bob");
        names.ShouldContain("Diana");
    }

    [Fact]
    public void FilterQuery_StringComparison_FiltersCorrectly()
    {
        var result = TestHelpers.QueryJson("$[?@.name == 'Bob']", RfcTestData.ArrayWithObjectsJson);

        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("score").GetInt32().ShouldBe(92);
    }

    [Fact]
    public void FilterQuery_LogicalAnd_CombinesConditions()
    {
        var result = TestHelpers.QueryJson("$[?@.score > 80 && @.id < 3]", RfcTestData.ArrayWithObjectsJson);

        result.Count.ShouldBe(2);
        var names = result.GetValues().Select(v => v.GetProperty("name").GetString()).ToList();
        names.ShouldBe(new[] { "Alice", "Bob" });
    }

    [Fact]
    public void FilterQuery_ExistenceTest_FindsNodesWithProperty()
    {
        var json = """
        [
          {"name":"Alice","email":"alice@example.com"},
          {"name":"Bob"},
          {"name":"Charlie","email":"charlie@example.com"}
        ]
        """;

        var result = TestHelpers.QueryJson("$[?@.email]", json);

        result.Count.ShouldBe(2);
        var names = result.GetValues().Select(v => v.GetProperty("name").GetString()).ToList();
        names.ShouldBe(new[] { "Alice", "Charlie" });
    }

    [Fact]
    public void FunctionQuery_Length_FiltersBasedOnStringLength()
    {
        var result = TestHelpers.QueryJson("$[?length(@.name) > 5]", RfcTestData.ArrayWithObjectsJson);

        result.Count.ShouldBe(1);
        result[0].Value.GetProperty("name").GetString().ShouldBe("Charlie");
    }

    [Fact]
    public void MultipleSelectors_CombinesResults()
    {
        var result = TestHelpers.QueryJson("$[0,2]", RfcTestData.SimpleArrayJson);

        result.Count.ShouldBe(2);
        var values = result.GetValues().Select(v => v.GetString()).ToList();
        values.ShouldBe(new[] { "apple", "cherry" });
    }

    [Fact]
    public void NormalizedPaths_GeneratedCorrectly()
    {
        var paths = TestHelpers.QueryJsonPaths("$.user.profile.name", RfcTestData.NestedObjectJson);

        paths.Count.ShouldBe(1);
        paths[0].ShouldBe("$['user']['profile']['name']");
    }

    [Fact]
    public void EmptyResult_ReturnsEmptyNodelist()
    {
        var result = TestHelpers.QueryJson("$.nonexistent", RfcTestData.SimpleObjectJson);

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void ComplexNestedQuery_WorksEndToEnd()
    {
        var json = """
        {
          "data": {
            "users": [
              {
                "name": "Alice",
                "orders": [
                  {"id": 1, "total": 100},
                  {"id": 2, "total": 200}
                ]
              },
              {
                "name": "Bob",
                "orders": [
                  {"id": 3, "total": 150}
                ]
              }
            ]
          }
        }
        """;

        var result = TestHelpers.QueryJson("$.data.users[*].orders[?@.total > 100]", json);

        result.Count.ShouldBe(2);
        var totals = result.GetValues().Select(v => v.GetProperty("total").GetInt32()).OrderBy(x => x).ToList();
        totals.ShouldBe(new[] { 150, 200 });
    }
}
