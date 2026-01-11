namespace Blazing.Json.JSONPath.Lexer;

/// <summary>
/// Represents a single token in a JSONPath query with its type, value, and position.
/// </summary>
public readonly struct JsonPathToken : IEquatable<JsonPathToken>
{
    /// <summary>
    /// Gets the type of the token.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    /// Gets the string value of the token (for literals, identifiers, etc.).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the starting position of this token in the original query string.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the length of this token in the original query string.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPathToken"/> struct.
    /// </summary>
    /// <param name="type">The token type.</param>
    /// <param name="value">The token value.</param>
    /// <param name="position">The starting position in the query.</param>
    /// <param name="length">The length of the token.</param>
    public JsonPathToken(TokenType type, string value, int position, int length)
    {
        Type = type;
        Value = value;
        Position = position;
        Length = length;
    }

    /// <summary>
    /// Creates a token with the specified type at the given position.
    /// </summary>
    /// <param name="type">The token type.</param>
    /// <param name="position">The starting position.</param>
    /// <param name="length">The length of the token.</param>
    /// <returns>A new token instance.</returns>
    public static JsonPathToken Create(TokenType type, int position, int length)
    {
        return new JsonPathToken(type, string.Empty, position, length);
    }

    /// <summary>
    /// Creates a token with the specified type and value at the given position.
    /// </summary>
    /// <param name="type">The token type.</param>
    /// <param name="value">The token value.</param>
    /// <param name="position">The starting position.</param>
    /// <param name="length">The length of the token.</param>
    /// <returns>A new token instance.</returns>
    public static JsonPathToken Create(TokenType type, string value, int position, int length)
    {
        return new JsonPathToken(type, value, position, length);
    }

    /// <inheritdoc/>
    public bool Equals(JsonPathToken other)
    {
        return Type == other.Type &&
               Value == other.Value &&
               Position == other.Position &&
               Length == other.Length;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is JsonPathToken token && Equals(token);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Value, Position, Length);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Value)
            ? $"{Type} at {Position}"
            : $"{Type}({Value}) at {Position}";
    }

    /// <summary>
    /// Determines whether two tokens are equal.
    /// </summary>
    public static bool operator ==(JsonPathToken left, JsonPathToken right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two tokens are not equal.
    /// </summary>
    public static bool operator !=(JsonPathToken left, JsonPathToken right)
    {
        return !left.Equals(right);
    }
}
