using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Tedd;

    internal static class InternalUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
    /// Transpiles a wildcard string into a Regex string.
    /// Time Complexity: O(n)
    /// Space Complexity: O(n) where n is the length of the string.
    /// </summary>
        public static string StringToWildcard(string wildcard)
        {
            if (wildcard == null) throw new ArgumentNullException(nameof(wildcard));

            int len = 2; // ^ and $
            for (int i = 0; i < wildcard.Length; i++)
            {
                char c = wildcard[i];
                if (c == '*' || c == '\t' || c == '\n' || c == '\f' || c == '\r' || c == ' ' || c == '#' || c == '$' || c == '(' || c == ')' || c == '+' || c == '.' || c == '[' || c == '\\' || c == '^' || c == '{' || c == '|') len += 2;
                else len += 1;
            }

            char[] arr = new char[len];
            arr[0] = '^';
            int pos = 1;
            for (int i = 0; i < wildcard.Length; i++)
            {
                char c = wildcard[i];
                if (c == '*') { arr[pos++] = '.'; arr[pos++] = '*'; }
                else if (c == '?') { arr[pos++] = '.'; }
                else if (c == '\t') { arr[pos++] = '\\'; arr[pos++] = 't'; }
                else if (c == '\n') { arr[pos++] = '\\'; arr[pos++] = 'n'; }
                else if (c == '\f') { arr[pos++] = '\\'; arr[pos++] = 'f'; }
                else if (c == '\r') { arr[pos++] = '\\'; arr[pos++] = 'r'; }
                else if (c == ' ' || c == '#' || c == '$' || c == '(' || c == ')' || c == '+' || c == '.' || c == '[' || c == '\\' || c == '^' || c == '{' || c == '|')
                {
                    arr[pos++] = '\\'; arr[pos++] = c;
                }
                else
                {
                    arr[pos++] = c;
                }
            }
            arr[pos] = '$';
            return new string(arr);
        }
    }
