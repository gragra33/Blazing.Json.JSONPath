using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// Tests for the <see cref="NodelistBuilder"/> class.
/// </summary>
public sealed class NodelistBuilderTests
{
    #region Add Tests

    [Fact]
    public void Add_SingleNode_AddsSuccessfully()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        var node = new JsonNode(json, "$");

        // Act
        builder.Add(node);

        // Assert
        builder.Count.ShouldBe(1);
    }

    [Fact]
    public void Add_MultipleNodes_AddsAll()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;

        // Act
        builder.Add(new JsonNode(json[0], "$[0]"));
        builder.Add(new JsonNode(json[1], "$[1]"));
        builder.Add(new JsonNode(json[2], "$[2]"));

        // Assert
        builder.Count.ShouldBe(3);
    }

    [Fact]
    public void Add_BeyondInitialCapacity_GrowsBuffer()
    {
        // Arrange
        using var builder = new NodelistBuilder(capacity: 2);
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;

        // Act
        for (int i = 0; i < 10; i++)
        {
            builder.Add(new JsonNode(json, $"$[{i}]"));
        }

        // Assert
        builder.Count.ShouldBe(10);
    }

    #endregion

    #region AddRange Tests

    [Fact]
    public void AddRange_Enumerable_AddsAllNodes()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };

        // Act
        builder.AddRange(nodes);

        // Assert
        builder.Count.ShouldBe(3);
    }

    [Fact]
    public void AddRange_Nodelist_AddsAllNodes()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        var nodelist = Nodelist.FromRoot(json);

        // Act
        builder.AddRange(nodelist);

        // Assert
        builder.Count.ShouldBe(1);
    }

    [Fact]
    public void AddRange_EmptyNodelist_DoesNotAddAnything()
    {
        // Arrange
        using var builder = new NodelistBuilder();

        // Act
        builder.AddRange(Nodelist.Empty);

        // Assert
        builder.Count.ShouldBe(0);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_ResetsCount()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        builder.Add(new JsonNode(json, "$"));

        // Act
        builder.Clear();

        // Assert
        builder.Count.ShouldBe(0);
    }

    [Fact]
    public void Clear_AllowsReuse()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        builder.Add(new JsonNode(json, "$"));
        builder.Clear();

        // Act
        builder.Add(new JsonNode(json, "$[0]"));

        // Assert
        builder.Count.ShouldBe(1);
    }

    #endregion

    #region ToNodelist Tests

    [Fact]
    public void ToNodelist_EmptyBuilder_ReturnsEmptyNodelist()
    {
        // Arrange
        using var builder = new NodelistBuilder();

        // Act
        var nodelist = builder.ToNodelist();

        // Assert
        nodelist.Count.ShouldBe(0);
        nodelist.ShouldBe(Nodelist.Empty);
    }

    [Fact]
    public void ToNodelist_WithNodes_ReturnsNodelist()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""[1, 2, 3]""").RootElement;
        builder.Add(new JsonNode(json[0], "$[0]"));
        builder.Add(new JsonNode(json[1], "$[1]"));
        builder.Add(new JsonNode(json[2], "$[2]"));

        // Act
        var nodelist = builder.ToNodelist();

        // Assert
        nodelist.Count.ShouldBe(3);
        nodelist[0].NormalizedPath.ShouldBe("$[0]");
        nodelist[1].NormalizedPath.ShouldBe("$[1]");
        nodelist[2].NormalizedPath.ShouldBe("$[2]");
    }

    [Fact]
    public void ToNodelist_WithDeduplication_RemovesDuplicates()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        builder.Add(new JsonNode(json, "$['a']"));
        builder.Add(new JsonNode(json, "$['b']"));
        builder.Add(new JsonNode(json, "$['a']")); // Duplicate

        // Act
        var nodelist = builder.ToNodelist(deduplicate: true);

        // Assert
        nodelist.Count.ShouldBe(2);
    }

    [Fact]
    public void ToNodelist_WithoutDeduplication_KeepsDuplicates()
    {
        // Arrange
        using var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        builder.Add(new JsonNode(json, "$['a']"));
        builder.Add(new JsonNode(json, "$['b']"));
        builder.Add(new JsonNode(json, "$['a']")); // Duplicate

        // Act
        var nodelist = builder.ToNodelist(deduplicate: false);

        // Assert
        nodelist.Count.ShouldBe(3);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ReleasesResources()
    {
        // Arrange
        var builder = new NodelistBuilder();
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
        builder.Add(new JsonNode(json, "$"));

        // Act
        builder.Dispose();

        // Assert
        builder.Count.ShouldBe(0);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var builder = new NodelistBuilder();

        // Act & Assert
        Should.NotThrow(() =>
        {
            builder.Dispose();
            builder.Dispose();
        });
    }

    #endregion

    #region Using Statement Tests

    [Fact]
    public void UsingStatement_AutomaticallyDisposesBuilder()
    {
        // Arrange & Act
        Nodelist nodelist;
        using (var builder = new NodelistBuilder())
        {
            var json = JsonDocument.Parse("""{"a": 1}""").RootElement;
            builder.Add(new JsonNode(json, "$"));
            nodelist = builder.ToNodelist();
        }

        // Assert
        nodelist.Count.ShouldBe(1);
    }

    #endregion

    #region Capacity Growth Tests

    [Fact]
    public void CapacityGrowth_HandlesLargeNumberOfNodes()
    {
        // Arrange
        using var builder = new NodelistBuilder(capacity: 4);
        var json = JsonDocument.Parse("""{"a": 1}""").RootElement;

        // Act
        for (int i = 0; i < 100; i++)
        {
            builder.Add(new JsonNode(json, $"$[{i}]"));
        }

        // Assert
        builder.Count.ShouldBe(100);
    }

    #endregion
}
