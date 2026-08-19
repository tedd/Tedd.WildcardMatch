using System;
using System.Runtime.CompilerServices;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Transpiles a wildcard pattern into a regular expression pattern.
    /// Time Complexity: O(N) where N is the length of the wildcard string.
    /// Space Complexity: O(N) where N is the length of the wildcard string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        if (wildcard == null)
        {
            throw new ArgumentNullException(nameof(wildcard));
        }

        int wildcardLength = wildcard.Length;
        int length = 2; // '^' and '$'

        for (int i = 0; i < wildcardLength; i++)
        {
            char c = wildcard[i];
            if (c == '*')
            {
                length += 2; // ".*"
            }
            else if (c == '?')
            {
                length += 1; // "."
            }
            else if (IsRegexMetacharacter(c))
            {
                length += 2; // "\c"
            }
            else
            {
                length += 1; // "c"
            }
        }

#if NETSTANDARD2_0
        char[] buffer = new char[length];
#else
        char[] buffer = System.Buffers.ArrayPool<char>.Shared.Rent(length);
#endif

        try
        {
            buffer[0] = '^';
            int pos = 1;
            for (int i = 0; i < wildcardLength; i++)
            {
                char c = wildcard[i];
                if (c == '*')
                {
                    buffer[pos++] = '.';
                    buffer[pos++] = '*';
                }
                else if (c == '?')
                {
                    buffer[pos++] = '.';
                }
                else
                {
                    if (IsRegexMetacharacter(c))
                    {
                        buffer[pos++] = '\\';
                    }
                    buffer[pos++] = c;
                }
            }
            buffer[pos] = '$';

            return new string(buffer, 0, length);
        }
        finally
        {
#if !NETSTANDARD2_0
            System.Buffers.ArrayPool<char>.Shared.Return(buffer);
#endif
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsRegexMetacharacter(char c)
    {
        switch (c)
        {
            case '\t':
            case '\n':
            case '\f':
            case '\r':
            case ' ':
            case '#':
            case '$':
            case '(':
            case ')':
            case '*':
            case '+':
            case '.':
            case '?':
            case '[':
            case '\\':
            case '^':
            case '{':
            case '|':
                return true;
            default:
                return false;
        }
    }
}
