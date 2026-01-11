namespace Blazing.Json.JSONPath.Lexer;

/// <summary>
/// Represents the type of a JSONPath token as defined in RFC 9535.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// Root identifier: $
    /// </summary>
    RootIdentifier,

    /// <summary>
    /// Current node identifier: @
    /// </summary>
    CurrentIdentifier,

    /// <summary>
    /// Left bracket: [
    /// </summary>
    LeftBracket,

    /// <summary>
    /// Right bracket: ]
    /// </summary>
    RightBracket,

    /// <summary>
    /// Dot: .
    /// </summary>
    Dot,

    /// <summary>
    /// Double dot (descendant segment): ..
    /// </summary>
    DoubleDot,

    /// <summary>
    /// Wildcard selector: *
    /// </summary>
    Wildcard,

    /// <summary>
    /// String literal: 'name' or "name"
    /// </summary>
    StringLiteral,

    /// <summary>
    /// Integer: 123, -5
    /// </summary>
    Integer,

    /// <summary>
    /// Colon (used in slicing): :
    /// </summary>
    Colon,

    /// <summary>
    /// Question mark (filter selector): ?
    /// </summary>
    Question,

    /// <summary>
    /// Logical AND operator: &amp;&amp;
    /// </summary>
    And,

    /// <summary>
    /// Logical OR operator: ||
    /// </summary>
    Or,

    /// <summary>
    /// Logical NOT operator: !
    /// </summary>
    Not,

    /// <summary>
    /// Equality operator: ==
    /// </summary>
    Equal,

    /// <summary>
    /// Inequality operator: !=
    /// </summary>
    NotEqual,

    /// <summary>
    /// Less than operator: &lt;
    /// </summary>
    Less,

    /// <summary>
    /// Less than or equal operator: &lt;=
    /// </summary>
    LessEqual,

    /// <summary>
    /// Greater than operator: &gt;
    /// </summary>
    Greater,

    /// <summary>
    /// Greater than or equal operator: &gt;=
    /// </summary>
    GreaterEqual,

    /// <summary>
    /// Left parenthesis: (
    /// </summary>
    LeftParen,

    /// <summary>
    /// Right parenthesis: )
    /// </summary>
    RightParen,

    /// <summary>
    /// Comma: ,
    /// </summary>
    Comma,

    /// <summary>
    /// Function name: length, count, match, search, value
    /// </summary>
    FunctionName,

    /// <summary>
    /// Boolean literal: true
    /// </summary>
    True,

    /// <summary>
    /// Boolean literal: false
    /// </summary>
    False,

    /// <summary>
    /// Null literal: null
    /// </summary>
    Null,

    /// <summary>
    /// Member name (shorthand dot notation without quotes)
    /// </summary>
    MemberName,

    /// <summary>
    /// End of input
    /// </summary>
    EndOfInput,

    /// <summary>
    /// Invalid token
    /// </summary>
    Invalid
}
