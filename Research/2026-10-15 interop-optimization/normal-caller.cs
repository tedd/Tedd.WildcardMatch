
using DotNetLab.Contracts;
using DotNetLab.Harness;
using System.Diagnostics;
Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1L << 30);
Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
var registry = new ScenarioRegistry();
foreach (var shape in new[]{"Literal","Simple","MultiStar","LongText","Dense4096","Dense16384","Newline","Diverse","GeneralShort","FixedMedium","FixedLong","LiteralLong"})
 foreach (bool reuse in new[]{true,false}) registry.Register(new MatchScenario(shape,reuse));
registry.Register(new PortfolioScenario());
var host = new HarnessHost(registry,args[1]);
if(args[0]=="--harness") {await host.RunOwnedAsync();return;}
using var cancel = new CancellationTokenSource();
var listener=host.ListenAsync(cancel.Token);
Console.WriteLine(Wire.Serialize(host.Target));Console.Out.Flush();
while(!File.Exists(args[2]))await Task.Delay(200);
cancel.Cancel();await listener;
sealed class MatchScenario : ISynchronousScenario {
 readonly string shape; readonly bool reuse; readonly string pattern; readonly string[] inputs;readonly bool[] expected;
 readonly Func<string,bool> match;
 string variant="baseline";long checksum,expectedChecksum;CancellationToken cancel;
 public MatchScenario(string shape,bool reuse){this.shape=shape;this.reuse=reuse;
 pattern=shape switch{"Literal"=>"report-2026.txt","Simple"=>"report-??.txt","MultiStar"=>"*a?c*e*","LongText"=>"*alpha*beta??*omega*","Diverse"=>"*a?c*e*","GeneralShort"=>"*a*b*c*","FixedMedium"=>"abcdefghijklmnop?","FixedLong"=>new string('a',512)+"?z","LiteralLong"=>new string('a',512)+"z",_=>"*"+new string('a',70)+"?b*"};
 int n=shape=="Dense16384"?16384:4096;
 inputs=shape switch{
 "Literal"=>["report-2026.txt","report-2025.txt","xreport-2026.txt","report-2026.txtx"],
 "Simple"=>["report-01.txt","report-aa.txt","report-1.txt","report-001.txt"],
 "MultiStar"=>["xabcyef","abce","abcd","ababcd","aZc-long-e-tail","acex"],
 "LongText"=>[new string('x',512)+"alpha-middle-beta42-tail-omega"+new string('z',512),new string('x',512)+"alpha-middle-beta42-tail-delta"+new string('z',512),new string('x',512)+"alpha-beta12-omega"+new string('z',512),new string('x',512)+"alpha-beta1-omega"+new string('z',512)],
 "Newline"=>["\n"+new string('a',n),new string('a',n)+"\n",new string('a',70)+"\nb",new string('a',70)+"zb\n"],
 "GeneralShort"=>["abcdef","abc","def","ac","xaxbxc","abbbc","a\nbc","abcdef\n",new string('x',32)+"abc"],
 "FixedMedium"=>["abcdefghijklmnopX","abcdefghijklmnop\n","abcdefghijklmnopX\n","xbcdefghijklmnopY","abcdefghijklmnop"],
 "FixedLong"=>[new string('a',512)+"Qz",new string('a',512)+"Qy",new string('a',512)+"\nz",new string('a',512)+"Qz\n"],
 "LiteralLong"=>[new string('a',512)+"z",new string('a',512)+"y","x"+new string('a',512)+"z",new string('a',512)+"z\n"],
 "Diverse"=>Enumerable.Range(0,256).Select(i=>new string('x',i%64)+(i%4==0?"aZce":i%4==1?"a\nce":i%4==2?"aZcxe\n":"aZcxx")+new string('z',i%17)).ToArray(),
 _=>[new string('a',n),new string('a',n)+"zb",new string('a',n)+"zc",new string('a',n)+"zb\n"]};
 expected=inputs.Select(s=>Oracle(s,pattern)).ToArray();
 if(reuse){var matcher=new Tedd.WildcardMatch(pattern);match=matcher.IsMatch;}else match=input=>Tedd.WildcardMatch.IsMatch(input,pattern);
 }
 internal static bool Oracle(string s,string p){bool Full(int end){var a=new bool[end+1];a[0]=true;foreach(char c in p){var b=new bool[end+1];b[0]=c=='*'&&a[0];for(int i=1;i<=end;i++)b[i]=c=='*'?a[i]||(b[i-1]&&s[i-1]!='\n'):a[i-1]&&(c=='?'?s[i-1]!='\n':c==s[i-1]);a=b;}return a[end];}return Full(s.Length)||(s.EndsWith('\n')&&Full(s.Length-1));}
 internal string Pattern => pattern; internal string[] Inputs => inputs;
 public ScenarioDescriptor Describe()=>new("interop-"+(reuse?"reused-":"static-")+shape,"2",Wire.Hash(new{pattern,inputs,expected}),ResetLevel.LogicalState,ThreadAffinity.Any,false,"One selected public API call through a prepared delegate; corpus cycled; checksum observable",["baseline","candidate"]);
 public void Prepare(ScenarioContext context){cancel=context.Cancellation;for(int i=0;i<inputs.Length;i++)if(match(inputs[i])!=expected[i])throw new Exception("Delegate oracle mismatch");}
 public void Reset(ResetContext context){checksum=expectedChecksum=0;}
 public long ExecuteBatch(int count){for(int i=0;i<count;i++){if((i&4095)==0)cancel.ThrowIfCancellationRequested();int j=i%inputs.Length;checksum+=match(inputs[j])?1:0;expectedChecksum+=expected[j]?1:0;}return count;}
 public ValidationResult Validate()=>new(checksum==expectedChecksum);public void Cleanup(){}
}

sealed class PortfolioScenario : ISynchronousScenario {
 readonly string[] patterns;readonly string[] inputs=new string[1024];readonly bool[] expected=new bool[1024];readonly Func<string,bool>[] matchers=new Func<string,bool>[1024];long checksum,expectedChecksum;CancellationToken cancel;
 public PortfolioScenario(){var shapes=new[]{"Literal","Simple","MultiStar","LongText"};patterns=new string[4];for(int k=0;k<4;k++){var fixture=new MatchScenario(shapes[k],true);patterns[k]=fixture.Pattern;var matcher=new Tedd.WildcardMatch(patterns[k]);Func<string,bool> match=matcher.IsMatch;for(int j=0;j<256;j++){int index=k*256+j;inputs[index]=fixture.Inputs[j%fixture.Inputs.Length];expected[index]=MatchScenario.Oracle(inputs[index],patterns[k]);matchers[index]=match;}}}
 public ScenarioDescriptor Describe()=>new("interop-reused-Portfolio","2",Wire.Hash(new{patterns,inputs,expected}),ResetLevel.LogicalState,ThreadAffinity.Any,false,"One public reused match through a prepared delegate; each original fixture occupies 256 rows in a fixed 1024-row portfolio",["baseline","candidate"]);
 public void Prepare(ScenarioContext context){cancel=context.Cancellation;for(int i=0;i<1024;i++)if(matchers[i](inputs[i])!=expected[i])throw new Exception("Portfolio oracle mismatch");}
 public void Reset(ResetContext context){checksum=expectedChecksum=0;}
 public long ExecuteBatch(int count){for(int i=0;i<count;i++){if((i&4095)==0)cancel.ThrowIfCancellationRequested();int j=i&1023;checksum+=matchers[j](inputs[j])?1:0;expectedChecksum+=expected[j]?1:0;}return count;}
 public ValidationResult Validate()=>new(checksum==expectedChecksum);public void Cleanup(){}
}
