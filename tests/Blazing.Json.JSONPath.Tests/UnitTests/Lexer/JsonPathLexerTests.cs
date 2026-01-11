using Blazing.Json.JSONPath.Exceptions;
using Blazing.Json.JSONPath.Lexer;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Lexer;

/// <summary>
/// Unit tests for the JsonPathLexer tokenization functionality.
/// </summary>
public class JsonPathLexerTests
{
    [Fact]
    public void Tokenize_RootIdentifier_ReturnsCorrectToken()
    {
        // Arrange
        const string query = "$";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(2); // $ + EndOfInput
        tokens[0].Type.ShouldBe(TokenType.RootIdentifier);
        tokens[0].Position.ShouldBe(0);
        tokens[0].Length.ShouldBe(1);
        tokens[1].Type.ShouldBe(TokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_CurrentIdentifier_ReturnsCorrectToken()
    {
        // Arrange
        const string query = "@";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(2);
        tokens[0].Type.ShouldBe(TokenType.CurrentIdentifier);
    }

    [Fact]
    public void Tokenize_SimplePath_ReturnsCorrectTokens()
    {
        // Arrange
        const string query = "$.store";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(4); // $, ., store, EndOfInput
        tokens[0].Type.ShouldBe(TokenType.RootIdentifier);
        tokens[1].Type.ShouldBe(TokenType.Dot);
        tokens[2].Type.ShouldBe(TokenType.MemberName);
        tokens[2].Value.ShouldBe("store");
        tokens[3].Type.ShouldBe(TokenType.EndOfInput);
    }

    [Theory]
    [InlineData("$", 2)]
    [InlineData("$.store", 4)]
    [InlineData("$['name']", 5)]
    [InlineData("$[*]", 5)]  // $, [, *, ], EndOfInput
    [InlineData("$.store.book[*]", 9)]  // $, ., store, ., book, [, *, ], EndOfInput
    public void Tokenize_VariousQueries_CorrectTokenCount(string query, int expectedCount)
    {
        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(expectedCount);
    }

    [Fact]
    public void Tokenize_BracketWildcard_ReturnsCorrectTokens()
    {
        // Arrange
        const string query = "$[*]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(5); // $, [, *, ], EndOfInput
        tokens[0].Type.ShouldBe(TokenType.RootIdentifier);
        tokens[1].Type.ShouldBe(TokenType.LeftBracket);
        tokens[2].Type.ShouldBe(TokenType.Wildcard);
        tokens[3].Type.ShouldBe(TokenType.RightBracket);
        tokens[4].Type.ShouldBe(TokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_SingleQuotedString_ReturnsCorrectToken()
    {
        // Arrange
        const string query = "$['name']";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.StringLiteral);
        tokens.First(t => t.Type == TokenType.StringLiteral).Value.ShouldBe("'name'");
    }

    [Fact]
    public void Tokenize_DoubleQuotedString_ReturnsCorrectToken()
    {
        // Arrange
        const string query = "$[\"name\"]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.StringLiteral);
        tokens.First(t => t.Type == TokenType.StringLiteral).Value.ShouldBe("\"name\"");
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("123", "123")]
    [InlineData("-5", "-5")]
    [InlineData("-0", "-0")]
    public void Tokenize_Integer_ReturnsCorrectToken(string number, string expected)
    {
        // Arrange
        var query = $"$[{number}]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.Integer);
        tokens.First(t => t.Type == TokenType.Integer).Value.ShouldBe(expected);
    }

    [Fact]
    public void Tokenize_DoubleDot_ReturnsCorrectToken()
    {
        // Arrange
        const string query = "$..author";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(4); // $, .., author, EndOfInput
        tokens[1].Type.ShouldBe(TokenType.DoubleDot);
    }

    [Theory]
    [InlineData("==", TokenType.Equal)]
    [InlineData("!=", TokenType.NotEqual)]
    [InlineData("<", TokenType.Less)]
    [InlineData("<=", TokenType.LessEqual)]
    [InlineData(">", TokenType.Greater)]
    [InlineData(">=", TokenType.GreaterEqual)]
    public void Tokenize_ComparisonOperator_ReturnsCorrectToken(string op, TokenType expectedType)
    {
        // Arrange
        var query = $"$[?@ {op} 10]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == expectedType);
    }

    [Theory]
    [InlineData("&&", TokenType.And)]
    [InlineData("||", TokenType.Or)]
    [InlineData("!", TokenType.Not)]
    public void Tokenize_LogicalOperator_ReturnsCorrectToken(string op, TokenType expectedType)
    {
        // Arrange
        var query = $"$[?{op}]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == expectedType);
    }

    [Theory]
    [InlineData("true", TokenType.True)]
    [InlineData("false", TokenType.False)]
    [InlineData("null", TokenType.Null)]
    public void Tokenize_Keyword_ReturnsCorrectToken(string keyword, TokenType expectedType)
    {
        // Arrange
        var query = $"$[?@ == {keyword}]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == expectedType);
    }

    [Theory]
    [InlineData("length", "length")]
    [InlineData("count", "count")]
    [InlineData("match", "match")]
    [InlineData("search", "search")]
    [InlineData("value", "value")]
    public void Tokenize_FunctionName_ReturnsCorrectToken(string functionName, string expectedValue)
    {
        // Arrange
        var query = $"$[?{functionName}(@.name)]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.FunctionName);
        tokens.First(t => t.Type == TokenType.FunctionName).Value.ShouldBe(expectedValue);
    }

    [Fact]
    public void Tokenize_FilterExpression_ReturnsCorrectTokenSequence()
    {
        // Arrange
        const string query = "$[?@.price < 10]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        var types = tokens.Select(t => t.Type).ToArray();
        types.ShouldBe(new[]
        {
            TokenType.RootIdentifier,
            TokenType.LeftBracket,
            TokenType.Question,
            TokenType.CurrentIdentifier,
            TokenType.Dot,
            TokenType.MemberName,
            TokenType.Less,
            TokenType.Integer,
            TokenType.RightBracket,
            TokenType.EndOfInput
        });
    }

    [Fact]
    public void Tokenize_ArraySlice_ReturnsCorrectTokenSequence()
    {
        // Arrange
        const string query = "$[1:5:2]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        var types = tokens.Select(t => t.Type).ToArray();
        types.ShouldBe(new[]
        {
            TokenType.RootIdentifier,
            TokenType.LeftBracket,
            TokenType.Integer,
            TokenType.Colon,
            TokenType.Integer,
            TokenType.Colon,
            TokenType.Integer,
            TokenType.RightBracket,
            TokenType.EndOfInput
        });
    }

    [Fact]
    public void Tokenize_WhitespaceIgnored_ReturnsCorrectTokens()
    {
        // Arrange
        const string query = "$ . store [ * ]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(7); // $, ., store, [, *, ], EndOfInput
        tokens.ShouldNotContain(t => t.Type == TokenType.Invalid);
    }

    [Fact]
    public void Tokenize_ComplexQuery_ReturnsAllTokens()
    {
        // Arrange
        const string query = "$..book[?@.price < 10 && @.category == 'fiction']";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldNotBeEmpty();
        tokens.Last().Type.ShouldBe(TokenType.EndOfInput);
        tokens.ShouldContain(t => t.Type == TokenType.DoubleDot);
        tokens.ShouldContain(t => t.Type == TokenType.Question);
        tokens.ShouldContain(t => t.Type == TokenType.Less);
        tokens.ShouldContain(t => t.Type == TokenType.And);
        tokens.ShouldContain(t => t.Type == TokenType.Equal);
        tokens.ShouldContain(t => t.Type == TokenType.StringLiteral);
    }

    [Fact]
    public void Tokenize_NullQuery_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => JsonPathLexer.Tokenize((string)null!));
    }

    [Fact]
    public void Tokenize_InvalidCharacter_ThrowsSyntaxException()
    {
        // Arrange
        const string query = "$#invalid";

        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => JsonPathLexer.Tokenize(query));
    }

    [Fact]
    public void Tokenize_UnterminatedString_ThrowsSyntaxException()
    {
        // Arrange
        const string query = "$['unterminated";

        // Act & Assert
        var exception = Should.Throw<JsonPathSyntaxException>(() => JsonPathLexer.Tokenize(query));
        exception.Message.ShouldContain("Unterminated string literal");
    }

    [Fact]
    public void Tokenize_EmptyQuery_ReturnsOnlyEndOfInput()
    {
        // Arrange
        const string query = "";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count.ShouldBe(1);
        tokens[0].Type.ShouldBe(TokenType.EndOfInput);
    }

    [Fact]
    public void Tokenize_ParenthesesAndComma_ReturnsCorrectTokens()
    {
        // Arrange
        const string query = "$[?(length(@.name) > 0)]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.LeftParen);
        tokens.ShouldContain(t => t.Type == TokenType.RightParen);
        tokens.ShouldContain(t => t.Type == TokenType.FunctionName);
    }

    [Fact]
    public void Tokenize_MultipleSelectors_ReturnsCommaTokens()
    {
        // Arrange
        const string query = "$[0,1,2]";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.Count(t => t.Type == TokenType.Comma).ShouldBe(2);
    }

    [Fact]
    public void Tokenize_TokenPositions_AreCorrect()
    {
        // Arrange
        const string query = "$.store";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens[0].Position.ShouldBe(0); // $
        tokens[1].Position.ShouldBe(1); // .
        tokens[2].Position.ShouldBe(2); // store
    }

    [Fact]
    public void Tokenize_MemberNameWithUnicode_ReturnsCorrectToken()
    {
        // Arrange - Use actual Unicode characters (Chinese for "name")
        const string query = "$.名前";

        // Act
        var tokens = JsonPathLexer.Tokenize(query);

        // Assert
        tokens.ShouldContain(t => t.Type == TokenType.MemberName);
        tokens.First(t => t.Type == TokenType.MemberName).Value.ShouldBe("名前");
    }
}
