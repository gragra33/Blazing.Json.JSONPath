using System.Globalization;
using System.Text.RegularExpressions;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Lexer;

/// <summary>
/// Tokenizes JSONPath query strings using optimized pattern matching.
/// Implements RFC 9535 ABNF grammar for lexical analysis.
/// </summary>
public static partial class JsonPathLexer
{
    // Integer and floating-point: -?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?
    [GeneratedRegex(@"^-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();

    // Function name: [a-z][a-z0-9_]*(?=\()
    [GeneratedRegex(@"^[a-z][a-z0-9_]*(?=\()", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FunctionNamePattern();

    // Member name shorthand: [A-Za-z_\u0080-\uD7FF\uE000-\uFFFF][A-Za-z0-9_\u0080-\uD7FF\uE000-\uFFFF]*
    [GeneratedRegex(@"^[A-Za-z_\u0080-\uD7FF\uE000-\uFFFF][A-Za-z0-9_\u0080-\uD7FF\uE000-\uFFFF]*", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex MemberNamePattern();

    /// <summary>
    /// Tokenizes a JSONPath query string into a sequence of tokens.
    /// </summary>
    /// <param name="query">The JSONPath query string to tokenize.</param>
    /// <returns>A sequence of tokens representing the query.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="JsonPathSyntaxException">Thrown when the query contains invalid syntax.</exception>
    public static IReadOnlyList<JsonPathToken> Tokenize(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return TokenizeCore(query);
    }

    /// <summary>
    /// Tokenizes a JSONPath query span into a sequence of tokens.
    /// </summary>
    /// <param name="query">The JSONPath query span to tokenize.</param>
    /// <returns>A sequence of tokens representing the query.</returns>
    /// <exception cref="JsonPathSyntaxException">Thrown when the query contains invalid syntax.</exception>
    public static IReadOnlyList<JsonPathToken> Tokenize(ReadOnlySpan<char> query)
    {
        return TokenizeCore(query.ToString());
    }

    private static IReadOnlyList<JsonPathToken> TokenizeCore(string query)
    {
        var tokens = new List<JsonPathToken>();
        int position = 0;

        while (position < query.Length)
        {
            // Skip whitespace
            if (char.IsWhiteSpace(query[position]))
            {
                position++;
                continue;
            }

            ReadOnlySpan<char> remaining = query.AsSpan(position);
            int consumed = 0;
            TokenType type = TokenType.Invalid;
            string? value = null;

            // Try to match tokens in order of precedence
            // Multi-character operators first to avoid ambiguity
            if (TryMatchDoubleDot(remaining, out consumed))
            {
                type = TokenType.DoubleDot;
            }
            else if (TryMatchAnd(remaining, out consumed))
            {
                type = TokenType.And;
            }
            else if (TryMatchOr(remaining, out consumed))
            {
                type = TokenType.Or;
            }
            else if (TryMatchEqual(remaining, out consumed))
            {
                type = TokenType.Equal;
            }
            else if (TryMatchNotEqual(remaining, out consumed))
            {
                type = TokenType.NotEqual;
            }
            else if (TryMatchLessEqual(remaining, out consumed))
            {
                type = TokenType.LessEqual;
            }
            else if (TryMatchGreaterEqual(remaining, out consumed))
            {
                type = TokenType.GreaterEqual;
            }
            // Keywords (before identifiers)
            else if (TryMatchTrue(remaining, out consumed))
            {
                type = TokenType.True;
                value = "true";
            }
            else if (TryMatchFalse(remaining, out consumed))
            {
                type = TokenType.False;
                value = "false";
            }
            else if (TryMatchNull(remaining, out consumed))
            {
                type = TokenType.Null;
                value = "null";
            }
            // Function name (before member name)
            else if (TryMatchFunctionName(remaining, out consumed, out value))
            {
                type = TokenType.FunctionName;
            }
            // String literals
            else if (TryMatchSingleQuotedString(remaining, out consumed, out value))
            {
                type = TokenType.StringLiteral;
            }
            else if (TryMatchDoubleQuotedString(remaining, out consumed, out value))
            {
                type = TokenType.StringLiteral;
            }
            // Numbers (floating-point or integer)
            else if (TryMatchNumber(remaining, out consumed, out value))
            {
                type = TokenType.Integer; // Use Integer token type for all numbers
            }
            // Member name shorthand
            else if (TryMatchMemberName(remaining, out consumed, out value))
            {
                type = TokenType.MemberName;
            }
            // Single character tokens
            else if (TryMatchRootIdentifier(remaining, out consumed))
            {
                type = TokenType.RootIdentifier;
            }
            else if (TryMatchCurrentIdentifier(remaining, out consumed))
            {
                type = TokenType.CurrentIdentifier;
            }
            else if (TryMatchLeftBracket(remaining, out consumed))
            {
                type = TokenType.LeftBracket;
            }
            else if (TryMatchRightBracket(remaining, out consumed))
            {
                type = TokenType.RightBracket;
            }
            else if (TryMatchDot(remaining, out consumed))
            {
                type = TokenType.Dot;
            }
            else if (TryMatchWildcard(remaining, out consumed))
            {
                type = TokenType.Wildcard;
            }
            else if (TryMatchColon(remaining, out consumed))
            {
                type = TokenType.Colon;
            }
            else if (TryMatchQuestion(remaining, out consumed))
            {
                type = TokenType.Question;
            }
            else if (TryMatchNot(remaining, out consumed))
            {
                type = TokenType.Not;
            }
            else if (TryMatchLess(remaining, out consumed))
            {
                type = TokenType.Less;
            }
            else if (TryMatchGreater(remaining, out consumed))
            {
                type = TokenType.Greater;
            }
            else if (TryMatchLeftParen(remaining, out consumed))
            {
                type = TokenType.LeftParen;
            }
            else if (TryMatchRightParen(remaining, out consumed))
            {
                type = TokenType.RightParen;
            }
            else if (TryMatchComma(remaining, out consumed))
            {
                type = TokenType.Comma;
            }
            else
            {
                throw new JsonPathSyntaxException($"Invalid token '{remaining[0]}'", position);
            }

            tokens.Add(value != null
                ? JsonPathToken.Create(type, value, position, consumed)
                : JsonPathToken.Create(type, position, consumed));

            position += consumed;
        }

        tokens.Add(JsonPathToken.Create(TokenType.EndOfInput, position, 0));
        return tokens;
    }

    // Root identifier: $
    [GeneratedRegex(@"^\$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex RootIdentifierPattern();

    private static bool TryMatchRootIdentifier(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '$')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    // Current node identifier: @
    [GeneratedRegex(@"^@", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex CurrentIdentifierPattern();

    private static bool TryMatchCurrentIdentifier(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '@')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    // Double-quoted string: "string" with escape sequences
    [GeneratedRegex(@"^""(?:[^""\\]|\\[""\\\/bfnrt]|\\u[0-9A-Fa-f]{4})*""", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex DoubleQuotedStringPattern();

    private static bool TryMatchDoubleQuotedString(ReadOnlySpan<char> input, out int consumed, out string? value)
    {
        if (input.Length > 0 && input[0] == '"')
        {
            // Find closing quote
            int i = 1;
            while (i < input.Length)
            {
                if (input[i] == '\\' && i + 1 < input.Length)
                {
                    i += 2; // Skip escaped character
                    continue;
                }
                if (input[i] == '"')
                {
                    consumed = i + 1;
                    value = input[..consumed].ToString();
                    return true;
                }
                i++;
            }
            throw new JsonPathSyntaxException("Unterminated string literal", 0);
        }
        consumed = 0;
        value = null;
        return false;
    }

    // Single-quoted string: 'string' with escape sequences
    [GeneratedRegex(@"^'(?:[^'\\]|\\['\\\/bfnrt]|\\u[0-9A-Fa-f]{4})*'", 
        RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex SingleQuotedStringPattern();

    private static bool TryMatchSingleQuotedString(ReadOnlySpan<char> input, out int consumed, out string? value)
    {
        if (input.Length > 0 && input[0] == '\'')
        {
            // Find closing quote
            int i = 1;
            while (i < input.Length)
            {
                if (input[i] == '\\' && i + 1 < input.Length)
                {
                    i += 2; // Skip escaped character
                    continue;
                }
                if (input[i] == '\'')
                {
                    consumed = i + 1;
                    value = input[..consumed].ToString();
                    return true;
                }
                i++;
            }
            throw new JsonPathSyntaxException("Unterminated string literal", 0);
        }
        consumed = 0;
        value = null;
        return false;
    }

    // Keywords
    [GeneratedRegex(@"^true\b", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex TruePattern();

    private static bool TryMatchTrue(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 4 && input[..4].SequenceEqual("true".AsSpan()))
        {
            // Check word boundary
            if (input.Length == 4 || !char.IsLetterOrDigit(input[4]))
            {
                consumed = 4;
                return true;
            }
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^false\b", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FalsePattern();

    private static bool TryMatchFalse(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 5 && input[..5].SequenceEqual("false".AsSpan()))
        {
            // Check word boundary
            if (input.Length == 5 || !char.IsLetterOrDigit(input[5]))
            {
                consumed = 5;
                return true;
            }
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^null\b", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NullPattern();

    private static bool TryMatchNull(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 4 && input[..4].SequenceEqual("null".AsSpan()))
        {
            // Check word boundary
            if (input.Length == 4 || !char.IsLetterOrDigit(input[4]))
            {
                consumed = 4;
                return true;
            }
        }
        consumed = 0;
        return false;
    }

    // Operators (multi-character first)
    [GeneratedRegex(@"^\.\.", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex DoubleDotPattern();

    private static bool TryMatchDoubleDot(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '.' && input[1] == '.')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^&&", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex AndPattern();

    private static bool TryMatchAnd(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '&' && input[1] == '&')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^\|\|", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex OrPattern();

    private static bool TryMatchOr(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '|' && input[1] == '|')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^==", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EqualPattern();

    private static bool TryMatchEqual(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '=' && input[1] == '=')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^!=", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex NotEqualPattern();

    private static bool TryMatchNotEqual(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '!' && input[1] == '=')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^<=", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex LessEqualPattern();

    private static bool TryMatchLessEqual(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '<' && input[1] == '=')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    [GeneratedRegex(@"^>=", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GreaterEqualPattern();

    private static bool TryMatchGreaterEqual(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length >= 2 && input[0] == '>' && input[1] == '=')
        {
            consumed = 2;
            return true;
        }
        consumed = 0;
        return false;
    }

    // Single character tokens
    private static bool TryMatchLeftBracket(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '[')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchRightBracket(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == ']')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchDot(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '.')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchWildcard(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '*')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchColon(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == ':')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchQuestion(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '?')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchNot(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '!')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchLess(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '<')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchGreater(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '>')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchLeftParen(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == '(')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchRightParen(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == ')')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchComma(ReadOnlySpan<char> input, out int consumed)
    {
        if (input.Length > 0 && input[0] == ',')
        {
            consumed = 1;
            return true;
        }
        consumed = 0;
        return false;
    }

    private static bool TryMatchNumber(ReadOnlySpan<char> input, out int consumed, out string? value)
    {
        var match = NumberPattern().Match(input.ToString());
        if (match.Success && match.Index == 0)
        {
            consumed = match.Length;
            value = match.Value;
            return true;
        }
        consumed = 0;
        value = null;
        return false;
    }

    private static bool TryMatchFunctionName(ReadOnlySpan<char> input, out int consumed, out string? value)
    {
        var match = FunctionNamePattern().Match(input.ToString());
        if (match.Success && match.Index == 0)
        {
            consumed = match.Length;
            value = match.Value;
            return true;
        }
        consumed = 0;
        value = null;
        return false;
    }

    private static bool TryMatchMemberName(ReadOnlySpan<char> input, out int consumed, out string? value)
    {
        var match = MemberNamePattern().Match(input.ToString());
        if (match.Success && match.Index == 0)
        {
            consumed = match.Length;
            value = match.Value;
            return true;
        }
        consumed = 0;
        value = null;
        return false;
    }
}
