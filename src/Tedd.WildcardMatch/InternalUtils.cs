using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Converts a wildcard string (* and ?) into a regular expression pattern anchored with ^ and $.
    ///
    /// Big O Complexity:
    /// Time Complexity: O(N) where N is the length of the escaped wildcard string. We iterate over the characters in a single pass.
    /// Space Complexity: O(N) since we allocate a buffer (either on the stack via stackalloc for strings <= 1024 chars, or on the heap otherwise) of length N+2.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string StringToWildcard(string wildcard)
    {
        if (string.IsNullOrEmpty(wildcard))
        {
            return "^$";
        }

        string escaped = Regex.Escape(wildcard);
        int maxLength = escaped.Length + 2;

        if (maxLength <= 1024)
        {
            char* buffer = stackalloc char[maxLength];
            return BuildString(escaped, buffer);
        }
        else
        {
            char[] buffer = new char[maxLength];
            fixed (char* ptr = buffer)
            {
                return BuildString(escaped, ptr);
            }
        }
    }

    private static unsafe string BuildString(string escaped, char* buffer)
    {
        buffer[0] = '^';
        int pos = 1;
        for (int i = 0; i < escaped.Length; i++)
        {
            char c = escaped[i];
            if (c == '\\' && i + 1 < escaped.Length)
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
        return new string(buffer, 0, pos);
    }
}
