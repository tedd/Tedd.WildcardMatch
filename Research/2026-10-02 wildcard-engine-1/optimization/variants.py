"""Single-mechanism candidates, applied to committed source only in the disposable workspace."""
def replace_once(source, old, new):
    assert source.count(old) == 1, (old[:100], source.count(old))
    return source.replace(old, new, 1)

def variant(engine, wrapper, name):
    if name == "H-004":
        engine = replace_once(engine, """            clock.Check();
            if (text == end || !Accept(pattern[token], input[text], singleline, behavior)) return false;""", """            clock.Check();
            if (pattern[token] == '?')
            {
                int next = token + 1;
                while (next < patternEnd && pattern[next] == '?') { next++; clock.Check(); }
                int length = next - token;
                if (length > end - text || (!singleline && input.IndexOf('\\n', text, length) >= 0)) return false;
                text += length;
                token = next;
                continue;
            }
            if (text == end || !Accept(pattern[token], input[text], singleline, behavior)) return false;""")
        engine = replace_once(engine, """            clock.Check();
            if (text == end || !Accept(pattern[patternEnd - 1], input[end - 1], singleline, behavior)) return false;""", """            clock.Check();
            if (pattern[patternEnd - 1] == '?')
            {
                int next = patternEnd - 1;
                while (next > token && pattern[next - 1] == '?') { next--; clock.Check(); }
                int length = patternEnd - next;
                if (length > end - text || (!singleline && input.IndexOf('\\n', end - length, length) >= 0)) return false;
                end -= length;
                patternEnd = next;
                continue;
            }
            if (text == end || !Accept(pattern[patternEnd - 1], input[end - 1], singleline, behavior)) return false;""")
    elif name == "H-005":
        engine = replace_once(engine, """                retryToken = token;
                retryText = text;
            }
            else if""", """                if (behavior == 0 && pattern[token] != '?')
                {
                    int found = input.IndexOf(pattern[token], text, end - text);
                    if (found < 0 || (!singleline && input.IndexOf('\\n', text, found - text) >= 0)) return false;
                    text = found;
                }
                retryToken = token;
                retryText = text;
            }
            else if""")
        engine = replace_once(engine, """                // later segments. Retrying only the latest star needs no stack/recursion.
                text = ++retryText;
                token = retryToken;""", """                if (behavior == 0 && pattern[retryToken] != '?')
                {
                    int found = input.IndexOf(pattern[retryToken], retryText + 1, end - retryText - 1);
                    if (found < 0 || (!singleline && input.IndexOf('\\n', retryText, found - retryText) >= 0)) return false;
                    retryText = found;
                    text = found;
                }
                else text = ++retryText;
                token = retryToken;""")
    elif name == "H-006":
        engine = replace_once(engine, """                token++;
                text++;
            }
            else if""", """                if (behavior == 0 && pattern[token] != '?')
                {
                    int next = token + 1;
                    while (next < patternEnd && pattern[next] != '*' && pattern[next] != '?') { next++; clock.Check(); }
                    int length = next - token;
                    if (length <= end - text && string.CompareOrdinal(input, text, pattern, token, length) == 0)
                    {
                        token = next;
                        text += length;
                        continue;
                    }
                    if (retryToken < 0 || retryText == end || (!singleline && input[retryText] == '\\n')) return false;
                    text = ++retryText;
                    token = retryToken;
                    continue;
                }
                token++;
                text++;
            }
            else if""")
    elif name == "H-007":
        engine = replace_once(engine, "value >= 'A' && value <= 'Z'", "(uint)(value - 'A') <= 'Z' - 'A'")
    elif name == "H-008":
        engine = replace_once(engine, "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n    private static bool Accept", "[MethodImpl(MethodImplOptions.NoInlining)]\n    private static bool Accept")
    elif name == "H-009":
        engine = replace_once(engine, "        internal void CheckNow()", "        [MethodImpl(MethodImplOptions.AggressiveInlining)]\n        internal void CheckNow()")
    elif name == "H-010":
        wrapper = replace_once(wrapper, "    public bool IsMatch(string input) =>", "    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n    public bool IsMatch(string input) =>")
    elif name == "H-011":
        engine = engine.replace("wildcard.IndexOfAny(Tokens)", "wildcard.AsSpan().IndexOfAny('*', '?')")
        engine = engine.replace("pattern.IndexOfAny(Tokens)", "pattern.AsSpan().IndexOfAny('*', '?')")
        engine = engine.replace("pattern.LastIndexOfAny(Tokens)", "pattern.AsSpan().LastIndexOfAny('*', '?')")
    elif name == "H-012":
        start = engine.index("        if (behavior == 0 && patternEnd >= 32)")
        end = engine.index("        // Anchor fixed prefix", start)
        engine = engine[:start] + engine[end:]
    elif name == "H-021":
        engine, wrapper = variant(engine, wrapper, "H-018")
        engine = replace_once(engine, """                if (behavior == 0 && pattern.Length >= 16 && pattern[retryToken] != '?')
                {
                    int found = FindLiteral(input, retryText + 1, end, pattern, retryToken, patternEnd, ref clock);
                    if (found < 0 || (!singleline && input.IndexOf('\\n', retryText, found - retryText) >= 0)) return false;
                    retryText = found;
                    text = found;
                }
                else text = ++retryText;""", """                text = ++retryText;""")
    elif name == "H-022":
        engine, wrapper = variant(engine, wrapper, "H-017")
        engine = replace_once(engine, """        // Regex uses Unicode lowercase mappings""", """        if (behavior >= 4) return value;
        // Regex uses Unicode lowercase mappings""")
        engine = engine.replace("Fold(input, behavior)", "Fold(input, behavior & 3)").replace("Fold(input[text], behavior)", "Fold(input[text], behavior & 3)")
        engine = replace_once(engine, "if (behavior == 3) return false;", "if ((behavior & 3) == 3) return false;")
        engine = replace_once(engine, "    internal static bool IsLiteral", """    internal static string PrepareCasePattern(string pattern, int behavior, out bool prepared)
    {
        prepared = false;
        if (behavior == 0) return pattern;
        foreach (char value in pattern)
        {
            if (value < 128) continue;
            prepared = true;
            char[] characters = pattern.ToCharArray();
            for (int i = 0; i < characters.Length; i++) characters[i] = Fold(characters[i], behavior);
            return new string(characters);
        }
        return pattern;
    }

    internal static bool IsLiteral""")
        wrapper = replace_once(wrapper, """        _matchPattern = WildcardEngine.GetMatchingPattern(wildcard, options);""", """        _matchPattern = WildcardEngine.GetMatchingPattern(wildcard, options);
        _matchPattern = WildcardEngine.PrepareCasePattern(_matchPattern, _caseBehavior, out bool prepared);
        if (prepared) _caseBehavior += 4;""")
    elif name == "H-019":
        engine, wrapper = variant(engine, wrapper, "H-018")
        engine = replace_once(engine, """        int text = 0, token = 0, patternEnd = pattern.Length;""", """        bool search = behavior == 0 && pattern.Length >= 16;
        int text = 0, token = 0, patternEnd = pattern.Length;""")
        engine = engine.replace("behavior == 0 && pattern.Length >= 16 && pattern[", "search && pattern[")
    elif name == "H-020":
        engine, wrapper = variant(engine, wrapper, "H-014")
        wrapper = replace_once(wrapper, "    private readonly bool _literal;", "    private readonly bool _literal;\n    private readonly int _leadingQuestions;")
        wrapper = replace_once(wrapper, """        _literal = WildcardEngine.IsLiteral(_matchPattern);""", """        _literal = WildcardEngine.IsLiteral(_matchPattern);
        if (_matchPattern.Length >= 32 && _matchPattern[0] == '?' && _matchPattern[1] == '?')
        {
            int length = 2;
            while (length < _matchPattern.Length && _matchPattern[length] == '?') length++;
            _leadingQuestions = length;
        }""")
        wrapper = replace_once(wrapper, "_literal, _timeout, Wildcard);", "_literal, _timeout, Wildcard, _leadingQuestions);")
        engine = replace_once(engine, "int behavior, bool literal, TimeSpan timeout, string originalPattern)", "int behavior, bool literal, TimeSpan timeout, string originalPattern, int leadingQuestions = -1)")
        engine = replace_once(engine, "MatchWindow(input, input.Length, pattern, singleline, behavior, ref clock);", "MatchWindow(input, input.Length, pattern, singleline, behavior, ref clock, leadingQuestions);")
        engine = replace_once(engine, "MatchWindow(input, input.Length - 1, pattern, singleline, behavior, ref clock);", "MatchWindow(input, input.Length - 1, pattern, singleline, behavior, ref clock, leadingQuestions);")
        engine = replace_once(engine, """        int behavior, ref MatchClock clock)
    {
        int text = 0""", """        int behavior, ref MatchClock clock, int preparedQuestions)
    {
        int text = 0""")
        engine = replace_once(engine, """            int length = 2;
            while (length < patternEnd && pattern[length] == '?') { length++; clock.Check(); }""", """            int length = preparedQuestions;
            if (length < 0)
            {
                length = 2;
                while (length < patternEnd && pattern[length] == '?') { length++; clock.Check(); }
            }""")
    elif name == "H-018":
        engine, wrapper = variant(engine, wrapper, "H-005")
        engine = engine.replace("behavior == 0 && pattern[token] != '?'", "behavior == 0 && pattern.Length >= 16 && pattern[token] != '?'")
        engine = engine.replace("behavior == 0 && pattern[retryToken] != '?'", "behavior == 0 && pattern.Length >= 16 && pattern[retryToken] != '?'")
        engine = replace_once(engine, "input.IndexOf(pattern[token], text, end - text)", "FindLiteral(input, text, end, pattern, token, patternEnd, ref clock)")
        engine = replace_once(engine, "input.IndexOf(pattern[retryToken], retryText + 1, end - retryText - 1)", "FindLiteral(input, retryText + 1, end, pattern, retryToken, patternEnd, ref clock)")
        engine = replace_once(engine, "    private static bool MatchWindow(", """    private static int FindLiteral(string input, int start, int end, string pattern, int token, int patternEnd, ref MatchClock clock)
    {
        int next = token + 1;
        while (next < patternEnd && pattern[next] != '*' && pattern[next] != '?') { next++; clock.Check(); }
        int length = next - token;
        if (length == 1) return input.IndexOf(pattern[token], start, end - start);
        int offset = input.AsSpan(start, end - start).IndexOf(pattern.AsSpan(token, length), StringComparison.Ordinal);
        return offset < 0 ? -1 : start + offset;
    }

    private static bool MatchWindow(""")
    elif name == "H-017":
        engine, wrapper = variant(engine, wrapper, "H-013")
        engine = replace_once(engine, "TryAsciiLiteral(input, pattern, behavior, out bool asciiResult)", "TryAsciiLiteral(input, pattern, behavior, ref clock, out bool asciiResult)")
        engine = replace_once(engine, "TryAsciiLiteral(string input, string pattern, int behavior, out bool result)", "TryAsciiLiteral(string input, string pattern, int behavior, ref MatchClock clock, out bool result)")
        engine = replace_once(engine, """            char a = pattern[i], b = input[i];""", """            clock.Check();
            char a = pattern[i], b = input[i];""")
    elif name == "H-015":
        start = engine.index("    private static bool MatchWindow(")
        end = engine.index("    private static bool MatchLines(", start)
        body = engine[start:end]
        body = body.replace("private static bool MatchWindow", "private static unsafe bool MatchWindow", 1)
        body = replace_once(body, """    {
        int text = 0""", """    {
        fixed (char* characters = input)
        fixed (char* tokens = pattern)
        {
        int text = 0""")
        body = body.replace("pattern[", "tokens[").replace("input[", "characters[")
        body = replace_once(body, """        return token == patternEnd;
    }""", """        return token == patternEnd;
        }
    }""")
        engine = engine[:start] + body + engine[end:]
    elif name == "H-016":
        wrapper = replace_once(wrapper, "    private readonly bool _literal;", "    private readonly bool _literal;\n    private readonly int _prefix = -1, _suffix = -1;")
        wrapper = replace_once(wrapper, """        _literal = WildcardEngine.IsLiteral(_matchPattern);""", """        _literal = WildcardEngine.IsLiteral(_matchPattern);
        if (!_literal && _caseBehavior == 0 && _matchPattern.Length >= 32 && ((int)options & 0x802) == 0)
        {
            _prefix = WildcardEngine.GetPrefix(_matchPattern);
            _suffix = WildcardEngine.GetSuffix(_matchPattern);
        }""")
        wrapper = replace_once(wrapper, "_literal, _timeout, Wildcard);", "_literal, _timeout, Wildcard, _prefix, _suffix);")
        engine = replace_once(engine, "    internal static bool IsLiteral", "    internal static int GetPrefix(string pattern) => pattern.IndexOfAny(Tokens);\n    internal static int GetSuffix(string pattern) => pattern.Length - pattern.LastIndexOfAny(Tokens) - 1;\n\n    internal static bool IsLiteral")
        engine = replace_once(engine, "int behavior, bool literal, TimeSpan timeout, string originalPattern)", "int behavior, bool literal, TimeSpan timeout, string originalPattern, int prefix = -1, int suffix = -1)")
        engine = replace_once(engine, "MatchWindow(input, input.Length, pattern, singleline, behavior, ref clock);", "MatchWindow(input, input.Length, pattern, singleline, behavior, ref clock, prefix, suffix);")
        engine = replace_once(engine, "MatchWindow(input, input.Length - 1, pattern, singleline, behavior, ref clock);", "MatchWindow(input, input.Length - 1, pattern, singleline, behavior, ref clock, prefix, suffix);")
        engine = replace_once(engine, """        int behavior, ref MatchClock clock)
    {
        int text = 0""", """        int behavior, ref MatchClock clock, int preparedPrefix, int preparedSuffix)
    {
        int text = 0""")
        engine = replace_once(engine, "int prefix = pattern.IndexOfAny(Tokens);", "int prefix = preparedPrefix >= 0 ? preparedPrefix : pattern.IndexOfAny(Tokens);")
        engine = replace_once(engine, "int suffix = patternEnd - pattern.LastIndexOfAny(Tokens) - 1;", "int suffix = preparedSuffix >= 0 ? preparedSuffix : patternEnd - pattern.LastIndexOfAny(Tokens) - 1;")
    elif name == "H-014":
        engine = replace_once(engine, """        // Anchor fixed prefix/suffix before retrying the interior.""", """        if (patternEnd >= 32 && token == 0 && pattern[0] == '?' && pattern[1] == '?')
        {
            int length = 2;
            while (length < patternEnd && pattern[length] == '?') { length++; clock.Check(); }
            if (length > end || (!singleline && input.IndexOf('\\n', 0, length) >= 0)) return false;
            token = text = length;
        }
        // Anchor fixed prefix/suffix before retrying the interior.""")
    elif name == "H-013":
        engine = replace_once(engine, """        else
        {
            result = MatchWindow""", """        else if (literal && behavior != 0 && TryAsciiLiteral(input, pattern, behavior, out bool asciiResult))
            result = asciiResult;
        else
        {
            result = MatchWindow""")
        engine = replace_once(engine, "    private static bool MatchWindow(", """    private static bool TryAsciiLiteral(string input, string pattern, int behavior, out bool result)
    {
        result = false;
        if (behavior == 3) return false;
        int length = pattern.Length;
        if (input.Length != length && !(input.Length > 0 && input.Length - 1 == length && input[input.Length - 1] == '\\n')) return true;
        for (int i = 0; i < length; i++)
        {
            char a = pattern[i], b = input[i];
            if ((a | b) >= 128) return false;
            if (a != b && (uint)((a | 32) - 'a') > 25) return true;
            if (a != b && (a | 32) != (b | 32)) return true;
        }
        result = true;
        return true;
    }

    private static bool MatchWindow(""")
    elif name != "control":
        raise ValueError(name)
    return engine, wrapper
