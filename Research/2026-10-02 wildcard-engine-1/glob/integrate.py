"""Integrate individually measured kernels; source generation only, no execution."""
from pathlib import Path
from variants import variant
here=Path(__file__).resolve().parent
repo=here.parents[2]
dest=repo/'src/Tedd.WildcardMatch'
sources=variant('H-037')
wrapper=sources['WildcardMatch.cs']
wrapper=wrapper.replace('    private readonly bool _fast;', '''    private readonly FixedPattern? _fixed;
    private readonly ShortPattern? _short;
    private readonly CompiledSegments? _segments;
    private readonly bool _fast;''')
marker='        _fast = _caseBehavior == 0 && timeout == WildcardEngine.InfiniteTimeout && ((int)options & 0x802) == 0;'
wrapper=wrapper.replace(marker,marker+'''
        // H-035/H-054/H-055/H-057: immutable pattern plans; never cache inputs.
        if (_fast && !_literal)
        {
            bool singleline = (options & WildcardOptions.Singleline) != 0;
            _fixed = FixedPattern.Create(_matchPattern, singleline);
            if (_fixed == null)
            {
                _short = ShortPattern.Create(_matchPattern, singleline);
                _segments = CompiledSegments.Create(_matchPattern, singleline);
            }
        }''')
wrapper=wrapper.replace('if (_fast) return DefaultWildcardEngine.IsMatch(input.AsSpan(), _matchPattern.AsSpan(), (_options & WildcardOptions.Singleline) != 0, _literal, _prefix, _suffix);','if (_fast) return MatchFast(input.AsSpan());')
wrapper=wrapper.replace('if (_fast) return DefaultWildcardEngine.IsMatch(input, _matchPattern.AsSpan(), (_options & WildcardOptions.Singleline) != 0, _literal, _prefix, _suffix);','if (_fast) return MatchFast(input);')
wrapper=wrapper[:wrapper.rfind('}')]+'''
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MatchFast(ReadOnlySpan<char> input)
    {
        bool singleline = (_options & WildcardOptions.Singleline) != 0;
        if (_literal) return DefaultWildcardEngine.IsMatch(input, _matchPattern.AsSpan(), singleline, true);
        if (_fixed != null) return _fixed.IsMatch(input);
        // H-057: bound per-character bitmap work; long text uses literal search.
        if (_short != null && input.Length <= 32) return _short.IsMatch(input);
        if (_segments != null && input.Length > 32) return _segments.IsMatch(input);
        return DefaultWildcardEngine.IsMatch(input, _matchPattern.AsSpan(), singleline, false, _prefix, _suffix);
    }
}
'''
sources['WildcardMatch.cs']=wrapper
engine=sources['DefaultWildcardEngine.cs']
engine=engine.replace('        for (int i = 0; i < length; i++) if (!Accept(pattern[i], input[i], singleline)) return false;', '''        // Preserve H-020: bulk-test a long leading question run, including LF.
        int start = 0;
        if (length >= 32 && pattern[0] == '?' && pattern[1] == '?')
        {
            start = 2;
            while (start < length && pattern[start] == '?') start++;
            if (!singleline && input.Slice(0, start).IndexOf('\\n') >= 0) return false;
        }
        for (int i = start; i < length; i++) if (!Accept(pattern[i], input[i], singleline)) return false;''')
engine=engine.replace('internal static class DefaultWildcardEngine','// H-032/H-037: no clock dispatch for infinite-timeout, ordinal calls.\n// Range-checked spans preserve UTF-16, embedded NUL, and final-LF semantics.\ninternal static class DefaultWildcardEngine')
engine=engine.replace('            ref ushort p =', '''            // H-037: two overlapping 8-lane loads cover widths 8..16 exactly.
            // The width check dominates both loads; no padding is read.
            ref ushort p =''',1)
sources['DefaultWildcardEngine.cs']=engine
fixed=(here/'H-054.cs.txt').read_text().replace('PreparedPattern','FixedPattern')
fixed=fixed.replace('    private readonly bool _singleline;', '    private readonly bool _singleline;\n    private readonly int _leadingQuestions;')
fixed=fixed.replace('        _singleline = singleline;', '''        _singleline = singleline;
        if (pattern.Length >= 32 && pattern[0] == '?' && pattern[1] == '?')
        {
            int run = 2;
            while (run < pattern.Length && pattern[run] == '?') run++;
            _leadingQuestions = run;
        }''',1)
fixed=fixed.replace('if (pattern.Length > 16)','if (Vector256.IsHardwareAccelerated && pattern.Length > 16 && pattern.Length <= 8192 && _leadingQuestions != pattern.Length)',1)
fixed=fixed.replace('if (pattern.Length >= 8 && pattern.Length <= 16)','if (Vector128.IsHardwareAccelerated && pattern.Length >= 8 && pattern.Length <= 16)',1)
fixed=fixed.replace('if (Vector256.IsHardwareAccelerated && length > 16)','if (Vector256.IsHardwareAccelerated && _blocks != null)')
fixed=fixed.replace('        for (int i = 0; i < length; i++)','''        // H-020 portable fallback also bounds the plan footprint for huge patterns.
        if (!_singleline && input.Slice(0, _leadingQuestions).IndexOf('\\n') >= 0) return false;
        for (int i = _leadingQuestions; i < length; i++)''')
fixed=fixed.replace('            Span<ushort> mask = stackalloc ushort[16];', '''            // H-054: one immutable array pairs masks with values; cap payload at 32 KiB.
            // Every lane is initialized and each overlapping tail load ends at Length.
            Span<ushort> mask = stackalloc ushort[16];''')
fixed=fixed.replace('            ref ushort text = ref Unsafe.As', '''            // H-035: fuse literal mismatch and wildcard LF errors before reduction.
            // Only the logical input prefix is loaded; literal LF remains literal.
            ref ushort text = ref Unsafe.As''')
sources['FixedPattern.cs']=fixed
short=(here/'H-055.cs.txt').read_text().replace('PreparedPattern','ShortPattern')
short=short.replace('        Span<char> codes =', '''        // H-055: bounded initialized scratch and collision-free low-bit buckets.
        // Exact Code verification handles arbitrary UTF-16, including NUL collisions.
        Span<char> codes =''')
short=short.replace('            active |= (active & _stars) << 1;', '''            // Consecutive stars were collapsed: one epsilon step closes every state.
            // At most 63 tokens leave bit 63 for acceptance; no shift by 64 occurs.
            active |= (active & _stars) << 1;''')
sources['ShortPattern.cs']=short
segments=(here/'H-056.cs.txt').read_text().replace('PreparedPattern','CompiledSegments')
segments=segments.replace("        pattern.IndexOf('*') >= 0 ? new CompiledSegments(pattern, singleline) : null;",'''        pattern.Length <= 8192 && pattern.IndexOf('*') >= 0 && CountSegments(pattern) <= 64
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
    }''')
segments=segments.replace('        int count = 0;','        // H-056: two passes allocate exactly the descriptors needed, without a List.\n        int count = 0;',1)
segments=segments.replace('                        ref ushort text =', '''                        // H-048: eight candidate starts, two literal anchors per SIMD mask.
                        // next <= max-7 and anchor < Length prove every load is in input.
                        // ushort extraction yields eight bits; guard before trailing-zero count.
                        ref ushort text =''')
sources['CompiledSegments.cs']=segments
sources['CompiledSegments.cs']=segments.replace("pattern.Length <= 8192 && pattern.IndexOf('*')", "pattern.Length <= 8192 && !(pattern.Length >= 32 && pattern[0] == '?' && pattern[1] == '?') && pattern.IndexOf('*')").replace('    {\n        if (segment.AnchorLength == segment.Length)', "    {\n        // H-020: all-question segments need only a bulk LF test.\n        if (segment.Anchor < 0) return _singleline || input.Slice(position, segment.Length).IndexOf('\\n') < 0;\n        if (segment.AnchorLength == segment.Length)")
for filename,source in sources.items():
    if filename not in ('WildcardMatch.cs','DefaultWildcardEngine.cs','FixedPattern.cs','ShortPattern.cs','CompiledSegments.cs'):
        (dest/filename).write_bytes((here/'baseline-source'/(filename+'.txt')).read_bytes())
    else:
        (dest/filename).write_bytes(source.replace('\r\n','\n').encode('utf-8'))
