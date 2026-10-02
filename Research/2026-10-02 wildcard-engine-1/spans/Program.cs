extern alias control;
using Baseline = control::Tedd.WildcardMatch;
using BaselineOptions = control::Tedd.WildcardOptions;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using Tedd;

int launch = int.Parse(args[0]);
if (OperatingSystem.IsWindows()) Process.GetCurrentProcess().ProcessorAffinity = (nint)(1L << 30);
var fixtures = new (string Name, string Text, string Pattern, WildcardOptions Options)[]
{
    ("empty", "", "", WildcardOptions.None),
    ("literal", "report-2026.txt", "report-2026.txt", WildcardOptions.None),
    ("simple", "a*abbaabbccddee", "*aa??cc*ee*", WildcardOptions.None),
    ("simple-ignorecase", "A*ABBAABBCCDDEE", "*aa??cc*ee*", WildcardOptions.IgnoreCase),
    ("miss", "a*abbaabbccddeef", "*aa??cc*ee", WildcardOptions.None),
    ("escaped", "a\\[.$(#)x\tYZ", "a\\[.$(#)*?YZ", WildcardOptions.None),
    ("long-literal", new string('a', 1024), new string('a', 1024), WildcardOptions.None),
    ("long-sparse", new string('a', 1024) + "z", "*" + new string('a', 1024) + "z", WildcardOptions.None),
    ("dense", string.Concat(Enumerable.Repeat("abXY", 128)), string.Concat(Enumerable.Repeat("ab??", 128)), WildcardOptions.None),
    ("unicode", "ÅİΣKabcdef", "åiσk*", WildcardOptions.IgnoreCase),
    ("newline", "a\nb\n", "a?b*", WildcardOptions.Singleline),
    ("multiline", "first\nsecond\nthird", "s?cond", (WildcardOptions)2),
    ("complex", "1234567890" + new string('x', 100) + "aabbaabbccddeeffaabbccddeeffgghhiijjkk aabbaabbccddeeffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz" + new string('x', 100) + "1234567890",
        "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*", WildcardOptions.None),
    ("question-run", new string('x', 1024), new string('?', 1024), WildcardOptions.None),
    ("question-run-lf", new string('x', 512) + "\n" + new string('x', 511), new string('?', 1024), WildcardOptions.None),
    ("question-star", "head" + new string('x', 512) + "end", "head*" + new string('?', 256) + "*end", WildcardOptions.None),
    ("retry-literal", new string('x', 2048) + "abcdefghijklmnop" + new string('x', 40), "*abcdefghijklmnop*", WildcardOptions.None),
    ("retry-near-miss", string.Concat(Enumerable.Repeat("aaaaaaaaaaaaaac", 128)), "*aaaaaaaaaaaaaab*", WildcardOptions.None),
    ("retry-lf", new string('x', 200) + "\na", "*a*", WildcardOptions.None),
    ("ascii-literal-ignorecase", new string('A', 1024), new string('a', 1024), WildcardOptions.IgnoreCase),
    ("short-literal-ignorecase", "REPORT.txt", "report.TXT", WildcardOptions.IgnoreCase),
    ("many-stars", "abc", new string('*', 1024) + "abc*", WildcardOptions.None),
    ("short-question", "axb", "a?b", WildcardOptions.None)
};
// Prime the public call paths together before the small screening windows.
// Keep subsequent paired warmup and runtime-default tiering/PGO enabled.
if (!args.Contains("--profile"))
{
    bool prime = false;
    foreach (var f in fixtures)
    {
        var a = new Baseline(f.Pattern, (BaselineOptions)f.Options);
        var b = new WildcardMatch(f.Pattern, f.Options);
        for (int i = 0; i < 20000; i++)
            prime ^= a.IsMatch(f.Text) ^ b.IsMatch(f.Text) ^ Baseline.IsMatch(f.Text, f.Pattern, (BaselineOptions)f.Options) ^
                WildcardMatch.IsMatch(f.Text, f.Pattern, f.Options) ^ b.IsMatch(f.Text.AsSpan()) ^ WildcardMatch.IsMatch(f.Text.AsSpan(), f.Pattern.AsSpan(), f.Options);
    }
    Thread.Sleep(500);
    Console.WriteLine("Priming checksum: " + prime);
}
var sliceCases = new HashSet<string> { "empty", "literal", "simple", "long-literal", "long-sparse", "dense", "unicode", "newline", "complex" };
var rows = new List<object>();
var cold = new List<object>();
bool asmOnly = args.Contains("--asm-only"), profile = args.Contains("--profile");
bool screening = args.Contains("--screening");
if (profile)
{
    bool checksum = false;
    foreach (var f in fixtures.Where(f => sliceCases.Contains(f.Name)))
    {
        var matcher = new WildcardMatch(f.Pattern, f.Options);
        string buffer = "GUARD" + f.Text + "GUARD";
        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2;
        while (Stopwatch.GetTimestamp() < until)
            for (int i = 0; i < 1024; i++) checksum ^= matcher.IsMatch(buffer.AsSpan(5, f.Text.Length));
    }
    Console.WriteLine("Profile checksum: " + checksum);
    return;
}
#if NET11_0_OR_GREATER
fixtures = Enumerable.Range(0, 5).Select(i => new[] { 63, 253, 254, 255, 1024 }[i])
    .Select(length => ("normalize-" + length, "AZ", "A" + new string('\v', length) + "?", (WildcardOptions)32)).ToArray();
#endif
foreach (var f in fixtures)
{
    string textBuffer = "GUARD" + f.Text + "GUARD";
    string patternBuffer = "GUARD" + f.Pattern + "GUARD";
    var direct = new WildcardMatch(f.Pattern, f.Options);
    var baseline = new Baseline(f.Pattern, (BaselineOptions)f.Options);
    var modes = sliceCases.Contains(f.Name)
        ? new[] { "static", "reused", "span-static", "span-reused", "sliced-static", "sliced-reused", "construction-span" }
        : new[] { "static", "reused" };
#if NET11_0_OR_GREATER
    modes = new[] { "normalization" };
    long coldStart = GC.GetAllocatedBytesForCurrentThread();
    bool coldResult = WildcardMatch.IsMatch(textBuffer.AsSpan(5, f.Text.Length), patternBuffer.AsSpan(5, f.Pattern.Length), f.Options);
    cold.Add(new { fixture = f.Name, bytes = GC.GetAllocatedBytesForCurrentThread() - coldStart, result = coldResult,
        note = "First candidate normalization call in this fresh process; may include enum and pool initialization." });
#endif
    foreach (string mode in modes)
    {
        Func<bool> a = mode switch
        {
#if CONTROL_HAS_SPANS
            "span-reused" or "sliced-reused" => () => baseline.IsMatch(textBuffer.AsSpan(5, f.Text.Length)),
            "span-static" or "sliced-static" or "normalization" => () => Baseline.IsMatch(textBuffer.AsSpan(5, f.Text.Length), patternBuffer.AsSpan(5, f.Pattern.Length), (BaselineOptions)f.Options),
            "construction-span" => () => new Baseline(patternBuffer.AsSpan(5, f.Pattern.Length), (BaselineOptions)f.Options).IsMatch(textBuffer.AsSpan(5, f.Text.Length)),
            "reused" => () => baseline.IsMatch(f.Text),
#else
            "reused" or "span-reused" => () => baseline.IsMatch(f.Text),
            "sliced-reused" => () => baseline.IsMatch(textBuffer.AsSpan(5, f.Text.Length).ToString()),
            "sliced-static" => () => Baseline.IsMatch(textBuffer.AsSpan(5, f.Text.Length).ToString(), patternBuffer.AsSpan(5, f.Pattern.Length).ToString(), (BaselineOptions)f.Options),
            "construction-span" => () => new Baseline(patternBuffer.AsSpan(5, f.Pattern.Length).ToString(), (BaselineOptions)f.Options).IsMatch(f.Text),
#endif
            _ => () => Baseline.IsMatch(f.Text, f.Pattern, (BaselineOptions)f.Options)
        };
        Func<bool> b = mode switch
        {
            "static" => () => WildcardMatch.IsMatch(f.Text, f.Pattern, f.Options),
            "reused" => () => direct.IsMatch(f.Text),
            "span-reused" or "sliced-reused" => () => direct.IsMatch(textBuffer.AsSpan(5, f.Text.Length)),
            "construction-span" => () => new WildcardMatch(patternBuffer.AsSpan(5, f.Pattern.Length), f.Options).IsMatch(textBuffer.AsSpan(5, f.Text.Length)),
            _ => () => WildcardMatch.IsMatch(textBuffer.AsSpan(5, f.Text.Length), patternBuffer.AsSpan(5, f.Pattern.Length), f.Options)
        };
        if (a() != b()) throw new Exception("Semantic mismatch: " + f.Name + "/" + mode);
        if (asmOnly) { for (int i = 0; i < 100000; i++) { a(); b(); } continue; }
        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 10;
        while (Stopwatch.GetTimestamp() < until) { a(); b(); }
        int count = 1;
        while (Measure(a, count).Ns * count < (screening ? 5_000_000 : 20_000_000) && count < 4_194_304) count *= 2;
        for (int block = 0; block < (screening ? 4 : 9); block++)
        {
            string[] order = block % 2 == 0 ? new[] { "Control", "Candidate", "Candidate", "Control" } : new[] { "Candidate", "Control", "Control", "Candidate" };
            foreach (string engine in order)
            {
                var result = Measure(engine == "Control" ? a : b, count);
                rows.Add(new { workload = f.Name, mode, engine, launch, block, count, ns = result.Ns, bytes = result.Bytes, checksum = result.Checksum });
            }
        }
    }
    Console.WriteLine("Measured " + f.Name);
}
if (asmOnly) return;
File.WriteAllText("span-results.json", JsonSerializer.Serialize(new
{
    launch, runtime = RuntimeInformation.FrameworkDescription, os = RuntimeInformation.OSDescription, cold,
    culture = CultureInfo.CurrentCulture.Name, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    serverGc = GCSettings.IsServerGC, affinity = "logical CPU 30", tiering = "runtime defaults", samples = rows,
    fixtures = fixtures.Select(f => new { name = f.Name, text = f.Text, pattern = f.Pattern, options = (int)f.Options })
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Launch {launch}: {rows.Count} samples, no filtering or overhead subtraction.");

static (double Ns, double Bytes, bool Checksum) Measure(Func<bool> action, int count)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    bool checksum = false;
    long started = Stopwatch.GetTimestamp();
    for (int i = 0; i < count; i++) checksum ^= action();
    long elapsed = Stopwatch.GetTimestamp() - started;
    return (elapsed * 1e9 / Stopwatch.Frequency / count, (double)(GC.GetAllocatedBytesForCurrentThread() - before) / count, checksum);
}
