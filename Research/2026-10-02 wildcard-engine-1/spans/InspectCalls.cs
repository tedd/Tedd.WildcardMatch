using System.Reflection;
using System.Reflection.Emit;

var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(o => unchecked((ushort)o.Value));
var control = Assembly.LoadFrom(args[0]);
var candidate = Assembly.LoadFrom(args[1]);
var rig = Assembly.LoadFrom(args[2]);
string Normalize(MethodBase method)
{
    byte[] il = method.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>();
    var lines = new List<string>();
    int i = 0;
    while (i < il.Length)
    {
        ushort key = il[i++];
        if (key == 0xfe) key = (ushort)(0xfe00 | il[i++]);
        OpCode opcode = opcodes[key];
        string operand = "";
        int size = opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + BitConverter.ToInt32(il, i) * 4,
            _ => 4
        };
        if (opcode.OperandType is OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineType or OperandType.InlineTok)
        {
            var member = method.Module.ResolveMember(BitConverter.ToInt32(il, i), method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null)!;
            operand = member.DeclaringType?.FullName + ":" + member;
        }
        else if (opcode.OperandType == OperandType.InlineString) operand = method.Module.ResolveString(BitConverter.ToInt32(il, i));
        else operand = Convert.ToHexString(il.AsSpan(i, size));
        lines.Add(opcode.Name + " " + operand);
        i += size;
    }
    return string.Join('\n', lines);
}
foreach (string typeName in new[] { "Tedd.WildcardEngine", "Tedd.WildcardEngine+MatchClock", "Tedd.WildcardMatch" })
{
    var before = control.GetType(typeName)!;
    var after = candidate.GetType(typeName)!;
    foreach (var method in before.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    {
        if (method.IsGenericMethod) continue;
        var equivalent = after.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Single(m => m.ToString() == method.ToString());
        bool same = Normalize(method) == Normalize(equivalent) && method.GetMethodImplementationFlags() == equivalent.GetMethodImplementationFlags();
        Console.WriteLine((same ? "IDENTICAL " : "DIFFERENT ") + typeName + ":" + method);
    }
}
foreach (var type in rig.GetTypes())
    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
    {
        if (method.GetMethodBody() == null || method.IsGenericMethod) continue;
        var calls = Normalize(method).Split('\n').Where(l => l.StartsWith("call") && l.Contains("Tedd.WildcardMatch:") && l.Contains("IsMatch"));
        foreach (string call in calls) Console.WriteLine("RIG " + method.Name + " " + call);
    }
