using System;
using System.Runtime.CompilerServices;

#if NETSTANDARD2_0
#else
using System.Buffers;
#endif

namespace Tedd
{
    internal static class InternalUtils
    {
        /// <summary>
        /// Big O Notation:
        /// Time Complexity: O(N) where N is the length of the wildcard string. We iterate through the string once.
        /// Space Complexity: O(N) to store the transposed string. We use ArrayPool/stackalloc to avoid intermediate allocations, creating only the final string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string StringToWildcard(string wildcard)
        {
            if (wildcard == null)
            {
                throw new ArgumentNullException(nameof(wildcard));
            }

            int length = wildcard.Length;
            // Every character could potentially be escaped (e.g. '\t', '\n' become 2 chars)
            // Plus '^' and '$' at the ends.
            int maxLength = length * 2 + 2;

#if NETSTANDARD2_0
            char[] buffer = new char[maxLength];
#else
            char[]? rentedArray = null;
            Span<char> buffer = maxLength <= 256 ? stackalloc char[maxLength] : (rentedArray = ArrayPool<char>.Shared.Rent(maxLength));
#endif

            buffer[0] = '^';
            int pos = 1;

            for (int j = 0; j < length; j++)
            {
                char c = wildcard[j];
                switch (c)
                {
                    case '*':
                        buffer[pos++] = '.';
                        buffer[pos++] = '*';
                        break;
                    case '?':
                        buffer[pos++] = '.';
                        break;
                    case ' ':
                    case '#':
                    case '$':
                    case '(':
                    case ')':
                    case '+':
                    case '.':
                    case '[':
                    case '\\':
                    case '^':
                    case '{':
                    case '|':
                        buffer[pos++] = '\\';
                        buffer[pos++] = c;
                        break;
                    case '\t':
                        buffer[pos++] = '\\';
                        buffer[pos++] = 't';
                        break;
                    case '\n':
                        buffer[pos++] = '\\';
                        buffer[pos++] = 'n';
                        break;
                    case '\r':
                        buffer[pos++] = '\\';
                        buffer[pos++] = 'r';
                        break;
                    case '\f':
                        buffer[pos++] = '\\';
                        buffer[pos++] = 'f';
                        break;
                    default:
                        buffer[pos++] = c;
                        break;
                }
            }

            buffer[pos++] = '$';

#if NETSTANDARD2_0
            string s = new string(buffer, 0, pos);
#else
            string s = new string(buffer.Slice(0, pos));
            if (rentedArray != null)
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
#endif

            return s;
        }
    }
}
