extern alias control;
using Baseline = control::Tedd.WildcardMatch;
using BaselineOptions = control::Tedd.WildcardOptions;
using System.Diagnostics;
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
        "*1*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*0*", WildcardOptions.None)
};
fixtures = fixtures.Concat(new (string Name, string Text, string Pattern, WildcardOptions Options)[]
{
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
}).ToArray();
var rows = new List<object>();
if (args.Contains("--profile"))
{
    bool checksum = false;
    foreach (var f in fixtures)
    {
        var matcher = new WildcardMatch(f.Pattern, f.Options);
        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2;
        while (Stopwatch.GetTimestamp() < until)
            for (int i = 0; i < 1024; i++) checksum ^= matcher.IsMatch(f.Text);
    }
    Console.WriteLine("Profile checksum: " + checksum);
    return;
}
if (args.Contains("--asm-only"))
{
    bool checksum = false;
    foreach (var f in fixtures)
    {
        var direct = new WildcardMatch(f.Pattern, f.Options);
        for (int i = 0; i < 10000; i++) checksum ^= direct.IsMatch(f.Text) ^ WildcardMatch.IsMatch(f.Text, f.Pattern, f.Options);
    }
    Console.WriteLine("Native-code warmup checksum: " + checksum);
    return;
}
string[] modes = new[] { "static", "reused", "construction" };
bool acceptance = args.Contains("--acceptance");
foreach (var f in fixtures)
{
    var direct = new WildcardMatch(f.Pattern, f.Options);
    var regex = new Baseline(f.Pattern, (BaselineOptions)f.Options);
    foreach (string mode in modes)
    {
        var compiledRegex = mode == "reused-compiled" ? new Baseline(f.Pattern, (BaselineOptions)(f.Options | WildcardOptions.Compiled)) : null;
        var compiledDirect = mode == "reused-compiled" ? new WildcardMatch(f.Pattern, f.Options | WildcardOptions.Compiled) : null;
        Func<bool> a = mode switch
        {
            "static" => () => Baseline.IsMatch(f.Text, f.Pattern, (BaselineOptions)f.Options),
            "reused" => () => regex.IsMatch(f.Text),
            "reused-compiled" => () => compiledRegex!.IsMatch(f.Text),
            _ => () => new Baseline(f.Pattern, (BaselineOptions)f.Options).IsMatch(f.Text)
        };
        Func<bool> b = mode switch
        {
            "static" => () => WildcardMatch.IsMatch(f.Text, f.Pattern, f.Options),
            "reused" => () => direct.IsMatch(f.Text),
            "reused-compiled" => () => compiledDirect!.IsMatch(f.Text),
            _ => () => new WildcardMatch(f.Pattern, f.Options).IsMatch(f.Text)
        };
        if (a() != b()) throw new Exception("Semantic mismatch: " + f.Name);
        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 10;
        while (Stopwatch.GetTimestamp() < until) { a(); b(); }
        int count = 1;
        while (Measure(a, count).Ns * count < (acceptance ? 20_000_000 : 5_000_000) && count < 4_194_304) count *= 2;
        for (int block = 0; block < (acceptance ? 9 : 4); block++)
        {
            string[] order = block % 2 == 0 ? new[] { "Control", "Candidate", "Candidate", "Control" } : new[] { "Candidate", "Control", "Control", "Candidate" };
            foreach (string engine in order)
            {
                var result = Measure(engine == "Control" ? a : b, count);
                rows.Add(new { workload = f.Name, mode, engine, launch, block, count, ns = result.Ns, bytes = result.Bytes, checksum = result.Checksum });
            }
        }
    }
}
File.WriteAllText("optimization-results.json", JsonSerializer.Serialize(new
{
    launch, runtime = RuntimeInformation.FrameworkDescription, os = RuntimeInformation.OSDescription,
    culture = System.Globalization.CultureInfo.CurrentCulture.Name,
    fixtures = fixtures.Select(f => new { name = f.Name, text = f.Text, pattern = f.Pattern, options = (int)f.Options }),
    architecture = RuntimeInformation.ProcessArchitecture.ToString(), serverGc = GCSettings.IsServerGC,
    affinity = "logical CPU 30", tiering = "runtime defaults", samples = rows
}, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Launch {launch}: {rows.Count} raw samples, 23 fixtures, {string.Join('/', modes)}, ABBA/BAAB, no filtering.");

static (double Ns, double Bytes, bool Checksum) Measure(Func<bool> action, int count)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    bool checksum = false;
    long started = Stopwatch.GetTimestamp();
    for (int i = 0; i < count; i++) checksum ^= action();
    long elapsed = Stopwatch.GetTimestamp() - started;
    long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
    return (elapsed * 1e9 / Stopwatch.Frequency / count, (double)bytes / count, checksum);
}
