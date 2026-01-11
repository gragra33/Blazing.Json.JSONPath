using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Functions;
using Blazing.Json.JSONPath.Functions.BuiltIn;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Functions;

/// <summary>
/// Tests for the length() built-in function.
/// Validates RFC 9535 Section 2.4.4 compliance.
/// </summary>
public sealed class LengthFunctionTests
{
    private readonly LengthFunction _function = new();
    private readonly EvaluationContext _context = new(default, default);

    #region String Length Tests

    [Fact]
    public void Length_String_ReturnsCharacterCount()
    {
        // Arrange
        var value = JsonDocument.Parse("\"hello\"").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Type.ShouldBe(FunctionType.ValueType);
        result.Value!.Value.GetInt32().ShouldBe(5);
    }

    [Fact]
    public void Length_EmptyString_ReturnsZero()
    {
        // Arrange
        var value = JsonDocument.Parse("\"\"").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(0);
    }

    [Fact]
    public void Length_UnicodeString_CountsScalarValues()
    {
        // Arrange - emoji is one Unicode scalar value (U+1F600)
        var value = JsonDocument.Parse("\"\\uD83D\\uDE00\"").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(1); // One emoji = one scalar value
    }

    #endregion

    #region Array Length Tests

    [Fact]
    public void Length_Array_ReturnsElementCount()
    {
        // Arrange
        var value = JsonDocument.Parse("[1, 2, 3, 4, 5]").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(5);
    }

    [Fact]
    public void Length_EmptyArray_ReturnsZero()
    {
        // Arrange
        var value = JsonDocument.Parse("[]").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(0);
    }

    #endregion

    #region Object Length Tests

    [Fact]
    public void Length_Object_ReturnsMemberCount()
    {
        // Arrange
        var value = JsonDocument.Parse("""{"a": 1, "b": 2, "c": 3}""").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(3);
    }

    [Fact]
    public void Length_EmptyObject_ReturnsZero()
    {
        // Arrange
        var value = JsonDocument.Parse("{}").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.Value!.Value.GetInt32().ShouldBe(0);
    }

    #endregion

    #region Nothing and Other Types Tests

    [Fact]
    public void Length_Nothing_ReturnsNothing()
    {
        // Arrange
        var args = new List<FunctionArgument> { ValueArgument.Nothing };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    [Fact]
    public void Length_Number_ReturnsNothing()
    {
        // Arrange
        var value = JsonDocument.Parse("42").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    [Fact]
    public void Length_Boolean_ReturnsNothing()
    {
        // Arrange
        var value = JsonDocument.Parse("true").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    [Fact]
    public void Length_Null_ReturnsNothing()
    {
        // Arrange
        var value = JsonDocument.Parse("null").RootElement;
        var args = new List<FunctionArgument> { new ValueArgument(value) };

        // Act
        var result = _function.Execute(args, _context);

        // Assert
        result.IsNothing.ShouldBeTrue();
    }

    #endregion
}
