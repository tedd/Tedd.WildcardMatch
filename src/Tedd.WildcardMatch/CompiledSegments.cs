using System;
using System.Runtime.CompilerServices;

#if NET10_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;
#endif
namespace Tedd;

internal sealed class CompiledSegments
{
    private readonly string _pattern;
    private readonly Segment[] _segments;
    private readonly bool _singleline, _startsStar, _endsStar, _containsLf;
    private readonly int _minimum;
    private readonly struct Segment
    {
        internal readonly int Offset, Length, Anchor, AnchorLength, Remaining, Second;
        internal Segment(int offset, int length, int anchor, int anchorLength, int remaining, int second = -1)
        { Second = second; Offset = offset; Length = length; Anchor = anchor; AnchorLength = anchorLength; Remaining = remaining; }
    }

    private CompiledSegments(string pattern, bool singleline)
    {
        _pattern = pattern; _singleline = singleline;
        _startsStar = pattern[0] == '*'; _endsStar = pattern[pattern.Length - 1] == '*';
        _containsLf = pattern.IndexOf('\n') >= 0;
        // H-056: two passes allocate exactly the descriptors needed, without a List.
        int count = 0;
        for (int i = 0; i < pattern.Length;)
        {
            if (pattern[i] == '*') { i++; continue; }
            int start = i++;
            while (i < pattern.Length && pattern[i] != '*') i++;
            count++; _minimum += i - start;
        }
        _segments = count == 0 ? Array.Empty<Segment>() : new Segment[count];
        int segmentIndex = 0;
        for (int offset = 0; offset < pattern.Length;)
        {
            if (pattern[offset] == '*') { offset++; continue; }
            int end = offset;
            while (end < pattern.Length && pattern[end] != '*') end++;
            int anchor = -1, anchorLength = 0;
            for (int i = offset; i < end;)
            {
                if (pattern[i] == '?') { i++; continue; }
                int start = i++;
                while (i < end && pattern[i] != '?') i++;
                if (i - start > anchorLength) { anchor = start - offset; anchorLength = i - start; }
            }
            int second = -1;
            if (anchorLength == 1)
                for (int i = end - 1; i >= offset; i--) if (pattern[i] != '?' && i - offset != anchor) { second = i - offset; break; }
            _segments[segmentIndex++] = new Segment(offset, end - offset, anchor, anchorLength, 0, second);
            offset = end;
        }
        int remaining = 0;
        for (int i = _segments.Length - 1; i >= 0; i--)
        {
            Segment s = _segments[i];
            _segments[i] = new Segment(s.Offset, s.Length, s.Anchor, s.AnchorLength, remaining, s.Second);
            remaining += s.Length;
        }
    }

    internal static CompiledSegments? Create(string pattern, bool singleline) =>
        pattern.Length <= 8192 && !(pattern.Length >= 32 && pattern[0] == '?' && pattern[1] == '?')
            && pattern.IndexOf('*') >= 0 && CountSegments(pattern) <= 64
            ? new CompiledSegments(pattern, singleline) : null;

    private static int CountSegments(string pattern)
    {
        int count = 0;
        bool inSegment = false;
        foreach (char token in pattern)
        {
            if (token == '*') inSegment = false;
            else if (!inSegment) { inSegment = true; if (++count > 64) break; }
        }
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsMatch(ReadOnlySpan<char> input)
    {
        if (!_singleline && !_containsLf)
        {
            if (input.Length > 0 && input[input.Length - 1] == '\n') input = input.Slice(0, input.Length - 1);
            return Window(input);
        }
        return Window(input) || (input.Length > 0 && input[input.Length - 1] == '\n' && Window(input.Slice(0, input.Length - 1)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Matches(ReadOnlySpan<char> input, in Segment segment, int position)
    {
        // H-020: an all-question segment needs only a bulk LF test, not token dispatch.
        if (segment.Anchor < 0) return _singleline || input.Slice(position, segment.Length).IndexOf('\n') < 0;
        if (segment.AnchorLength == segment.Length)
            return input.Slice(position, segment.Length).SequenceEqual(_pattern.AsSpan(segment.Offset, segment.Length));
        for (int i = 0; i < segment.Length; i++)
        {
            char token = _pattern[segment.Offset + i], value = input[position + i];
            if (token == '?' ? !_singleline && value == '\n' : token != value) return false;
        }
        return true;
    }

    private bool Window(ReadOnlySpan<char> input)
    {
        if (input.Length < _minimum) return false;
        int first = 0, last = _segments.Length, position = 0, limit = input.Length;
        if (!_startsStar)
        {
            ref readonly Segment prefix = ref _segments[first++];
            if (!Matches(input, in prefix, position)) return false;
            position += prefix.Length;
        }
        if (!_endsStar)
        {
            ref readonly Segment suffix = ref _segments[--last];
            limit -= suffix.Length;
            if (limit < position || !Matches(input, in suffix, limit)) return false;
        }
        for (int i = first; i < last; i++)
        {
            ref readonly Segment segment = ref _segments[i];
            int max = input.Length - segment.Remaining - segment.Length;
            int candidate = position;
            while (true)
            {
                if (candidate > max) return false;
                if (segment.Anchor >= 0)
                {
                    int searchStart = candidate + segment.Anchor;
                    int searchLength = max - candidate + segment.AnchorLength;
                    int found;
#if NET10_0_OR_GREATER
                    if (Vector128.IsHardwareAccelerated && segment.Second >= 0 && max - candidate >= 7)
                    {
                        // H-048: eight candidate starts, two literal anchors per SIMD mask.
                        // next <= max-7 and anchor < Length prove every load is in input.
                        // ushort extraction yields eight bits; guard before trailing-zero count.
                        ref ushort text = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
                        int next = candidate;
                        found = -1;
                        while (max - next >= 7)
                        {
                            Vector128<ushort> a = Vector128.LoadUnsafe(ref text, (nuint)(next + segment.Anchor));
                            Vector128<ushort> b = Vector128.LoadUnsafe(ref text, (nuint)(next + segment.Second));
                            uint bits = (Vector128.Equals(a, Vector128.Create((ushort)_pattern[segment.Offset + segment.Anchor]))
                                & Vector128.Equals(b, Vector128.Create((ushort)_pattern[segment.Offset + segment.Second]))).ExtractMostSignificantBits();
                            if (bits != 0) { found = next - candidate + BitOperations.TrailingZeroCount(bits); break; }
                            next += 8;
                        }
                        if (found < 0)
                        {
                            int tail = input.Slice(next + segment.Anchor, max - next + segment.AnchorLength).IndexOf(_pattern.AsSpan(segment.Offset + segment.Anchor, segment.AnchorLength));
                            if (tail >= 0) found = next - candidate + tail;
                        }
                    }
                    else
#endif
                        found = input.Slice(searchStart, searchLength).IndexOf(_pattern.AsSpan(segment.Offset + segment.Anchor, segment.AnchorLength));
                    if (found < 0) return false;
                    candidate += found;
                }
                if (!_singleline && input.Slice(position, candidate - position).IndexOf('\n') >= 0) return false;
                if ((segment.Second < 0 || input[candidate + segment.Second] == _pattern[segment.Offset + segment.Second]) && Matches(input, in segment, candidate)) break;
                if (segment.Anchor < 0) return false;
                candidate++;
            }
            position = candidate + segment.Length;
        }
        return _singleline || input.Slice(position, limit - position).IndexOf('\n') < 0;
    }
}
