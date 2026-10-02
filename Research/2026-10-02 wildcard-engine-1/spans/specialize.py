"""H-031 representation specialization, generated once from preserved source epochs."""
from pathlib import Path
import subprocess
HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
original = subprocess.check_output(["git", "show", "0c39b9d:src/Tedd.WildcardMatch/WildcardEngine.cs"], cwd=REPO).decode("utf-8-sig")
original = original.replace("internal static class WildcardEngine", "internal static partial class WildcardEngine", 1)
span = (HERE / "H-030/WildcardEngine.cs.txt").read_text(encoding="utf-8")
start = span.index("    // H-029:")
span = span[start:]
span = span.replace("    private static bool IsUnicodeNewline(char value) =>\n        value == '\\n' || value == '\\r' || value == '\\v' || value == '\\f' ||\n        value == '\\u0085' || value == '\\u2028' || value == '\\u2029';\n\n", "", 1)
assert "private static bool IsUnicodeNewline" not in span
span = span.replace("MatchClock", "SpanMatchClock")
header = """using System;
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
        ((int)options & 0x20) != 0 && NewlineOption.Supported && wildcard.IndexOf('\\v') >= 0;

"""
(REPO / "src/Tedd.WildcardMatch/WildcardEngine.cs").write_bytes(original.replace("\r\n", "\n").encode())
(REPO / "src/Tedd.WildcardMatch/WildcardEngine.Spans.cs").write_bytes((header + span).replace("\r\n", "\n").encode())
print("Preserved released string bodies and extracted tested span overloads.")
