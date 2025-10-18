using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        WriteInfo("ObjectFactory.Fody: Starting IL weaving");

        var newobjCount = 0;

        foreach (var type in ModuleDefinition.Types)
        {
            newobjCount += ProcessType(type);
        }

        WriteInfo($"ObjectFactory.Fody: Found {newobjCount} newobj instructions");
    }

    private int ProcessType(TypeDefinition type)
    {
        var count = 0;

        foreach (var method in type.Methods)
        {
            if (!method.HasBody)
                continue;

            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Newobj)
                {
                    count++;
                    var constructor = instruction.Operand as MethodReference;
                    var typeName = constructor?.DeclaringType.FullName ?? "Unknown";
                    WriteInfo($"  Found: new {typeName}() in {type.FullName}.{method.Name}");
                }
            }
        }

        foreach (var nestedType in type.NestedTypes)
        {
            count += ProcessType(nestedType);
        }

        return count;
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }
}
