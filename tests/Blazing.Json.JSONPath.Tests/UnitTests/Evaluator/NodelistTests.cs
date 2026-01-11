using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// Tests for the <see cref="Nodelist"/> class.
/// </summary>
public sealed class NodelistTests
{
    #region Creation Tests

    [Fact]
    public void Empty_ReturnsEmptyNodelist()
    {
        // Act
        var nodelist = Nodelist.Empty;

        // Assert
        nodelist.ShouldNotBeNull();
        nodelist.Count.ShouldBe(0);
    }

    [Fact]
    public void FromRoot_CreatesNodelistWithRootNode()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;

        // Act
        var nodelist = Nodelist.FromRoot(json);

        // Assert
        nodelist.Count.ShouldBe(1);
        nodelist[0].NormalizedPath.ShouldBe("$");
        nodelist[0].Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Fact]
    public void Create_FromEnumerable_CreatesNodelist()
    {
        // Arrange
        var json1 = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var json2 = JsonDocument.Parse("""{"b": 2}""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json1, "$[0]"),
            new JsonNode(json2, "$[1]")
        };

        // Act
        var nodelist = Nodelist.Create(nodes);

        // Assert
        nodelist.Count.ShouldBe(2);
        nodelist[0].NormalizedPath.ShouldBe("$[0]");
        nodelist[1].NormalizedPath.ShouldBe("$[1]");
    }

    [Fact]
    public void Create_FromSpan_CreatesNodelist()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var nodes = new JsonNode[2]
        {
            new JsonNode(json, "$[0]"),
            new JsonNode(json, "$[1]")
        };

        // Act
        var nodelist = Nodelist.Create(nodes.AsSpan());

        // Assert
        nodelist.Count.ShouldBe(2);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_ReturnsCorrectNode()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act & Assert
        nodelist[0].Value.GetInt32().ShouldBe(1);
        nodelist[1].Value.GetInt32().ShouldBe(2);
        nodelist[2].Value.GetInt32().ShouldBe(3);
    }

    #endregion

    #region GetNormalizedPaths Tests

    [Fact]
    public void GetNormalizedPaths_ReturnsAllPaths()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json, "$['a']"),
            new JsonNode(json, "$['b']"),
            new JsonNode(json, "$['c']")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var paths = nodelist.GetNormalizedPaths();

        // Assert
        paths.Length.ShouldBe(3);
        paths[0].ShouldBe("$['a']");
        paths[1].ShouldBe("$['b']");
        paths[2].ShouldBe("$['c']");
    }

    #endregion

    #region GetValues Tests

    [Fact]
    public void GetValues_ReturnsAllValues()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var values = nodelist.GetValues();

        // Assert
        values.Length.ShouldBe(3);
        values[0].GetInt32().ShouldBe(1);
        values[1].GetInt32().ShouldBe(2);
        values[2].GetInt32().ShouldBe(3);
    }

    #endregion

    #region Deduplication Tests

    [Fact]
    public void Deduplicate_RemovesDuplicatePaths()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json, "$['a']"),
            new JsonNode(json, "$['b']"),
            new JsonNode(json, "$['a']"), // Duplicate
            new JsonNode(json, "$['c']")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var deduplicated = nodelist.Deduplicate();

        // Assert
        deduplicated.Count.ShouldBe(3);
        var paths = deduplicated.GetNormalizedPaths();
        paths.ShouldContain("$['a']");
        paths.ShouldContain("$['b']");
        paths.ShouldContain("$['c']");
    }

    [Fact]
    public void Deduplicate_EmptyNodelist_RemainsEmpty()
    {
        // Arrange
        var nodelist = Nodelist.Empty;

        // Act
        var deduplicated = nodelist.Deduplicate();

        // Assert
        deduplicated.Count.ShouldBe(0);
    }

    [Fact]
    public void Deduplicate_SingleNode_ReturnsSame()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var nodelist = Nodelist.FromRoot(json);

        // Act
        var deduplicated = nodelist.Deduplicate();

        // Assert
        deduplicated.Count.ShouldBe(1);
        deduplicated[0].NormalizedPath.ShouldBe("$");
    }

    #endregion

    #region ToLogical Tests

    [Fact]
    public void ToLogical_EmptyNodelist_ReturnsFalse()
    {
        // Arrange
        var nodelist = Nodelist.Empty;

        // Act
        var result = nodelist.ToLogical();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void ToLogical_NonEmptyNodelist_ReturnsTrue()
    {
        // Arrange
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var nodelist = Nodelist.FromRoot(json);

        // Act
        var result = nodelist.ToLogical();

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumerator_EnumeratesAllNodes()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var count = 0;
        foreach (var node in nodelist)
        {
            count++;
        }

        // Assert
        count.ShouldBe(3);
    }

    [Fact]
    public void GetEnumerator_CanUseLinq()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3, 4, 5]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]"),
            new JsonNode(json[3], "$[3]"),
            new JsonNode(json[4], "$[4]")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var evenValues = nodelist
            .Where(n => n.Value.GetInt32() % 2 == 0)
            .Select(n => n.Value.GetInt32())
            .ToList();

        // Assert
        evenValues.ShouldBe([2, 4]);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsReadableRepresentation()
    {
        // Arrange
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };
        var nodelist = Nodelist.Create(nodes);

        // Act
        var result = nodelist.ToString();

        // Assert
        result.ShouldContain("Nodelist");
        result.ShouldContain("3");
    }

    #endregion
}
