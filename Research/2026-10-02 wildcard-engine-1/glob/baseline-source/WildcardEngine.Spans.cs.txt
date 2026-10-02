using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Tedd;

internal static partial class WildcardEngine
{
    // H-031: representation-specific loops preserve the released string caller's
    // generated code. Case folding, flags and newline definitions are shared;
    // exhaustive and differential tests exercise both kernels against independent
    // oracles. Keep corresponding changes covered by the span fuzz entry points.
    // Evidence: Research/2026-10-02 wildcard-engine-1/spans/plan.md.
    internal static bool RequiresNormalization(ReadOnlySpan<char> wildcard, WildcardOptions options) =>
        ((int)options & 0x20) != 0 && NewlineOption.Supported && wildcard.IndexOf('\v') >= 0;

    // H-029: keep the shared entry visible to string/span callers so the JIT can
    // specialize literal and timeout branches without an extra forwarding frame.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsMatch(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern, WildcardOptions options,
        int behavior, bool literal, TimeSpan timeout, string? originalPattern, int prefix = -1, int suffix = -1, int leadingQuestions = -1)
    {
        var clock = new SpanMatchClock(input, originalPattern, timeout);
        bool singleline = (options & WildcardOptions.Singleline) != 0;
        bool result;
        if (((int)options & AnyNewLine) != 0)
            result = MatchUnicodeLines(input, pattern, singleline, behavior, ((int)options & Multiline) != 0, ref clock);
        else if (((int)options & Multiline) != 0)
            result = MatchLines(input, pattern, singleline, behavior, ref clock);
        else if (literal && behavior == 0)
        {
            // Bulk comparison avoids scalar traversal of long literal patterns.
            int length = pattern.Length;
            result = (input.Length == length ||
                      (input.Length > 0 && input.Length - 1 == length && input[input.Length - 1] == '\n')) &&
                     input.Slice(0, length).SequenceEqual(pattern);
        }
        else if (literal && behavior != 0 && TryAsciiLiteral(input, pattern, behavior, ref clock, out bool asciiResult))
            result = asciiResult;
        else
        {
            result = MatchWindow(input, input.Length, pattern, singleline, behavior, ref clock, prefix, suffix, leadingQuestions);
            // '$' also accepts the position before one final LF. Try the true end
            // first because patterns with literal LF may require the full input.
            if (!result && input.Length > 0 && input[input.Length - 1] == '\n')
                result = MatchWindow(input, input.Length - 1, pattern, singleline, behavior, ref clock, prefix, suffix, leadingQuestions);
        }
        clock.CheckNow();
        return result;
    }

    // H-017: ASCII letters have identical groups in invariant/non-Turkic modes.
    // Any non-ASCII unit falls back to the full Regex-compatible case mapping;
    // Turkic groups bypass this path. Keep cooperative timeout checks in the loop.
    private static bool TryAsciiLiteral(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern, int behavior, scoped ref SpanMatchClock clock, out bool result)
    {
        result = false;
        if (behavior == 3) return false;
        int length = pattern.Length;
        if (input.Length != length && !(input.Length > 0 && input.Length - 1 == length && input[input.Length - 1] == '\n')) return true;
        for (int i = 0; i < length; i++)
        {
            clock.Check();
            char a = pattern[i], b = input[i];
            if ((a | b) >= 128) return false;
            if (a != b && (uint)((a | 32) - 'a') > 25) return true;
            if (a != b && (a | 32) != (b | 32)) return true;
        }
        result = true;
        return true;
    }

    // H-018: search the complete literal run to avoid first-character false
    // positives. Spans are bounded by the current window; no substring is made.
    // The caller verifies that the star did not consume an intervening LF.
    private static int FindLiteral(ReadOnlySpan<char> input, int start, int end, ReadOnlySpan<char> pattern, int token, int patternEnd, scoped ref SpanMatchClock clock)
    {
        int next = token + 1;
        while (next < patternEnd && pattern[next] != '*' && pattern[next] != '?') { next++; clock.Check(); }
        int length = next - token;
        int offset = length == 1 ? input.Slice(start, end - start).IndexOf(pattern[token]) :
            input.Slice(start, end - start).IndexOf(pattern.Slice(token, length), StringComparison.Ordinal);
        return offset < 0 ? -1 : start + offset;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool MatchWindow(ReadOnlySpan<char> input, int end, ReadOnlySpan<char> pattern, bool singleline,
        int behavior, scoped ref SpanMatchClock clock, int preparedPrefix, int preparedSuffix, int preparedQuestions)
    {
        int text = 0, token = 0, patternEnd = pattern.Length;
        if (behavior == 0 && patternEnd >= 32)
        {
            // Long literal runs use runtime bulk comparison. Keep short patterns on
            // the scalar path to avoid extra searches. See Research/2026-10-02 wildcard-engine-1.
            int prefix = preparedPrefix >= 0 ? preparedPrefix : pattern.IndexOfAny('*', '?');
            if (prefix > 0)
            {
                if (prefix > end || !input.Slice(0, prefix).SequenceEqual(pattern.Slice(0, prefix))) return false;
                text = token = prefix;
            }
            int suffix = preparedSuffix >= 0 ? preparedSuffix : patternEnd - pattern.LastIndexOfAny('*', '?') - 1;
            if (suffix > 0)
            {
                if (suffix > end - text || !input.Slice(end - suffix, suffix).SequenceEqual(pattern.Slice(patternEnd - suffix, suffix))) return false;
                patternEnd -= suffix;
                end -= suffix;
            }
            clock.CheckNow();
        }
        if (patternEnd >= 32 && token == 0 && pattern[0] == '?' && pattern[1] == '?')
        {
            // H-014/H-020: consume a long leading question run as one bounded
            // input range. '?' still excludes LF unless Singleline is selected.
            int length = preparedQuestions;
            if (length < 0)
            {
                length = 2;
                while (length < patternEnd && pattern[length] == '?') { length++; clock.Check(); }
            }
            if (length > end || (!singleline && input.Slice(0, length).IndexOf('\n') >= 0)) return false;
            token = text = length;
        }
        // Anchor fixed prefix/suffix before retrying the interior. A suffix mismatch
        // rejects '*aaaa...b' without rescanning it at every input offset.
        while (token < patternEnd && pattern[token] != '*')
        {
            clock.Check();
            if (text == end || !Accept(pattern[token], input[text], singleline, behavior)) return false;
            token++;
            text++;
        }
        if (token == patternEnd) return text == end;
        while (pattern[patternEnd - 1] != '*')
        {
            clock.Check();
            if (text == end || !Accept(pattern[patternEnd - 1], input[end - 1], singleline, behavior)) return false;
            patternEnd--;
            end--;
        }

        int retryToken = -1, retryText = 0;
        while (text < end)
        {
            clock.Check();
            if (token < patternEnd && pattern[token] == '*')
            {
                do { token++; clock.Check(); } while (token < patternEnd && pattern[token] == '*');
                if (token == patternEnd)
                    return singleline || input.Slice(text, end - text).IndexOf('\n') < 0;
                if (behavior == 0 && pattern.Length >= 16 && pattern[token] != '?')
                {
                    int found = FindLiteral(input, text, end, pattern, token, patternEnd, ref clock);
                    if (found < 0 || (!singleline && input.Slice(text, found - text).IndexOf('\n') >= 0)) return false;
                    text = found;
                }
                retryToken = token;
                retryText = text;
            }
            else if (token < patternEnd && Accept(pattern[token], input[text], singleline, behavior))
            {
                token++;
                text++;
            }
            else if (retryToken >= 0 && retryText < end && (singleline || input[retryText] != '\n'))
            {
                // Earliest feasible placement of a segment leaves maximal room for
                // later segments. Only the latest star needs retry state.
                if (behavior == 0 && pattern.Length >= 16 && pattern[retryToken] != '?')
                {
                    int found = FindLiteral(input, retryText + 1, end, pattern, retryToken, patternEnd, ref clock);
                    if (found < 0 || (!singleline && input.Slice(retryText, found - retryText).IndexOf('\n') >= 0)) return false;
                    retryText = found;
                    text = found;
                }
                else text = ++retryText;
                token = retryToken;
            }
            else return false;
        }
        while (token < patternEnd && pattern[token] == '*') { token++; clock.Check(); }
        return token == patternEnd;
    }

    private static bool MatchLines(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern, bool singleline, int behavior, scoped ref SpanMatchClock clock)
    {
        // Numeric RegexOptions.Multiline casts retain '^'/'$' line-anchor semantics
        // without adding line-search overhead to ordinary whole-string matches.
        int start = 0;
        do
        {
            if (MatchFromLine(input, start, pattern, singleline, behavior, ref clock)) return true;
            int offset = input.Slice(start).IndexOf('\n');
            clock.CheckNow();
            if (offset < 0) return false;
            start += offset + 1;
        } while (start <= input.Length);
        return false;
    }

    private static bool MatchFromLine(ReadOnlySpan<char> input, int start, ReadOnlySpan<char> pattern, bool singleline,
        int behavior, scoped ref SpanMatchClock clock)
    {
        int text = start, token = 0, retryToken = -1, retryText = 0;
        while (true)
        {
            clock.Check();
            if (token == pattern.Length)
            {
                if (text == input.Length || input[text] == '\n') return true;
            }
            else if (pattern[token] == '*')
            {
                do { token++; clock.Check(); } while (token < pattern.Length && pattern[token] == '*');
                retryToken = token;
                retryText = text;
                continue;
            }
            else if (text < input.Length && Accept(pattern[token], input[text], singleline, behavior))
            {
                token++;
                text++;
                continue;
            }
            if (retryToken < 0 || retryText == input.Length || (!singleline && input[retryText] == '\n')) return false;
            text = ++retryText;
            token = retryToken;
        }
    }

    private static bool UnicodeEnd(ReadOnlySpan<char> input, int position, bool multiline)
    {
        if (position == input.Length) return true;
        if (!IsUnicodeNewline(input[position])) return false;
        // CRLF is one sequence: its interior is neither an end nor a line start.
        if (input[position] == '\n' && position > 0 && input[position - 1] == '\r') return false;
        int length = input[position] == '\r' && position + 1 < input.Length && input[position + 1] == '\n' ? 2 : 1;
        return multiline || position + length == input.Length;
    }

    private static bool MatchUnicodeLines(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern, bool singleline,
        int behavior, bool multiline, scoped ref SpanMatchClock clock)
    {
        // .NET 11's AnyNewLine numeric flag broadens both dot and anchor semantics.
        // Keep this optional mode separate from the ordinary LF-only fast paths.
        int start = 0;
        while (true)
        {
            int text = start, token = 0, retryToken = -1, retryText = 0;
            while (true)
            {
                clock.Check();
                if (token == pattern.Length)
                {
                    if (UnicodeEnd(input, text, multiline)) return true;
                }
                else if (pattern[token] == '*')
                {
                    do { token++; clock.Check(); } while (token < pattern.Length && pattern[token] == '*');
                    retryToken = token;
                    retryText = text;
                    continue;
                }
                else if (text < input.Length && (pattern[token] == '?'
                    ? singleline || !IsUnicodeNewline(input[text])
                    : pattern[token] == input[text] || (behavior != 0 && Fold(pattern[token], behavior) == Fold(input[text], behavior))))
                {
                    token++;
                    text++;
                    continue;
                }
                if (retryToken < 0 || retryText == input.Length || (!singleline && IsUnicodeNewline(input[retryText]))) break;
                text = ++retryText;
                token = retryToken;
            }
            if (!multiline) return false;
            while (start < input.Length && !IsUnicodeNewline(input[start])) { start++; clock.Check(); }
            if (start == input.Length) return false;
            if (input[start++] == '\r' && start < input.Length && input[start] == '\n') start++;
        }
    }

    private ref struct SpanMatchClock
    {
        private readonly ReadOnlySpan<char> _input;
        private readonly string? _pattern;
        private readonly TimeSpan _timeout;
        private readonly long _started, _budget;
        private int _checks;

        internal SpanMatchClock(ReadOnlySpan<char> input, string? pattern, TimeSpan timeout)
        {
            _pattern = pattern;
            _timeout = timeout;
            _budget = timeout == InfiniteTimeout ? 0 : Math.Max(1, (long)Math.Ceiling(timeout.TotalSeconds * Stopwatch.Frequency));
            // H-027: only finite calls need borrowed input diagnostics. Reusable
            // matchers already own their original pattern; static spans need no copy.
            _input = _budget == 0 ? default : input;
            _started = _budget == 0 ? 0 : Stopwatch.GetTimestamp();
            _checks = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Check()
        {
            // Default matches never read the clock; finite checks are amortized.
            if (_budget != 0 && (++_checks & 1023) == 0) CheckNow();
        }

        internal void CheckNow()
        {
            if (_budget != 0 && Stopwatch.GetTimestamp() - _started >= _budget)
                // Borrow the original slices for the call; allocate diagnostics only
                // on timeout. Spans and stack buffers never escape into the exception.
                throw new RegexMatchTimeoutException(_input.ToString(), InternalUtils.StringToWildcard(_pattern!), _timeout);
        }
    }
}
