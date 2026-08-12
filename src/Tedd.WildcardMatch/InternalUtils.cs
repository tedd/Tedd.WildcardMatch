using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Converts a wildcard string to a regex pattern.
    /// Big-O Complexity: O(N) Time, O(N) Space, where N is the length of the escaped wildcard string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        string escaped = Regex.Escape(wildcard);
        int escapedLength = escaped.Length;
        int maxBufferLen = escapedLength + 2;

#if NETSTANDARD2_0
        char[] buffer = new char[maxBufferLen];
#else
        Span<char> buffer = maxBufferLen <= 512
            ? stackalloc char[maxBufferLen]
            : new char[maxBufferLen];
#endif

        buffer[0] = '^';
        int pos = 1;
        for (int i = 0; i < escapedLength; i++)
        {
            char c = escaped[i];
            if (c == '\\' && i + 1 < escapedLength)
            {
                char next = escaped[i + 1];
                if (next == '*')
                {
                    buffer[pos++] = '.';
                    buffer[pos++] = '*';
                    i++;
                    continue;
                }
                if (next == '?')
                {
                    buffer[pos++] = '.';
                    i++;
                    continue;
                }
            }
            buffer[pos++] = c;
        }
        buffer[pos++] = '$';

#if NETSTANDARD2_0
        return new string(buffer, 0, pos);
#else
        return new string(buffer.Slice(0, pos));
#endif
    }
}
