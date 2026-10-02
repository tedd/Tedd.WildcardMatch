"""Generate documented experiment branches; source only, no build or timing."""
from pathlib import Path
here=Path(__file__).resolve().parent
h35=(here/'H-035.cs.txt').read_text()
h36=h35.replace('private readonly Vector128<ushort> _pattern0, _pattern1, _mask0, _mask1;',
    'private readonly Vector128<ushort> _pattern0, _pattern1, _mask0, _mask1;\n    private readonly Vector256<ushort>[]? _patterns, _masks;')
marker='        if (pattern.Length >= 8 && pattern.Length <= 16)'
h36=h36.replace(marker,'''        if (pattern.Length > 16)
        {
            int count = (pattern.Length + 15) / 16;
            _patterns = new Vector256<ushort>[count]; _masks = new Vector256<ushort>[count];
            Span<ushort> mask = stackalloc ushort[16];
            ref ushort p = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(pattern.AsSpan()));
            for (int block = 0; block < count; block++)
            {
                int offset = Math.Min(block * 16, pattern.Length - 16);
                for (int lane = 0; lane < 16; lane++) mask[lane] = pattern[offset + lane] == '?' ? (ushort)0 : ushort.MaxValue;
                _patterns[block] = Vector256.LoadUnsafe(ref p, (nuint)offset);
                _masks[block] = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(mask));
            }
        }
'''+marker)
marker='        if (Vector128.IsHardwareAccelerated && length >= 8 && length <= 16)'
h36=h36.replace(marker,'''        if (Vector256.IsHardwareAccelerated && length > 16)
        {
            ref ushort text = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
            for (int block = 0; block < _patterns!.Length; block++)
            {
                int offset = Math.Min(block * 16, length - 16);
                Vector256<ushort> a = Vector256.LoadUnsafe(ref text, (nuint)offset);
                Vector256<ushort> mask = _masks![block];
                Vector256<ushort> bad = (a ^ _patterns[block]) & mask;
                if (!_singleline) bad |= Vector256.Equals(a, Vector256.Create((ushort)'\\n')) & ~mask;
                if (!Vector256.EqualsAll(bad, Vector256<ushort>.Zero)) return false;
            }
            return true;
        }
'''+marker)
(here/'H-036.cs.txt').write_text(h36)
h54=h36.replace('private readonly Vector256<ushort>[]? _patterns, _masks;', '''private readonly Block[]? _blocks;
    private readonly struct Block
    {
        internal readonly Vector256<ushort> Pattern, Mask;
        internal Block(Vector256<ushort> pattern, Vector256<ushort> mask) { Pattern = pattern; Mask = mask; }
    }''')
h54=h54.replace('_patterns = new Vector256<ushort>[count]; _masks = new Vector256<ushort>[count];', '_blocks = new Block[count];')
h54=h54.replace('''                _patterns[block] = Vector256.LoadUnsafe(ref p, (nuint)offset);
                _masks[block] = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(mask));''',
'''                _blocks[block] = new Block(Vector256.LoadUnsafe(ref p, (nuint)offset), Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(mask)));''')
h54=h54.replace('_patterns!.Length', '_blocks!.Length')
h54=h54.replace('''                Vector256<ushort> mask = _masks![block];
                Vector256<ushort> bad = (a ^ _patterns[block]) & mask;''',
'''                ref readonly Block plan = ref _blocks[block];
                Vector256<ushort> mask = plan.Mask;
                Vector256<ushort> bad = (a ^ plan.Pattern) & mask;''')
(here/'H-054.cs.txt').write_text(h54)
h32=(here/'H-032.cs.txt').read_text()
(here/'H-038.cs.txt').write_text(h32.replace('pattern.Length >= 16 && pattern[', 'pattern['))
# H-037 isolates dynamic vector masks atop the already clock-free header.
body='''
    private static bool Fixed(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern, bool singleline)
    {
        int length = pattern.Length;
        if (input.Length != length && !(input.Length == length + 1 && input[length] == '\\n')) return false;
#if NET10_0_OR_GREATER
        if (Vector128.IsHardwareAccelerated && length >= 8 && length <= 16)
        {
            ref ushort p = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(pattern));
            ref ushort text = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
            Vector128<ushort> p0 = Vector128.LoadUnsafe(ref p), p1 = Vector128.LoadUnsafe(ref p, (nuint)(length - 8));
            Vector128<ushort> q = Vector128.Create((ushort)'?');
            Vector128<ushort> m0 = Vector128.Equals(p0, q), m1 = Vector128.Equals(p1, q);
            Vector128<ushort> a = Vector128.LoadUnsafe(ref text), b = Vector128.LoadUnsafe(ref text, (nuint)(length - 8));
            Vector128<ushort> bad = ((a ^ p0) & ~m0) | ((b ^ p1) & ~m1);
            if (!singleline)
                bad |= (Vector128.Equals(a, Vector128.Create((ushort)'\\n')) & m0)
                    | (Vector128.Equals(b, Vector128.Create((ushort)'\\n')) & m1);
            return Vector128.EqualsAll(bad, Vector128<ushort>.Zero);
        }
#endif
        for (int i = 0; i < length; i++) if (!Accept(pattern[i], input[i], singleline)) return false;
        return true;
    }
'''
h37=h32.replace('namespace Tedd;', '''#if NET10_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.InteropServices;
#endif
namespace Tedd;''')
h37=h37.replace('        return Window(input, pattern, singleline, prefix, suffix)',
    "        if (pattern.IndexOf('*') < 0) return Fixed(input, pattern, singleline);\n        return Window(input, pattern, singleline, prefix, suffix)",1)
h37=h37.replace('    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    private static bool Accept',body+'\n    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    private static bool Accept')
(here/'H-037.cs.txt').write_text(h37)
h46=h35.replace('''            if (!_singleline)
            {
                Vector128<ushort> lf''','''            if (!Vector128.EqualsAll(bad, Vector128<ushort>.Zero)) return false;
            bad = Vector128<ushort>.Zero;
            if (!_singleline)
            {
                Vector128<ushort> lf''')
(here/'H-046.cs.txt').write_text(h46)
h51=h35.replace('private readonly bool _singleline;', 'private readonly bool _singleline;\n    private readonly int _length;')
h51=h51.replace('_pattern = pattern;', '_pattern = pattern;\n        _length = pattern.Length;')
h51=h51.replace('int length = _pattern.Length;', 'int length = _length;')
(here/'H-051.cs.txt').write_text(h51)
h40=(here/'H-040.cs.txt').read_text()
h41=h40.replace('private readonly Dictionary<char, ulong> _literals = new Dictionary<char, ulong>();',
    'private readonly ulong[] _literals = new ulong[128];')
h41=h41.replace('_literals.TryGetValue(token, out ulong mask); _literals[token] = mask | bit;', '_literals[token] |= bit;')
h41=h41.replace("pattern.Length <= 63 && pattern.IndexOf('*') >= 0 ?", "pattern.Length <= 63 && pattern.IndexOf('*') >= 0 && IsAscii(pattern) ?")
h41=h41.replace('    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    internal bool IsMatch',
    '    private static bool IsAscii(string pattern) { foreach (char c in pattern) if (c >= 128) return false; return true; }\n\n    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    internal bool IsMatch')
h41=h41.replace('_literals.TryGetValue(value, out ulong literals);', 'ulong literals = value < 128 ? _literals[value] : 0;')
(here/'H-041.cs.txt').write_text(h41)
h39=(here/'H-039.cs.txt').read_text()
h45=h39.replace('AnchorLength, Remaining;', 'AnchorLength, Remaining, Second;')
h45=h45.replace('int remaining)\n        { Offset', 'int remaining, int second = -1)\n        { Second = second; Offset')
h45=h45.replace('segments.Add(new Segment(offset, end - offset, anchor, anchorLength, 0));',
'''int second = -1;
            if (anchorLength == 1)
                for (int i = end - 1; i >= offset; i--) if (pattern[i] != '?' && i - offset != anchor) { second = i - offset; break; }
            segments.Add(new Segment(offset, end - offset, anchor, anchorLength, 0, second));''')
h45=h45.replace('s.AnchorLength, remaining);', 's.AnchorLength, remaining, s.Second);')
h45=h45.replace('                if (Matches(input, in segment, candidate)) break;',
    '                if ((segment.Second < 0 || input[candidate + segment.Second] == _pattern[segment.Offset + segment.Second]) && Matches(input, in segment, candidate)) break;')
(here/'H-045.cs.txt').write_text(h45)
# H-048 materializes candidate-position bits from two literal anchor comparisons.
h48=h45.replace('namespace Tedd;', '''#if NET10_0_OR_GREATER
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;
#endif
namespace Tedd;''')
marker='''                    int found = input.Slice(searchStart, searchLength).IndexOf(_pattern.AsSpan(segment.Offset + segment.Anchor, segment.AnchorLength));'''
h48=h48.replace(marker,'''                    int found;
#if NET10_0_OR_GREATER
                    if (Vector128.IsHardwareAccelerated && segment.Second >= 0 && max - candidate >= 7)
                    {
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
                        found = input.Slice(searchStart, searchLength).IndexOf(_pattern.AsSpan(segment.Offset + segment.Anchor, segment.AnchorLength));''')
(here/'H-048.cs.txt').write_text(h48)
h56=h48.replace('''        var segments = new List<Segment>();''','''        int count = 0;
        for (int i = 0; i < pattern.Length;)
        {
            if (pattern[i] == '*') { i++; continue; }
            int start = i++;
            while (i < pattern.Length && pattern[i] != '*') i++;
            count++; _minimum += i - start;
        }
        _segments = count == 0 ? Array.Empty<Segment>() : new Segment[count];
        int segmentIndex = 0;''')
h56=h56.replace('''            segments.Add(new Segment(offset, end - offset, anchor, anchorLength, 0, second));
            _minimum += end - offset; offset = end;''','''            _segments[segmentIndex++] = new Segment(offset, end - offset, anchor, anchorLength, 0, second);
            offset = end;''')
h56=h56.replace('        _segments = segments.ToArray();\n', '')
h56=h56.replace('using System.Collections.Generic;\n', '')
(here/'H-056.cs.txt').write_text(h56)
