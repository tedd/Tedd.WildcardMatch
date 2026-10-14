using System;
using System.Text.RegularExpressions;

namespace Tedd;

internal static class InternalUtils
{
    private static readonly char[] WildcardCharacters = { '*', '?' };
    private static readonly bool[] EscapedCharacters = CreateEscapedCharacters();

    private static bool[] CreateEscapedCharacters()
    {
        var characters = new bool[128];
        foreach (char character in "\t\n\f\r #$()+.[\\^{|")
            characters[character] = true;
        return characters;
    }

    /// <summary>
    /// Transpiles a wildcard pattern string into its corresponding regex pattern string.
    /// Time Complexity: O(N) where N is the length of the input wildcard string.
    /// Space Complexity: O(N) for the returned string allocation.
    /// </summary>
    public static string StringToWildcard(string wildcard)
    {
        if (wildcard is null)
#pragma warning disable CA2208 // Preserve the existing Regex.Escape exception metadata.
            throw new ArgumentNullException("str"); // Preserve Regex.Escape's parameter name.
#pragma warning restore CA2208
        if (wildcard.Length == 0)
            return "^$";

        int firstWildcard = wildcard.IndexOfAny(WildcardCharacters);

        // Fast paths logic taken from the original method
        if (firstWildcard < 0)
        {
            // Regex.Escape optimization.
            return "^" + Regex.Escape(wildcard) + "$";
        }

        if (wildcard.Length >= 128)
        {
            int count = 0;
            int denseCount = wildcard.Length / 32 + 1;
            int next = firstWildcard;
            bool hasStar = false;
            bool hasQuestion = false;
            do
            {
                hasStar |= wildcard[next] == '*';
                hasQuestion |= wildcard[next] == '?';
                count++;
                if (count == denseCount)
                    break;
                next = wildcard.IndexOfAny(WildcardCharacters, next + 1);
            } while (next >= 0);

            if (count < denseCount)
            {
                string escaped = Regex.Escape(wildcard);
                if (hasStar)
                    escaped = escaped.Replace(@"\*", ".*");
                if (hasQuestion)
                    escaped = escaped.Replace(@"\?", ".");
                return "^" + escaped + "$";
            }
        }

        int exactLength = 2; // Anchors ^ and $
        foreach (char character in wildcard)
        {
            if (character == '*') exactLength += 2; // .*
            else if (character < EscapedCharacters.Length && EscapedCharacters[character]) exactLength += 2; // \c
            else exactLength++;
        }

#if NETSTANDARD2_0
        var buffer = new char[exactLength];
        int position = 0;
        buffer[position++] = '^';
        foreach (char character in wildcard)
        {
            if (character == '*')
            {
                buffer[position++] = '.';
                buffer[position++] = '*';
            }
            else if (character == '?')
            {
                buffer[position++] = '.';
            }
            else
            {
                if (character < EscapedCharacters.Length && EscapedCharacters[character])
                {
                    buffer[position++] = '\\';
                }
                buffer[position++] = character switch
                {
                    '\t' => 't',
                    '\n' => 'n',
                    '\r' => 'r',
                    '\f' => 'f',
                    _ => character
                };
            }
        }
        buffer[position++] = '$';
        return new string(buffer);
#else
        return string.Create(exactLength, wildcard, static (span, w) =>
        {
            int position = 0;
            span[position++] = '^';
            foreach (char character in w)
            {
                if (character == '*')
                {
                    span[position++] = '.';
                    span[position++] = '*';
                }
                else if (character == '?')
                {
                    span[position++] = '.';
                }
                else
                {
                    if (character < EscapedCharacters.Length && EscapedCharacters[character])
                    {
                        span[position++] = '\\';
                    }
                    span[position++] = character switch
                    {
                        '\t' => 't',
                        '\n' => 'n',
                        '\r' => 'r',
                        '\f' => 'f',
                        _ => character
                    };
                }
            }
            span[position++] = '$';
        });
#endif
    }
}
