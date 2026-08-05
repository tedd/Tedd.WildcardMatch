using System;
using System.Runtime.CompilerServices;
using System.Buffers;

namespace Tedd;

internal static class InternalUtils
{
    /// <summary>
    /// Transpiles a wildcard pattern to a Regex pattern.
    /// Time Complexity: O(n) where n is the length of the wildcard string.
    /// Space Complexity: O(n) auxiliary space using stackalloc pointer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringToWildcard(string wildcard)
    {
        if (wildcard == null) throw new ArgumentNullException(nameof(wildcard));

        int maxLen = wildcard.Length * 2 + 2;
        char[]? poolArray = null;

        Span<char> buffer = maxLen <= 256
            ? stackalloc char[maxLen]
            : (poolArray = ArrayPool<char>.Shared.Rent(maxLen));

        int pos = 0;
        buffer[pos++] = '^';

        for (int i = 0; i < wildcard.Length; i++)
        {
            char c = wildcard[i];
            switch (c)
            {
                case '*':
                    buffer[pos++] = '.';
                    buffer[pos++] = '*';
                    break;
                case '?':
                    buffer[pos++] = '.';
                    break;
                case '\t':
                    buffer[pos++] = '\\';
                    buffer[pos++] = 't';
                    break;
                case '\n':
                    buffer[pos++] = '\\';
                    buffer[pos++] = 'n';
                    break;
                case '\f':
                    buffer[pos++] = '\\';
                    buffer[pos++] = 'f';
                    break;
                case '\r':
                    buffer[pos++] = '\\';
                    buffer[pos++] = 'r';
                    break;
                case ' ': case '#': case '$': case '(': case ')': case '+': case '.': case '[': case '\\': case '^': case '{': case '|':
                    buffer[pos++] = '\\';
                    buffer[pos++] = c;
                    break;
                default:
                    buffer[pos++] = c;
                    break;
            }
        }

        buffer[pos++] = '$';

        string result = buffer.Slice(0, pos).ToString();

        if (poolArray != null)
        {
            ArrayPool<char>.Shared.Return(poolArray);
        }

        return result;
    }
}
