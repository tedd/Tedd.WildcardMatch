extern alias BaselineLibrary;
using OriginalWildcardMatch = BaselineLibrary::Tedd.WildcardMatch;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Horology;
using BenchmarkDotNet.Running;

public static class Variants
{
    public static string Baseline(string s) => "^" + Regex.Escape(s).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
    public static readonly char[] Specials = "\t\n\f\r #$()*+.?[\\^{|".ToCharArray();
    public static bool Expands(char c) => c is '*' or '\t' or '\n' or '\f' or '\r' or ' ' or '#' or '$' or '(' or ')' or '+' or '.' or '[' or '\\' or '^' or '{' or '|';
    public static char Escape(char c) => c switch { '\t' => 't', '\n' => 'n', '\r' => 'r', '\f' => 'f', _ => c };
    public static string Builder(string s) => Build(s, 16, false);
    public static string Preallocated(string s) => Build(s, checked(s.Length * 2 + 2), false);
    public static string Runs(string s) => Build(s, checked(s.Length * 2 + 2), true);
    private static string Build(string s, int capacity, bool runs)
    {
        var b = new StringBuilder(capacity).Append('^');
        int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bool special = c == '?' || Expands(c);
            if (runs && !special) continue;
            if (runs) b.Append(s, start, i - start);
            if (c == '*') b.Append(".*");
            else if (c == '?') b.Append('.');
            else if (special) b.Append('\\').Append(Escape(c));
            else b.Append(c);
            start = i + 1;
        }
        if (runs) b.Append(s, start, s.Length - start);
        return b.Append('$').ToString();
    }
    public static string MaxArray(string s) => Array(s, false, false);
    public static string ExactArray(string s) => Array(s, true, false);
    public static string TableArray(string s) => Array(s, false, true);
    private static readonly bool[] ExpansionTable = Enumerable.Range(0, 128).Select(i => Expands((char)i)).ToArray();
    private static string Array(string s, bool exact, bool table)
    {
        int capacity = checked(s.Length * 2 + 2);
        if (exact)
        {
            capacity = checked(s.Length + 2);
            foreach (char c in s) if (Expands(c)) capacity = checked(capacity + 1);
        }
        var buffer = new char[capacity];
        int p = 0;
        buffer[p++] = '^';
        foreach (char c in s)
        {
            if (c == '*') { buffer[p++] = '.'; buffer[p++] = '*'; }
            else if (c == '?') buffer[p++] = '.';
            else
            {
                if (table ? c < 128 && ExpansionTable[c] : Expands(c)) buffer[p++] = '\\';
                buffer[p++] = Escape(c);
            }
        }
        buffer[p++] = '$';
        return new string(buffer, 0, p);
    }
    public static string LiteralFast(string s) => s.IndexOfAny(Specials) < 0 ? "^" + s + "$" : MaxArray(s);
    public static string EscapedFusion(string s)
    {
        string escaped = Regex.Escape(s);
        var b = new StringBuilder(escaped.Length + 2).Append('^');
        for (int i = 0; i < escaped.Length; i++)
        {
            if (escaped[i] == '\\' && i + 1 < escaped.Length && escaped[i + 1] is '*' or '?')
            { b.Append(escaped[++i] == '*' ? ".*" : "."); }
            else b.Append(escaped[i]);
        }
        return b.Append('$').ToString();
    }
    public static string EmptyFast(string s) => s.Length == 0 ? "^$" : MaxArray(s);
    private static readonly char[] Wildcards = ['*', '?'];
    public static string NoWildcardFast(string s) => s.IndexOfAny(Wildcards) < 0 ? "^" + Regex.Escape(s) + "$" : TableArray(s);
    public static string SeparateSearch(string s) => s.IndexOf('*') < 0 && s.IndexOf('?') < 0 ? "^" + Regex.Escape(s) + "$" : TableArray(s);
    public static string Combined(string s) => s.Length == 0 ? "^$" : NoWildcardFast(s);
    public static string SparseFallback(string s) => Sparse(s, false);
    public static string SparseSelective(string s) => Sparse(s, true);
    public static string SparseDensity(string s) => Sparse(s, true, true);
    private static string Sparse(string s, bool selective, bool density = false)
    {
        if (s.Length < 128) return Combined(s);
        int count = 0, position = -1;
        int limit = density ? s.Length / 32 + 1 : 5;
        bool star = false, question = false;
        while (count < limit && (position = s.IndexOfAny(Wildcards, position + 1)) >= 0)
        {
            star |= s[position] == '*'; question |= s[position] == '?'; count++;
        }
        if (count >= limit) return Combined(s);
        if (!selective) return Baseline(s);
        string escaped = Regex.Escape(s);
        if (star) escaped = escaped.Replace(@"\*", ".*");
        if (question) escaped = escaped.Replace(@"\?", ".");
        return "^" + escaped + "$";
    }
    public static (string Id, Func<string, string> Convert)[] All =
    [ ("control", Baseline), ("H-001", Builder), ("H-002", Preallocated), ("H-003", MaxArray),
      ("H-004", ExactArray), ("H-005", LiteralFast), ("H-006", Runs), ("H-007", TableArray),
      ("H-008", EscapedFusion), ("H-009", EmptyFast), ("H-011", NoWildcardFast), ("H-012", SeparateSearch), ("H-013", Combined), ("H-014", SparseFallback), ("H-015", SparseSelective), ("H-016", SparseDensity) ];
}

public record Fixture(string Name, string Pattern, string Input);
public static class Fixtures
{
    public static Fixture[] All =
    [ new("empty", "", ""), new("literal", "report2026", "report2026"),
      new("simple", "*aa??cc*ee*", "a*abbaabbccddee"),
      new("miss", "prefix*missing?", "prefixxxxxxxxxx"),
      new("complex", "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*", "1234567890" + new string('x', 100) + "aabbccddeeffgghhiijjkkllmmnnooppqqrrssttuuvvwwxxyyzz" + new string('x', 100) + "1234567890"),
      new("escaped", "C:\\data\\[ab](x)+{name}.txt*?# \t\r\n\f", "C:\\data\\[ab](x)+{name}.txtXz# \t\r\n\f"),
      new("long-literal", new string('a', 1024), new string('a', 1024)),
      new("long-prefix", new string('a', 1024) + "?", new string('a', 1024) + "b"),
      new("long-suffix", "*" + new string('a', 1024), "b" + new string('a', 1024)),
      new("long-pair", "*" + new string('a', 1024) + "?", "b" + new string('a', 1024) + "z"),
      new("long-cluster", "*?*?" + new string('a', 1024), "abcd" + new string('a', 1024)),
      new("long-five-prefix", new string('a', 1024) + "?????", new string('a', 1024) + "bcdef"),
      new("long-five-suffix", "?????" + new string('a', 1024), "bcdef" + new string('a', 1024)),
      new("long-mixed", string.Concat(Enumerable.Repeat("segment.*?\\[x] ", 32)), "no-match"),
      new("unicode", "*Åßİ😀?", "valueÅßİ😀z") ];
}

public static class Program
{
    private static object? sink;
    public static void Main(string[] args)
    {
        Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1L << 30);
        if (args.Contains("--check")) { Check(); return; }
        if (args.Contains("--paired")) { Check(); Paired(int.Parse(args[1])); return; }
        if (args.Contains("--inlining")) { Check(); Inlining(); return; }
        if (args.Contains("--screen")) { Check(); Screen(args.Contains("--final")); return; }
        if (args.Contains("--profile") || args.Contains("--profile-short"))
        {
            var stop = Stopwatch.StartNew(); long operations = 0;
            while (stop.Elapsed.TotalSeconds < 12)
                foreach (var f in Fixtures.All.Take(6).Where(f => !args.Contains("--profile-short") || f.Name != "complex"))
                    for (int i = 0; i < 1024; i++) { sink = Tedd.WildcardMatch.IsMatch(f.Input, f.Pattern); operations++; }
            Console.WriteLine($"Profile operations: {operations}"); return;
        }
        if (args.Contains("--asm"))
        {
            for (int i = 0; i < 200000; i++) foreach (var f in Fixtures.All.Take(6).Where(f => f.Name != "complex"))
            { sink = Variants.Baseline(f.Pattern); sink = Variants.MaxArray(f.Pattern); sink = Variants.TableArray(f.Pattern); sink = ProductionCaller(f.Input, f.Pattern); sink = ForcedCaller(f.Input, f.Pattern); }
            Thread.Sleep(1000);
            for (int i = 0; i < 20000; i++) foreach (var f in Fixtures.All.Take(6).Where(f => f.Name != "complex")) sink = ProductionCaller(f.Input, f.Pattern);
            return;
        }
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool ProductionCaller(string input, string pattern) => Tedd.WildcardMatch.IsMatch(input, pattern);
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool ForcedCaller(string input, string pattern) => Regex.IsMatch(input, Tedd.ForcedInternalUtils.StringToWildcard(pattern), RegexOptions.None);
    public static void Check()
    {
        int count = 0;
        foreach (var v in Variants.All)
        {
            // EscapedFusion deliberately screens replacement-token interactions too.
            for (int c = 0; c <= char.MaxValue; c++)
            {
                string s = "a" + (char)c + "*?\\" + (char)c;
                if (v.Convert(s) != Variants.Baseline(s)) throw new Exception($"{v.Id} U+{c:X4}");
                if (Tedd.InternalUtils.StringToWildcard(s) != Variants.Baseline(s)) throw new Exception($"Production U+{c:X4}");
                count++;
            }
        }
        var random = new Random(19373);
        const string alphabet = "ab*?\\[](){}.+^$|# \t\r\n\fÅßİı\0";
        foreach (var v in Variants.All)
            for (int n = 0; n < 5000; n++)
            {
                string s = new(Enumerable.Range(0, random.Next(0, 150)).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
                if (v.Convert(s) != Variants.Baseline(s)) throw new Exception($"{v.Id} random {n}");
                count++;
            }
        foreach (var culture in new[] { "en-US", "tr-TR" })
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            for (int n = 0; n < 3000; n++)
            {
                string pattern = new(Enumerable.Range(0, random.Next(0, 12)).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
                string input = new(Enumerable.Range(0, random.Next(0, 18)).Select(_ => alphabet[random.Next(alphabet.Length)]).ToArray());
                var wm = new Tedd.WildcardMatch(pattern);
                if (wm.WildcardRegex != Variants.Baseline(pattern)) throw new Exception("Production translation");
                foreach (var options in new[] { RegexOptions.None, RegexOptions.IgnoreCase, RegexOptions.Singleline, RegexOptions.RightToLeft, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexOptions.IgnorePatternWhitespace })
                {
                    bool expected = Regex.IsMatch(input, Variants.Baseline(pattern), options);
                    if (Tedd.WildcardMatch.IsMatch(input, pattern, (Tedd.WildcardOptions)options) != expected) throw new Exception("Static semantics");
                    if (new Tedd.WildcardMatch(pattern, (Tedd.WildcardOptions)options, TimeSpan.FromSeconds(1)).IsMatch(input) != expected) throw new Exception("Instance semantics");
                    count++;
                }
            }
        }
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine($"Differential assertions passed: {count}");
    }
    private static (double Ns, double Bytes) Measure(Action action, int count)
    {
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        for (int i = 0; i < count; i++) action();
        long end = Stopwatch.GetTimestamp();
        return ((end - start) * (1e9 / Stopwatch.Frequency) / count, (GC.GetAllocatedBytesForCurrentThread() - allocated) / (double)count);
    }
    private static void Screen(bool final)
    {
        var rows = new List<object>();
        foreach (var f in Fixtures.All)
        {
            var regex = new Regex(Variants.Baseline(f.Pattern));
            var variants = Variants.All.Select(v => (v.Id, Action: (Action)(() => sink = v.Convert(f.Pattern)))).ToList();
            variants.Add(("regex-only", () => sink = regex.IsMatch(f.Input)));
            variants.Add(("static-control", () => sink = OriginalWildcardMatch.IsMatch(f.Input, f.Pattern)));
            variants.Add(("static-H-007", () => sink = Regex.IsMatch(f.Input, Variants.TableArray(f.Pattern))));
            variants.Add(("static-H-011", () => sink = Regex.IsMatch(f.Input, Variants.NoWildcardFast(f.Pattern))));
            variants.Add(("static-H-012", () => sink = Regex.IsMatch(f.Input, Variants.SeparateSearch(f.Pattern))));
            variants.Add(("static-H-013", () => sink = Regex.IsMatch(f.Input, Variants.Combined(f.Pattern))));
            variants.Add(("static-H-014", () => sink = Regex.IsMatch(f.Input, Variants.SparseFallback(f.Pattern))));
            variants.Add(("static-H-015", () => sink = Regex.IsMatch(f.Input, Variants.SparseSelective(f.Pattern))));
            variants.Add(("static-H-016", () => sink = Regex.IsMatch(f.Input, Variants.SparseDensity(f.Pattern))));
            if (final) variants.Add(("static-final", () => sink = Tedd.WildcardMatch.IsMatch(f.Input, f.Pattern)));
            if (final) variants.Add(("static-H-010", () => sink = ForcedCaller(f.Input, f.Pattern)));
            if (final) variants.Add(("static-noinline-control", () => sink = ProductionCaller(f.Input, f.Pattern)));
            // Boxed match results add 24 B uniformly; translator returns strings without boxing.
            foreach (var v in variants) for (int i = 0; i < 10000; i++) v.Action();
            Thread.Sleep(100);
            var counts = variants.ToDictionary(v => v.Id, v =>
            {
                int count = 256;
                while (Measure(v.Action, count).Ns * count < 10000000 && count < 1048576) count *= 2;
                return count;
            });
            for (int trial = 0; trial < 9; trial++)
                foreach (var v in trial % 2 == 0 ? variants : variants.AsEnumerable().Reverse())
                {
                    int count = counts[v.Id];
                    var m = Measure(v.Action, count);
                    rows.Add(new { workload = f.Name, variant = v.Id, trial, count, ns = m.Ns, bytes = m.Bytes });
                }
        }
        File.WriteAllText(final ? "screen-final.json" : "screen-before.json", JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Screening samples: {rows.Count}");
    }

    private static int checksumSink;
    private static void Inlining()
    {
        var rows = new List<object>();
        foreach (var f in Fixtures.All)
        {
            Func<bool> normal = () => ProductionCaller(f.Input, f.Pattern);
            Func<bool> forced = () => ForcedCaller(f.Input, f.Pattern);
            var warmup = Stopwatch.StartNew();
            while (warmup.ElapsedMilliseconds < 300) { MeasureCall(normal, 256); MeasureCall(forced, 256); }
            int count = 256;
            while (Math.Min(MeasureCall(normal, count).Ns, MeasureCall(forced, count).Ns) * count < 10000000 && count < 1048576) count *= 2;
            for (int trial = 0; trial < 9; trial++)
            foreach (string variant in trial % 2 == 0 ? new[] { "normal", "forced" } : new[] { "forced", "normal" })
            {
                var measured = MeasureCall(variant == "normal" ? normal : forced, count);
                rows.Add(new { workload = f.Name, variant, trial, count, ns = measured.Ns, bytes = measured.Bytes });
            }
        }
        File.WriteAllText("inlining.json", JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Inlining samples: {rows.Count}; wrappers both call Regex.IsMatch with explicit RegexOptions.None");
    }
    private static (double Ns, double Bytes) MeasureCall(Func<bool> call, int count)
    {
        int checksum = 0;
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        for (int i = 0; i < count; i++) checksum += call() ? 1 : 0;
        long end = Stopwatch.GetTimestamp();
        checksumSink = checksum;
        return ((end - start) * (1e9 / Stopwatch.Frequency) / count, (GC.GetAllocatedBytesForCurrentThread() - allocated) / (double)count);
    }
    private static void Paired(int launch)
    {
        var rows = new List<object>();
        var overhead = MeasureCall(() => true, 1048576);
        foreach (bool construction in new[] { false, true })
        foreach (var f in Fixtures.All.Where(f => !construction || f.Name is "simple" or "long-mixed"))
        {
            Func<bool> before = construction ? () => new OriginalWildcardMatch(f.Pattern).IsMatch(f.Input) : () => OriginalWildcardMatch.IsMatch(f.Input, f.Pattern);
            Func<bool> after = construction ? () => new Tedd.WildcardMatch(f.Pattern).IsMatch(f.Input) : () => Tedd.WildcardMatch.IsMatch(f.Input, f.Pattern);
            if (before() != after()) throw new Exception("Paired fixture correctness");
            var warmup = Stopwatch.StartNew();
            while (warmup.ElapsedMilliseconds < 500) { MeasureCall(before, 256); MeasureCall(after, 256); }
            int count = 256;
            while (Math.Min(MeasureCall(before, count).Ns, MeasureCall(after, count).Ns) * count < 20000000 && count < 1048576) count *= 2;
            // ABBA / BAAB blocks counter temporal drift; useful results remain observable.
            for (int block = 0; block < 9; block++)
            foreach (int index in (block + launch) % 2 == 0 ? new[] { 0, 1, 1, 0 } : new[] { 1, 0, 0, 1 })
            {
                var measured = MeasureCall(index == 0 ? before : after, count);
                rows.Add(new { workload = f.Name, kind = construction ? "construction" : "static", variant = index == 0 ? "Before" : "After", launch, block, count, ns = measured.Ns, bytes = measured.Bytes });
            }
        }
        File.WriteAllText("paired.json", JsonSerializer.Serialize(new { launch, overheadNs = overhead.Ns, samples = rows }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Paired samples: {rows.Count}; delegate/loop calibration {overhead.Ns:F2} ns/call");
    }
}

[MemoryDiagnoser]
[Config(typeof(AcceptanceConfig))]
public class Acceptance
{
    [Params("empty", "literal", "simple", "miss", "escaped", "unicode", "complex", "long-literal", "long-mixed", "long-prefix", "long-suffix", "long-pair", "long-cluster", "long-five-prefix", "long-five-suffix")]
    public string Case { get; set; } = "simple";
    private Fixture fixture = null!;
    [GlobalSetup] public void Setup() => fixture = Fixtures.All.Single(f => f.Name == Case);
    [Benchmark(Baseline = true)] public bool Before() => OriginalWildcardMatch.IsMatch(fixture.Input, fixture.Pattern);
    [Benchmark] public bool After() => Tedd.WildcardMatch.IsMatch(fixture.Input, fixture.Pattern);
}

public class AcceptanceConfig : ManualConfig
{
    public AcceptanceConfig() => AddJob(Job.Default.WithId("net10-cpu30")
        .WithLaunchCount(2).WithWarmupCount(5).WithIterationCount(10)
        .WithIterationTime(TimeInterval.FromMilliseconds(300)).WithAffinity(new IntPtr(1L << 30)));
}

public sealed class BaselineInstance
{
    private readonly Regex regex;
    public string Wildcard { get; }
    public string WildcardRegex { get; }
    public BaselineInstance(string pattern)
    {
        Wildcard = pattern;
        WildcardRegex = Variants.Baseline(pattern);
        regex = new Regex(WildcardRegex);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsMatch(string input) => regex.IsMatch(input);
}

[MemoryDiagnoser]
[Config(typeof(AcceptanceConfig))]
public class ConstructionAcceptance
{
    [Params("simple", "long-mixed")]
    public string Case { get; set; } = "simple";
    private Fixture fixture = null!;
    [GlobalSetup] public void Setup() => fixture = Fixtures.All.Single(f => f.Name == Case);
    [Benchmark(Baseline = true)] public bool Before() => new OriginalWildcardMatch(fixture.Pattern).IsMatch(fixture.Input);
    [Benchmark] public bool After() => new Tedd.WildcardMatch(fixture.Pattern).IsMatch(fixture.Input);
}
