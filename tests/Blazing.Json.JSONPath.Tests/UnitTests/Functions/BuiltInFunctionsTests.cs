using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Functions;
using Blazing.Json.JSONPath.Functions.BuiltIn;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Functions;

/// <summary>
/// Tests for all built-in functions.
/// </summary>
public sealed class BuiltInFunctionsTests
{
    private readonly EvaluationContext _context = new(default, default);

    #region count() Function Tests

    [Fact]
    public void Count_EmptyNodelist_ReturnsZero()
    {
        // Arrange
        var function = new CountFunction();
        var args = new List<FunctionArgument> { new NodesArgument(Nodelist.Empty) };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(0);
    }

    [Fact]
    public void Count_NodelistWithElements_ReturnsCount()
    {
        // Arrange
        var function = new CountFunction();
        var json = JsonDocument.Parse("[1, 2, 3]").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]"),
            new JsonNode(json[2], "$[2]")
        };
        var nodelist = Nodelist.Create(nodes);
        var args = new List<FunctionArgument> { new NodesArgument(nodelist) };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(3);
    }

    #endregion

    #region match() Function Tests

    [Fact]
    public void Match_StringMatchesPattern_ReturnsTrue()
    {
        // Arrange
        var function = new MatchFunction();
        var str = JsonDocument.Parse("\"hello\"").RootElement;
        var pattern = JsonDocument.Parse("\"h.*o\"").RootElement;
        var args = new List<FunctionArgument>
        {
            new ValueArgument(str),
            new ValueArgument(pattern)
        };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Logical!.Value.ShouldBeTrue();
    }

    [Fact]
    public void Match_StringDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var function = new MatchFunction();
        var str = JsonDocument.Parse("\"hello\"").RootElement;
        var pattern = JsonDocument.Parse("\"world\"").RootElement;
        var args = new List<FunctionArgument>
        {
            new ValueArgument(str),
            new ValueArgument(pattern)
        };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Logical!.Value.ShouldBeFalse();
    }

    [Fact]
    public void Match_NothingInput_ReturnsFalse()
    {
        // Arrange
        var function = new MatchFunction();
        var pattern = JsonDocument.Parse("\".*\"").RootElement;
        var args = new List<FunctionArgument>
        {
            ValueArgument.Nothing,
            new ValueArgument(pattern)
        };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Logical!.Value.ShouldBeFalse();
    }

    #endregion

    #region search() Function Tests

    [Fact]
    public void Search_PatternFoundInString_ReturnsTrue()
    {
        // Arrange
        var function = new SearchFunction();
        var str = JsonDocument.Parse("\"hello world\"").RootElement;
        var pattern = JsonDocument.Parse("\"world\"").RootElement;
        var args = new List<FunctionArgument>
        {
            new ValueArgument(str),
            new ValueArgument(pattern)
        };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Logical!.Value.ShouldBeTrue();
    }

    [Fact]
    public void Search_PatternNotFound_ReturnsFalse()
    {
        // Arrange
        var function = new SearchFunction();
        var str = JsonDocument.Parse("\"hello\"").RootElement;
        var pattern = JsonDocument.Parse("\"world\"").RootElement;
        var args = new List<FunctionArgument>
        {
            new ValueArgument(str),
            new ValueArgument(pattern)
        };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Logical!.Value.ShouldBeFalse();
    }

    #endregion

    #region value() Function Tests

    [Fact]
    public void Value_SingularNodelist_ReturnsValue()
    {
        // Arrange
        var function = new ValueFunction();
        var json = JsonDocument.Parse("42").RootElement;
        var nodelist = Nodelist.FromRoot(json);
        var args = new List<FunctionArgument> { new NodesArgument(nodelist) };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.Type.ShouldBe(FunctionType.ValueType);
        result.Value!.Value.GetInt32().ShouldBe(42);
    }

    [Fact]
    public void Value_EmptyNodelist_ReturnsNothing()
    {
        // Arrange
        var function = new ValueFunction();
        var args = new List<FunctionArgument> { new NodesArgument(Nodelist.Empty) };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    [Fact]
    public void Value_MultipleNodes_ReturnsNothing()
    {
        // Arrange
        var function = new ValueFunction();
        var json = JsonDocument.Parse("[1, 2]").RootElement;
        var nodes = new[]
        {
            new JsonNode(json[0], "$[0]"),
            new JsonNode(json[1], "$[1]")
        };
        var nodelist = Nodelist.Create(nodes);
        var args = new List<FunctionArgument> { new NodesArgument(nodelist) };

        // Act
        var result = function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    #endregion

    #region FunctionRegistry Tests

    [Fact]
    public void FunctionRegistry_DefaultContainsBuiltInFunctions()
    {
        // Arrange
        var registry = FunctionRegistry.Default;

        // Act & Assert
        registry.TryGetFunction("length", out _).ShouldBeTrue();
        registry.TryGetFunction("count", out _).ShouldBeTrue();
        registry.TryGetFunction("match", out _).ShouldBeTrue();
        registry.TryGetFunction("search", out _).ShouldBeTrue();
        registry.TryGetFunction("value", out _).ShouldBeTrue();
    }

    [Fact]
    public void FunctionRegistry_UnknownFunction_ThrowsException()
    {
        // Arrange
        var registry = FunctionRegistry.Default;

        // Act & Assert
        Should.Throw<Blazing.Json.JSONPath.Exceptions.JsonPathEvaluationException>(
            () => registry.GetFunction("unknown"));
    }

    #endregion
}
