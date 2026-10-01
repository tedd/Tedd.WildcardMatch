using BenchmarkDotNet.Running;
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

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
