using System;
using System.Runtime.CompilerServices;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Transpiles a wildcard pattern to a regex pattern.
    /// Time Complexity: O(n) where n is the length of the wildcard pattern.
    /// Space Complexity: O(n) as we allocate a new string of length slightly larger than n.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        if (wildcard == null) throw new ArgumentNullException(nameof(wildcard));

        int newLength = 2; // ^ and $
        int wildcardLength = wildcard.Length;
        for (int i = 0; i < wildcardLength; i++)
        {
            char c = wildcard[i];
            if (c == '*') newLength += 2; // .*
            else if (c == '?') newLength += 1; // .
            else if (IsRegexChar(c)) newLength += 2; // \c
            else newLength += 1; // c
        }

#if NETSTANDARD2_0
        char[] buffer = new char[newLength];
        buffer[0] = '^';
        int writeIdx = 1;
        for (int i = 0; i < wildcardLength; i++)
        {
            char c = wildcard[i];
            if (c == '*')
            {
                buffer[writeIdx++] = '.';
                buffer[writeIdx++] = '*';
            }
            else if (c == '?')
            {
                buffer[writeIdx++] = '.';
            }
            else if (IsRegexChar(c))
            {
                buffer[writeIdx++] = '\\';
                buffer[writeIdx++] = c;
            }
            else
            {
                buffer[writeIdx++] = c;
            }
        }
        buffer[writeIdx] = '$';
        return new string(buffer, 0, newLength);
#else
        return string.Create(newLength, wildcard, (span, wc) =>
        {
            span[0] = '^';
            int writeIdx = 1;
            for (int i = 0; i < wc.Length; i++)
            {
                char c = wc[i];
                if (c == '*')
                {
                    span[writeIdx++] = '.';
                    span[writeIdx++] = '*';
                }
                else if (c == '?')
                {
                    span[writeIdx++] = '.';
                }
                else if (IsRegexChar(c))
                {
                    span[writeIdx++] = '\\';
                    span[writeIdx++] = c;
                }
                else
                {
                    span[writeIdx++] = c;
                }
            }
            span[writeIdx] = '$';
        });
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsRegexChar(char c)
    {
        switch (c)
        {
            case '\t': case '\n': case '\v': case '\f': case '\r': case ' ':
            case '#': case '$': case '(': case ')': case '+': case '.':
            case '[': case '\\': case '^': case '{': case '|':
                return true;
            default:
                return false;
        }
    }
}
