"""Isolated source transforms from the frozen baseline; never mutate repository C#."""
from pathlib import Path
HERE=Path(__file__).resolve().parent

def variant(name):
    if name == "current":
        return {p.name:p.read_text(encoding="utf-8-sig") for p in (HERE.parents[2]/"src/Tedd.WildcardMatch").glob("*.cs")}
    sources={p.name[:-4]:p.read_text(encoding="utf-8-sig") for p in (HERE/"baseline-source").glob("*.cs.txt")}
    if name=="baseline": return sources
    if name=="H-057":
        sources=variant("H-032")
        wrapper=sources["WildcardMatch.cs"].replace("    private readonly bool _fast;", "    private readonly PreparedPattern? _prepared;\n    private readonly bool _fast;")
        marker="        _fast = _caseBehavior == 0 && timeout == WildcardEngine.InfiniteTimeout && ((int)options & 0x802) == 0;"
        wrapper=wrapper.replace(marker,marker+"\n        if (_fast) _prepared = PreparedPattern.Create(_matchPattern, (options & WildcardOptions.Singleline) != 0);")
        wrapper=wrapper.replace("        if (_fast) return DefaultWildcardEngine.IsMatch(input.AsSpan(),", "        if (_prepared != null && input.Length <= 32) return _prepared.IsMatch(input.AsSpan());\n        if (_fast) return DefaultWildcardEngine.IsMatch(input.AsSpan(),")
        wrapper=wrapper.replace("        if (_fast) return DefaultWildcardEngine.IsMatch(input,", "        if (_prepared != null && input.Length <= 32) return _prepared.IsMatch(input);\n        if (_fast) return DefaultWildcardEngine.IsMatch(input,")
        sources["WildcardMatch.cs"]=wrapper
        sources["PreparedPattern.cs"]=(HERE/"H-055.cs.txt").read_text()
        return sources
    if name in ("H-052", "H-053"):
        sources=variant("H-037")
        if name=="H-053":
            sources["DefaultWildcardEngine.cs"]=sources["DefaultWildcardEngine.cs"].replace("    private static bool Fixed(", "    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    private static bool Fixed(")
            return sources
        wrapper=sources["WildcardMatch.cs"]
        marker="        WildcardEngine.ValidatePattern(wildcard);\n        WildcardEngine.ValidateOptions(options);\n        string pattern"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"""        if (options == WildcardOptions.None)
        {
            WildcardEngine.ValidatePattern(wildcard);
            if (input is null) throw new ArgumentNullException(nameof(input));
            return DefaultWildcardEngine.IsMatch(input.AsSpan(), wildcard.AsSpan(), false, wildcard.AsSpan().IndexOfAny('*', '?') < 0);
        }
"""+marker,1)
        marker="        WildcardEngine.ValidateOptions(options);\n        if (WildcardEngine.RequiresNormalization"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"        if (options == WildcardOptions.None) return DefaultWildcardEngine.IsMatch(input, wildcard, false, wildcard.IndexOfAny('*', '?') < 0);\n"+marker,1)
        sources["WildcardMatch.cs"]=wrapper
        return sources
    if name=="H-044":
        sources=variant("H-035")
        wrapper=sources["WildcardMatch.cs"]
        marker="        return WildcardEngine.IsMatch(input, pattern, options, WildcardEngine.GetCaseBehavior(options),"
        wrapper=wrapper.replace(marker,"        if (((int)options & 0x803) == 0)\n        {\n            if (input is null) throw new ArgumentNullException(nameof(input));\n            return Cached(pattern.AsSpan(), options).IsMatch(input);\n        }\n"+marker,1)
        marker="        return WildcardEngine.IsMatch(input, wildcard, options, WildcardEngine.GetCaseBehavior(options),"
        wrapper=wrapper.replace(marker,"        if (((int)options & 0x803) == 0) return Cached(wildcard, options).IsMatch(input);\n"+marker,1)
        cache='''
    [ThreadStatic] private static WildcardMatch? _firstCached, _secondCached;
    private static WildcardMatch Cached(ReadOnlySpan<char> pattern, WildcardOptions options)
    {
        WildcardMatch? first = _firstCached;
        if (first != null && first._options == options && pattern.SequenceEqual(first._matchPattern.AsSpan())) return first;
        WildcardMatch? second = _secondCached;
        if (second != null && second._options == options && pattern.SequenceEqual(second._matchPattern.AsSpan()))
        { _secondCached = first; _firstCached = second; return second; }
        var created = new WildcardMatch(pattern, options);
        _secondCached = first; _firstCached = created;
        return created;
    }
'''
        wrapper=wrapper[:wrapper.rfind('}')]+cache+'}\n'
        sources["WildcardMatch.cs"]=wrapper
        return sources
    if name in ("H-032", "H-037", "H-038", "H-050"):
        wrapper=sources["WildcardMatch.cs"]
        wrapper=wrapper.replace("using System.Threading;","using System.Threading;\nusing System.Runtime.CompilerServices;")
        wrapper=wrapper.replace("    private readonly WildcardOptions _options;", "    private readonly bool _fast;\n    private readonly WildcardOptions _options;")
        wrapper=wrapper.replace("        _literal = WildcardEngine.IsLiteral(_matchPattern);", "        _literal = WildcardEngine.IsLiteral(_matchPattern);\n        _fast = _caseBehavior == 0 && timeout == WildcardEngine.InfiniteTimeout && ((int)options & 0x802) == 0;")
        wrapper=wrapper.replace("    public bool IsMatch(string input) =>\n        WildcardEngine.IsMatch(input,", "    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    public bool IsMatch(string input)\n    {\n        if (input is null) throw new ArgumentNullException(nameof(input));\n        if (_fast) return DefaultWildcardEngine.IsMatch(input.AsSpan(), _matchPattern.AsSpan(), (_options & WildcardOptions.Singleline) != 0, _literal, _prefix, _suffix);\n        return WildcardEngine.IsMatch(input,")
        marker="Wildcard, _prefix, _suffix, _leadingQuestions);\n\n    /// <summary>Matches a borrowed"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"Wildcard, _prefix, _suffix, _leadingQuestions);\n    }\n\n    /// <summary>Matches a borrowed")
        wrapper=wrapper.replace("    public bool IsMatch(ReadOnlySpan<char> input) =>\n        WildcardEngine.IsMatch(input,", "    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    public bool IsMatch(ReadOnlySpan<char> input)\n    {\n        if (_fast) return DefaultWildcardEngine.IsMatch(input, _matchPattern.AsSpan(), (_options & WildcardOptions.Singleline) != 0, _literal, _prefix, _suffix);\n        return WildcardEngine.IsMatch(input,")
        marker="Wildcard, _prefix, _suffix, _leadingQuestions);\n}"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"Wildcard, _prefix, _suffix, _leadingQuestions);\n    }\n}")
        marker="        return WildcardEngine.IsMatch(input, pattern, options, WildcardEngine.GetCaseBehavior(options),"
        wrapper=wrapper.replace(marker,"        if (((int)options & 0x803) == 0)\n        {\n            if (input is null) throw new ArgumentNullException(nameof(input));\n            return DefaultWildcardEngine.IsMatch(input.AsSpan(), pattern.AsSpan(), (options & WildcardOptions.Singleline) != 0, WildcardEngine.IsLiteral(pattern));\n        }\n"+marker,1)
        marker="        return WildcardEngine.IsMatch(input, wildcard, options, WildcardEngine.GetCaseBehavior(options),"
        wrapper=wrapper.replace(marker,"        if (((int)options & 0x803) == 0)\n            return DefaultWildcardEngine.IsMatch(input, wildcard, (options & WildcardOptions.Singleline) != 0, wildcard.IndexOfAny('*', '?') < 0);\n"+marker,1)
        sources["WildcardMatch.cs"]=wrapper
        sources["DefaultWildcardEngine.cs"]=(HERE/(name+".cs.txt")).read_text(encoding="utf-8")
        return sources
    if name in ("H-033","H-034","H-035","H-036","H-039","H-040","H-041","H-042","H-045","H-046","H-048","H-051","H-054","H-055","H-056","integrated"):
        wrapper=sources["WildcardMatch.cs"]
        wrapper=wrapper.replace("using System.Threading;","using System.Threading;\nusing System.Runtime.CompilerServices;")
        wrapper=wrapper.replace("    private readonly WildcardOptions _options;","    private readonly PreparedPattern? _prepared;\n    private readonly WildcardOptions _options;")
        wrapper=wrapper.replace("        _literal = WildcardEngine.IsLiteral(_matchPattern);","        _literal = WildcardEngine.IsLiteral(_matchPattern);\n        if (_caseBehavior == 0 && timeout == WildcardEngine.InfiniteTimeout && ((int)options & 0x802) == 0)\n            _prepared = PreparedPattern.Create(_matchPattern, (options & WildcardOptions.Singleline) != 0);")
        wrapper=wrapper.replace("    public bool IsMatch(string input) =>\n        WildcardEngine.IsMatch(input,", "    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    public bool IsMatch(string input)\n    {\n        if (input is null) throw new ArgumentNullException(nameof(input));\n        if (_prepared != null) return _prepared.IsMatch(input.AsSpan());\n        return WildcardEngine.IsMatch(input,")
        marker="Wildcard, _prefix, _suffix, _leadingQuestions);\n\n    /// <summary>Matches a borrowed"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"Wildcard, _prefix, _suffix, _leadingQuestions);\n    }\n\n    /// <summary>Matches a borrowed")
        wrapper=wrapper.replace("    public bool IsMatch(ReadOnlySpan<char> input) =>\n        WildcardEngine.IsMatch(input,", "    [MethodImpl(MethodImplOptions.AggressiveInlining)]\n    public bool IsMatch(ReadOnlySpan<char> input)\n    {\n        if (_prepared != null) return _prepared.IsMatch(input);\n        return WildcardEngine.IsMatch(input,")
        marker="Wildcard, _prefix, _suffix, _leadingQuestions);\n}"
        assert marker in wrapper
        wrapper=wrapper.replace(marker,"Wildcard, _prefix, _suffix, _leadingQuestions);\n    }\n}")
        sources["WildcardMatch.cs"]=wrapper
        sources["PreparedPattern.cs"]=(HERE/(name+".cs.txt")).read_text(encoding="utf-8")
    else:
        raise ValueError(name)
    return sources
