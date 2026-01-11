using System.Globalization;
using System.Text;
using Blazing.Json.JSONPath.Exceptions;

namespace Blazing.Json.JSONPath.Utilities;

/// <summary>
/// Handles JSONPath string literal escaping and unescaping per RFC 9535.
/// </summary>
public static class StringEscaping
{
    /// <summary>
    /// Unescapes a quoted string literal from a JSONPath query.
    /// Handles: \b \t \n \f \r \" \' \/ \\ \uXXXX
    /// </summary>
    /// <param name="quoted">The quoted string literal (including quotes).</param>
    /// <returns>The unescaped string value.</returns>
    /// <exception cref="JsonPathSyntaxException">Thrown when escape sequences are invalid.</exception>
    public static string Unescape(string quoted)
    {
        return Unescape(quoted.AsSpan());
    }

    /// <summary>
    /// Unescapes a quoted string literal span from a JSONPath query.
    /// </summary>
    /// <param name="quoted">The quoted string literal span (including quotes).</param>
    /// <returns>The unescaped string value.</returns>
    /// <exception cref="JsonPathSyntaxException">Thrown when escape sequences are invalid.</exception>
    public static string Unescape(ReadOnlySpan<char> quoted)
    {
        if (quoted.Length < 2)
            throw new JsonPathSyntaxException("String literal must have opening and closing quotes");

        // Verify quotes match
        char openQuote = quoted[0];
        char closeQuote = quoted[^1];

        if ((openQuote != '\'' && openQuote != '"') || openQuote != closeQuote)
            throw new JsonPathSyntaxException("String literal must have matching quotes");

        // Remove quotes
        ReadOnlySpan<char> inner = quoted[1..^1];

        // Fast path: no escapes
        if (!inner.Contains('\\'))
            return inner.ToString();

        // Process escape sequences
        var sb = new StringBuilder(inner.Length);

        for (int i = 0; i < inner.Length; i++)
        {
            if (inner[i] != '\\')
            {
                sb.Append(inner[i]);
                continue;
            }

            i++; // Consume backslash
            if (i >= inner.Length)
                throw new JsonPathSyntaxException("Incomplete escape sequence at end of string");

            switch (inner[i])
            {
                case 'b':
                    sb.Append('\b');
                    break;
                case 't':
                    sb.Append('\t');
                    break;
                case 'n':
                    sb.Append('\n');
                    break;
                case 'f':
                    sb.Append('\f');
                    break;
                case 'r':
                    sb.Append('\r');
                    break;
                case '"':
                    sb.Append('"');
                    break;
                case '\'':
                    sb.Append('\'');
                    break;
                case '/':
                    sb.Append('/');
                    break;
                case '\\':
                    sb.Append('\\');
                    break;
                case 'u':
                    // \uXXXX - Unicode escape
                    if (i + 4 >= inner.Length)
                        throw new JsonPathSyntaxException("Incomplete \\uXXXX escape sequence");

                    ReadOnlySpan<char> hex = inner.Slice(i + 1, 4);
                    if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort codePoint))
                        throw new JsonPathSyntaxException($"Invalid \\uXXXX escape sequence: \\u{hex}");

                    // Handle surrogate pairs
                    if (codePoint >= 0xD800 && codePoint <= 0xDBFF)
                    {
                        // High surrogate - expect low surrogate next
                        i += 4;
                        if (i + 6 >= inner.Length || inner[i + 1] != '\\' || inner[i + 2] != 'u')
                            throw new JsonPathSyntaxException("High surrogate not followed by low surrogate");

                        ReadOnlySpan<char> lowHex = inner.Slice(i + 3, 4);
                        if (!ushort.TryParse(lowHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort low))
                            throw new JsonPathSyntaxException($"Invalid low surrogate: \\u{lowHex}");

                        if (low < 0xDC00 || low > 0xDFFF)
                            throw new JsonPathSyntaxException("Invalid low surrogate range");

                        // Combine surrogates
                        int codePointValue = 0x10000 + ((codePoint - 0xD800) << 10) + (low - 0xDC00);
                        sb.Append(char.ConvertFromUtf32(codePointValue));
                        i += 6;
                    }
                    else
                    {
                        sb.Append((char)codePoint);
                        i += 4;
                    }
                    break;

                default:
                    throw new JsonPathSyntaxException($"Invalid escape sequence: \\{inner[i]}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes a string for use in a normalized path.
    /// Only escapes: \b \t \n \f \r \' \\ and control characters (U+0000-U+001F).
    /// </summary>
    /// <param name="value">The string value to escape.</param>
    /// <returns>The escaped string suitable for normalized path.</returns>
    public static string EscapeForNormalizedPath(string value)
    {
        return EscapeForNormalizedPath(value.AsSpan());
    }

    /// <summary>
    /// Escapes a string span for use in a normalized path.
    /// </summary>
    /// <param name="value">The string value span to escape.</param>
    /// <returns>The escaped string suitable for normalized path.</returns>
    public static string EscapeForNormalizedPath(ReadOnlySpan<char> value)
    {
        // Fast path: check if escaping is needed
        bool needsEscaping = false;
        foreach (char c in value)
        {
            if (NeedsEscaping(c))
            {
                needsEscaping = true;
                break;
            }
        }

        if (!needsEscaping)
            return value.ToString();

        // Escape characters
        var sb = new StringBuilder(value.Length + 10);

        foreach (char c in value)
        {
            switch (c)
            {
                case '\b':
                    sb.Append(@"\b");
                    break;
                case '\t':
                    sb.Append(@"\t");
                    break;
                case '\n':
                    sb.Append(@"\n");
                    break;
                case '\f':
                    sb.Append(@"\f");
                    break;
                case '\r':
                    sb.Append(@"\r");
                    break;
                case '\'':
                    sb.Append(@"\'");
                    break;
                case '\\':
                    sb.Append(@"\\");
                    break;
                default:
                    // Escape control characters U+0000-U+001F (except those above)
                    if (c < 0x20)
                    {
                        sb.Append(@"\u");
                        sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines if a character needs escaping in a normalized path.
    /// </summary>
    private static bool NeedsEscaping(char c)
    {
        return c switch
        {
            '\b' or '\t' or '\n' or '\f' or '\r' or '\'' or '\\' => true,
            _ => c < 0x20 // Control characters
        };
    }
}
