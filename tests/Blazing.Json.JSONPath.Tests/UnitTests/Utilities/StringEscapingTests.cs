using Blazing.Json.JSONPath.Exceptions;
using Blazing.Json.JSONPath.Utilities;
using Shouldly;
using Xunit;

namespace Blazing.Json.JSONPath.Tests.UnitTests.Utilities;

/// <summary>
/// Unit tests for the StringEscaping utility.
/// </summary>
public class StringEscapingTests
{
    [Theory]
    [InlineData("'name'", "name")]
    [InlineData("\"name\"", "name")]
    [InlineData("'simple'", "simple")]
    [InlineData("\"double\"", "double")]
    public void Unescape_SimpleString_ReturnsUnquoted(string quoted, string expected)
    {
        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'tab\\there'", "tab\there")]
    [InlineData("'new\\nline'", "new\nline")]
    [InlineData("'back\\bspace'", "back\bspace")]
    [InlineData("'form\\ffeed'", "form\ffeed")]
    [InlineData("'carriage\\rreturn'", "carriage\rreturn")]
    public void Unescape_EscapeSequences_ReturnsUnescaped(string quoted, string expected)
    {
        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'quote\\'here'", "quote'here")]
    [InlineData("\"quote\\\"here\"", "quote\"here")]
    [InlineData("'back\\\\slash'", "back\\slash")]
    [InlineData("'for\\/ward'", "for/ward")]
    public void Unescape_QuotesAndSlashes_ReturnsUnescaped(string quoted, string expected)
    {
        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("'\\u0041'", "A")]
    [InlineData("'\\u0061'", "a")]
    [InlineData("'\\u0030'", "0")]
    [InlineData("'\\u00A9'", "©")]
    [InlineData("'\\u2764'", "❤")]
    public void Unescape_UnicodeEscape_ReturnsUnicodeCharacter(string quoted, string expected)
    {
        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Unescape_SurrogatePair_ReturnsCorrectCharacter()
    {
        // Arrange - Emoji "😀" (U+1F600)
        const string quoted = "'\\uD83D\\uDE00'";

        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe("😀");
    }

    [Fact]
    public void Unescape_NoEscapes_ReturnsSameString()
    {
        // Arrange
        const string quoted = "'simple text'";

        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe("simple text");
    }

    [Theory]
    [InlineData("'incomplete")]
    [InlineData("incomplete'")]
    [InlineData("\"mismatched'")]
    [InlineData("'")]
    [InlineData("\"")]
    public void Unescape_MismatchedQuotes_ThrowsSyntaxException(string quoted)
    {
        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => StringEscaping.Unescape(quoted));
    }

    [Theory]
    [InlineData("'incomplete\\")]
    [InlineData("'bad\\x'")]
    [InlineData("'invalid\\q'")]
    public void Unescape_InvalidEscape_ThrowsSyntaxException(string quoted)
    {
        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => StringEscaping.Unescape(quoted));
    }

    [Theory]
    [InlineData("'\\u'")]
    [InlineData("'\\u123'")]
    [InlineData("'\\uGGGG'")]
    public void Unescape_InvalidUnicodeEscape_ThrowsSyntaxException(string quoted)
    {
        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => StringEscaping.Unescape(quoted));
    }

    [Fact]
    public void Unescape_HighSurrogateWithoutLow_ThrowsSyntaxException()
    {
        // Arrange - High surrogate without low surrogate
        const string quoted = "'\\uD83D'";

        // Act & Assert
        Should.Throw<JsonPathSyntaxException>(() => StringEscaping.Unescape(quoted));
    }

    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("no escapes needed", "no escapes needed")]
    public void EscapeForNormalizedPath_NoEscaping_ReturnsSameString(string value, string expected)
    {
        // Act
        var result = StringEscaping.EscapeForNormalizedPath(value);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("quote'here", "quote\\'here")]
    [InlineData("back\\slash", "back\\\\slash")]
    [InlineData("tab\there", "tab\\there")]
    [InlineData("new\nline", "new\\nline")]
    public void EscapeForNormalizedPath_SpecialCharacters_ReturnsEscaped(string value, string expected)
    {
        // Act
        var result = StringEscaping.EscapeForNormalizedPath(value);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void EscapeForNormalizedPath_ControlCharacters_ReturnsEscaped()
    {
        // Arrange - NULL character (U+0000)
        var value = "null\u0000char";

        // Act
        var result = StringEscaping.EscapeForNormalizedPath(value);

        // Assert
        result.ShouldContain("\\u0000");
    }

    [Fact]
    public void EscapeForNormalizedPath_AllEscapeTypes_ReturnsCorrectlyEscaped()
    {
        // Arrange
        const string value = "test\t\n\r'\\end";

        // Act
        var result = StringEscaping.EscapeForNormalizedPath(value);

        // Assert
        result.ShouldBe("test\\t\\n\\r\\'\\\\end");
    }

    [Fact]
    public void EscapeForNormalizedPath_Span_ReturnsCorrectly()
    {
        // Arrange
        ReadOnlySpan<char> value = "test'value".AsSpan();

        // Act
        var result = StringEscaping.EscapeForNormalizedPath(value);

        // Assert
        result.ShouldBe("test\\'value");
    }

    [Fact]
    public void Unescape_Span_ReturnsCorrectly()
    {
        // Arrange
        ReadOnlySpan<char> quoted = "'test\\nvalue'".AsSpan();

        // Act
        var result = StringEscaping.Unescape(quoted);

        // Assert
        result.ShouldBe("test\nvalue");
    }
}
