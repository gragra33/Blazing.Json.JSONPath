using Blazing.Json.JSONPath.Exceptions;
using Blazing.Json.JSONPath.Lexer;

namespace Blazing.Json.JSONPath.Parser;

/// <summary>
/// Manages the state and position during JSONPath query parsing.
/// Provides token navigation and lookahead capabilities.
/// </summary>
internal sealed class ParserContext
{
    private readonly IReadOnlyList<JsonPathToken> _tokens;
    private int _position;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParserContext"/> class.
    /// </summary>
    /// <param name="tokens">The tokens to parse.</param>
    public ParserContext(IReadOnlyList<JsonPathToken> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    /// <summary>
    /// Gets the current token.
    /// </summary>
    public JsonPathToken Current => _position < _tokens.Count ? _tokens[_position] : JsonPathToken.Create(TokenType.EndOfInput, _position, 0);

    /// <summary>
    /// Gets the current position in the token stream.
    /// </summary>
    public int Position => _position;

    /// <summary>
    /// Gets a value indicating whether we've reached the end of the token stream.
    /// </summary>
    public bool IsAtEnd => Current.Type == TokenType.EndOfInput;

    /// <summary>
    /// Peeks at the next token without consuming it.
    /// </summary>
    /// <param name="offset">The offset from the current position (default is 1).</param>
    /// <returns>The token at the specified offset.</returns>
    public JsonPathToken Peek(int offset = 1)
    {
        var peekPosition = _position + offset;
        return peekPosition < _tokens.Count 
            ? _tokens[peekPosition] 
            : JsonPathToken.Create(TokenType.EndOfInput, _tokens.Count, 0);
    }

    /// <summary>
    /// Advances to the next token and returns the previous current token.
    /// </summary>
    /// <returns>The token that was current before advancing.</returns>
    public JsonPathToken Advance()
    {
        var token = Current;
        if (!IsAtEnd)
        {
            _position++;
        }
        return token;
    }

    /// <summary>
    /// Checks if the current token matches the specified type.
    /// </summary>
    /// <param name="type">The token type to check.</param>
    /// <returns>True if the current token matches the type; otherwise, false.</returns>
    public bool Check(TokenType type)
    {
        return Current.Type == type;
    }

    /// <summary>
    /// Checks if the current token matches any of the specified types.
    /// </summary>
    /// <param name="types">The token types to check.</param>
    /// <returns>True if the current token matches any of the types; otherwise, false.</returns>
    public bool Check(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Current.Type == type)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Consumes the current token if it matches the specified type.
    /// </summary>
    /// <param name="type">The expected token type.</param>
    /// <returns>True if the token was consumed; otherwise, false.</returns>
    public bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Consumes the current token if it matches any of the specified types.
    /// </summary>
    /// <param name="types">The expected token types.</param>
    /// <returns>True if a token was consumed; otherwise, false.</returns>
    public bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Consumes the current token if it matches the expected type, or throws an exception.
    /// </summary>
    /// <param name="type">The expected token type.</param>
    /// <param name="message">The error message if the token doesn't match.</param>
    /// <returns>The consumed token.</returns>
    /// <exception cref="JsonPathSyntaxException">Thrown when the current token doesn't match the expected type.</exception>
    public JsonPathToken Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw new JsonPathSyntaxException(
            $"{message}. Expected {type}, but found {Current.Type}",
            Current.Position);
    }

    /// <summary>
    /// Creates a syntax exception at the current position.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new syntax exception.</returns>
    public JsonPathSyntaxException Error(string message)
    {
        return new JsonPathSyntaxException(message, Current.Position);
    }
}
