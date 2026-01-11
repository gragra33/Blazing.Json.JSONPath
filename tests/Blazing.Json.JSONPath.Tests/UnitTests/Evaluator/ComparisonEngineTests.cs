using System.Text.Json;
using Blazing.Json.JSONPath.Evaluator;
using Blazing.Json.JSONPath.Parser.Nodes;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Evaluator;

/// <summary>
/// Tests for the <see cref="ComparisonEngine"/> class.
/// Validates RFC 9535 Section 2.3.5.2.2 comparison semantics (Table 11).
/// </summary>
public sealed class ComparisonEngineTests
{
    #region RFC Table 11 Examples

    [Fact]
    public void Compare_TwoNothings_Equal()
    {
        // RFC Table 11: $.absent1 == $.absent2 ? true
        var result = ComparisonEngine.Compare(
            ComparableValue.Nothing,
            ComparisonOperator.Equal,
            ComparableValue.Nothing);

        result.ShouldBeTrue();
    }

    [Fact]
    public void Compare_NothingAndValue_NotEqual()
    {
        // RFC Table 11: $.absent == 'g' ? false
        var value = JsonDocument.Parse("\"g\"").RootElement;
        var result = ComparisonEngine.Compare(
            ComparableValue.Nothing,
            ComparisonOperator.Equal,
            ComparableValue.FromValue(value));

        result.ShouldBeFalse();
    }

    [Fact]
    public void Compare_ValueAndNothing_NotEqual()
    {
        // Symmetric test
        var value = JsonDocument.Parse("\"g\"").RootElement;
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(value),
            ComparisonOperator.Equal,
            ComparableValue.Nothing);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Compare_TwoNothings_NotNotEqual()
    {
        // !(Nothing == Nothing) = false
        var result = ComparisonEngine.Compare(
            ComparableValue.Nothing,
            ComparisonOperator.NotEqual,
            ComparableValue.Nothing);

        result.ShouldBeFalse();
    }

    [Fact]
    public void Compare_NothingLessThanValue_False()
    {
        // RFC Table 11: Nothing is never less than anything
        var value = JsonDocument.Parse("1").RootElement;
        var result = ComparisonEngine.Compare(
            ComparableValue.Nothing,
            ComparisonOperator.Less,
            ComparableValue.FromValue(value));

        result.ShouldBeFalse();
    }

    #endregion

    #region Numeric Comparison Tests

    [Theory]
    [InlineData(1, 2, ComparisonOperator.Less, true)]
    [InlineData(2, 1, ComparisonOperator.Less, false)]
    [InlineData(1, 1, ComparisonOperator.Less, false)]
    [InlineData(1, 2, ComparisonOperator.LessEqual, true)]
    [InlineData(2, 2, ComparisonOperator.LessEqual, true)]
    [InlineData(3, 2, ComparisonOperator.LessEqual, false)]
    public void Compare_Numbers_Correctly(int left, int right, ComparisonOperator op, bool expected)
    {
        // Arrange
        var leftValue = JsonDocument.Parse(left.ToString()).RootElement;
        var rightValue = JsonDocument.Parse(right.ToString()).RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            op,
            ComparableValue.FromValue(rightValue));

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Compare_FloatingPoint_HandlesEpsilon()
    {
        // Arrange
        var left = JsonDocument.Parse("10.0").RootElement;
        var right = JsonDocument.Parse("10.0").RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(left),
            ComparisonOperator.Equal,
            ComparableValue.FromValue(right));

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region String Comparison Tests

    [Theory]
    [InlineData("a", "b", ComparisonOperator.Less, true)]
    [InlineData("b", "a", ComparisonOperator.Less, false)]
    [InlineData("a", "a", ComparisonOperator.Equal, true)]
    [InlineData("a", "A", ComparisonOperator.Equal, false)] // Case sensitive
    [InlineData("apple", "banana", ComparisonOperator.Less, true)]
    public void Compare_Strings_OrdinalComparison(string left, string right, ComparisonOperator op, bool expected)
    {
        // Arrange
        var leftValue = JsonDocument.Parse($"\"{left}\"").RootElement;
        var rightValue = JsonDocument.Parse($"\"{right}\"").RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            op,
            ComparableValue.FromValue(rightValue));

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Boolean Comparison Tests

    [Theory]
    [InlineData(true, true, ComparisonOperator.Equal, true)]
    [InlineData(true, false, ComparisonOperator.Equal, false)]
    [InlineData(false, true, ComparisonOperator.Less, true)] // false < true
    [InlineData(true, false, ComparisonOperator.Less, false)]
    [InlineData(false, false, ComparisonOperator.LessEqual, true)]
    public void Compare_Booleans_Correctly(bool left, bool right, ComparisonOperator op, bool expected)
    {
        // Arrange
        var leftValue = JsonDocument.Parse(left.ToString().ToLower()).RootElement;
        var rightValue = JsonDocument.Parse(right.ToString().ToLower()).RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            op,
            ComparableValue.FromValue(rightValue));

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Null Comparison Tests

    [Fact]
    public void Compare_TwoNulls_Equal()
    {
        // Arrange
        var leftValue = JsonDocument.Parse("null").RootElement;
        var rightValue = JsonDocument.Parse("null").RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            ComparisonOperator.Equal,
            ComparableValue.FromValue(rightValue));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Compare_NullsNotLess()
    {
        // Arrange
        var leftValue = JsonDocument.Parse("null").RootElement;
        var rightValue = JsonDocument.Parse("null").RootElement;

        // Act
        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            ComparisonOperator.Less,
            ComparableValue.FromValue(rightValue));

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Type Mismatch Tests

    [Fact]
    public void Compare_NumberAndString_TypeMismatch()
    {
        // RFC: Type mismatch ? only != can be true
        var numberValue = JsonDocument.Parse("13").RootElement;
        var stringValue = JsonDocument.Parse("\"13\"").RootElement;

        // Equal should be false
        var equalResult = ComparisonEngine.Compare(
            ComparableValue.FromValue(numberValue),
            ComparisonOperator.Equal,
            ComparableValue.FromValue(stringValue));
        equalResult.ShouldBeFalse();

        // NotEqual should be true
        var notEqualResult = ComparisonEngine.Compare(
            ComparableValue.FromValue(numberValue),
            ComparisonOperator.NotEqual,
            ComparableValue.FromValue(stringValue));
        notEqualResult.ShouldBeTrue();

        // Less should be false
        var lessResult = ComparisonEngine.Compare(
            ComparableValue.FromValue(numberValue),
            ComparisonOperator.Less,
            ComparableValue.FromValue(stringValue));
        lessResult.ShouldBeFalse();
    }

    [Fact]
    public void Compare_StringAndBoolean_TypeMismatch()
    {
        var stringValue = JsonDocument.Parse("\"true\"").RootElement;
        var boolValue = JsonDocument.Parse("true").RootElement;

        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(stringValue),
            ComparisonOperator.Equal,
            ComparableValue.FromValue(boolValue));

        result.ShouldBeFalse();
    }

    #endregion

    #region Operator Derivation Tests

    [Theory]
    [InlineData(5, 10, ComparisonOperator.Greater, false)]
    [InlineData(10, 5, ComparisonOperator.Greater, true)]
    [InlineData(5, 5, ComparisonOperator.Greater, false)]
    [InlineData(5, 5, ComparisonOperator.GreaterEqual, true)]
    [InlineData(10, 5, ComparisonOperator.GreaterEqual, true)]
    [InlineData(5, 10, ComparisonOperator.GreaterEqual, false)]
    public void Compare_DerivedOperators_FromEqualAndLess(int left, int right, ComparisonOperator op, bool expected)
    {
        // Tests that >, >= are correctly derived from == and <
        var leftValue = JsonDocument.Parse(left.ToString()).RootElement;
        var rightValue = JsonDocument.Parse(right.ToString()).RootElement;

        var result = ComparisonEngine.Compare(
            ComparableValue.FromValue(leftValue),
            op,
            ComparableValue.FromValue(rightValue));

        result.ShouldBe(expected);
    }

    #endregion
}
