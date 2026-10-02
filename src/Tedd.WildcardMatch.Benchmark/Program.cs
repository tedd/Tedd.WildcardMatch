using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Tedd.WildcardMatchBenchmark;

var validation = Comparison.Validate();
if (args.Length > 0 && args[0] == "--validate-packages")
{
    var json = System.Text.Json.JsonSerializer.Serialize(validation,
        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    if (args.Length > 1) File.WriteAllText(args[1], json);
    Console.WriteLine(json);
    return;
}

// Keep comparative measurements in optimized code; short tiered runs can expose Tier0.
var config = DefaultConfig.Instance.AddJob(Job.Default.WithEnvironmentVariable("DOTNET_TieredCompilation", "0"));
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
