using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    // Regex.Escape escapes: \t, \n, \f, \r, space, #, $, (, ), *, +, ., ?, [, \, ^, {, |
    // Big O Complexity:
    // Time: O(n) where n is the length of the input wildcard string (we perform a single pass to calculate length, and a single pass to construct the result).
    // Space: O(n) since we allocate a single string of at most 2 * n + 2 characters (via StringBuilder or string.Create).
    private static bool IsRegexChar(char ch)
    {
        return ch switch
        {
            '\t' or '\n' or '\f' or '\r' or ' ' or '#' or '$' or '(' or ')' or '*' or '+' or '.' or '?' or '[' or '\\' or '^' or '{' or '|' => true,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        if (wildcard == null)
            throw new ArgumentNullException(nameof(wildcard));

        if (wildcard.Length == 0)
            return "^$";

        // Precalculate maximum length to avoid reallocation.
        // "^" + "$" = 2
        // Max expansion per char: '*' -> ".*" (2), '?' -> "." (1), escaped char -> "\c" (2)
        // Wait, Regex.Escape escapes '*' to "\*", then Replace(@"\*", ".*") turns it into ".*".
        // And '?' to "\?", then Replace(@"\?", ".") turns it into ".".
        // Wait! The original code says:
        // Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".")
        // Regex.Escape escapes '*' into "\*", so Replace(@"\*", ".*") matches the literal string "\*".

        int len = 2;
        foreach (char c in wildcard)
        {
            if (c == '*') len += 2;
            else if (c == '?') len += 1;
            else if (IsRegexChar(c)) len += 2;
            else len += 1;
        }

#if NETSTANDARD2_0
        StringBuilder sb = new StringBuilder(len);
        sb.Append('^');
        foreach (char c in wildcard)
        {
            if (c == '*')
            {
                sb.Append(".*");
            }
            else if (c == '?')
            {
                sb.Append('.');
            }
            else if (IsRegexChar(c))
            {
                sb.Append('\\');
                sb.Append(c);
            }
            else
            {
                sb.Append(c);
            }
        }
        sb.Append('$');
        return sb.ToString();
#else
        return string.Create(len, wildcard, (span, w) =>
        {
            span[0] = '^';
            int pos = 1;
            foreach (char c in w)
            {
                if (c == '*')
                {
                    span[pos++] = '.';
                    span[pos++] = '*';
                }
                else if (c == '?')
                {
                    span[pos++] = '.';
                }
                else if (IsRegexChar(c))
                {
                    span[pos++] = '\\';
                    span[pos++] = c;
                }
                else
                {
                    span[pos++] = c;
                }
            }
            span[pos] = '$';
        });
#endif
    }
}
