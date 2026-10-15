using System.Text.RegularExpressions;
using DotNetLab.Contracts;
using DotNetLab.Harness;

var registry = new ScenarioRegistry();
foreach (string shape in new[] { "dense-4096", "dense-16384", "ordinary", "newline" })
    registry.Register(new MatchScenario(shape));
var host = new HarnessHost(registry, args[1]);
if (args[0] == "--harness") { await host.RunOwnedAsync(); return; }
using var cancellation = new CancellationTokenSource();
var listener = host.ListenAsync(cancellation.Token);
Console.WriteLine(Wire.Serialize(host.Target)); Console.Out.Flush();
await Console.In.ReadLineAsync(); cancellation.Cancel(); await listener;

sealed class MatchScenario : ISynchronousScenario
{
    readonly string shape;
    readonly string pattern = "*" + new string('a', 70) + "?b*";
    readonly string[] inputs;
    readonly bool[] expected;
    readonly Tedd.WildcardMatch matcher;
#if EXPLORATORY
    readonly Control.WildcardMatch control;
#endif
    string variant = "baseline";
    long checksum, expectedChecksum;
    CancellationToken cancellation;
    public MatchScenario(string shape)
    {
        this.shape = shape;
        int length = shape == "dense-16384" ? 16384 : 4096;
        inputs = shape switch {
            "ordinary" => [new string('x', 256), new string('a', 70) + "zb", "prefix" + new string('a', 70) + "zb-tail", "aab"],
            "newline" => ["\n" + new string('a', length), new string('a', length) + "\n", new string('a', 70) + "\nb", new string('a', 70) + "zb\n"],
            _ => [new string('a', length), new string('a', length) + "zb", new string('a', length) + "zc", new string('a', length) + "zb\n"]
        };
        var oracle = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.None, TimeSpan.FromSeconds(5));
        expected = inputs.Select(input => oracle.IsMatch(input)).ToArray(); matcher = new(pattern);
#if EXPLORATORY
        control = new(pattern);
#endif
    }
    public ScenarioDescriptor Describe() => new("wildcard-" + shape, "1", Wire.Hash(new { pattern, inputs, expected }),
        ResetLevel.LogicalState, ThreadAffinity.Any, false, "One public instance IsMatch call; four inputs cycled in fixed order", ["baseline", "candidate"]);
    public void Prepare(ScenarioContext context)
    {
        variant = context.Variant; cancellation = context.Cancellation;
        // Differential oracle checks both string and span entry points outside timing.
        for (int i = 0; i < inputs.Length; i++)
        {
            if (matcher.IsMatch(inputs[i]) != expected[i] || matcher.IsMatch(inputs[i].AsSpan()) != expected[i])
                throw new InvalidOperationException("Candidate oracle mismatch");
#if EXPLORATORY
            if (control.IsMatch(inputs[i]) != expected[i] || control.IsMatch(inputs[i].AsSpan()) != expected[i])
                throw new InvalidOperationException("Control oracle mismatch");
#endif
        }
    }
    public void Reset(ResetContext context) { checksum = expectedChecksum = 0; }
    public long ExecuteBatch(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if ((i & 1023) == 0) cancellation.ThrowIfCancellationRequested();
            int index = i & 3;
#if EXPLORATORY
            bool value = variant == "baseline" ? control.IsMatch(inputs[index]) : matcher.IsMatch(inputs[index]);
#else
            bool value = matcher.IsMatch(inputs[index]);
#endif
            checksum += value ? 1 : 0; expectedChecksum += expected[index] ? 1 : 0;
        }
        return count;
    }
    public ValidationResult Validate() => new(checksum == expectedChecksum);
    public void Cleanup() { }
}
