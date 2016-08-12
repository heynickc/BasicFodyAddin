using System;
using System.Linq;
using BasicFodyAddin.Fody;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public class ModuleWeaver
{
    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    TypeSystem typeSystem;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    public void Execute()
    {
        typeSystem = ModuleDefinition.TypeSystem;
        var allTypes = ModuleDefinition.GetAllTypes();
        foreach (var type in allTypes)
        {
            var allMethods = type.GetMethods();
            foreach (var method in allMethods)
            {
                CustomAttribute unSwallowAttribute;
                if (!TryGetCustomAttribute(method, "UnSwallowExceptions.Fody.UnSwallowExceptionsAttribute", out unSwallowAttribute)) continue;

                Console.Write("UnSwallowException attribute found");
                ProcessMethodDefinition(method);
            }
        }
    }

    private bool TryGetCustomAttribute(MethodDefinition method, string attributeType, out CustomAttribute result)
    {
        result = null;
        if (!method.HasCustomAttributes)
            return false;

        foreach (CustomAttribute attribute in method.CustomAttributes)
        {
            if (attribute.AttributeType.FullName != attributeType)
                continue;

            result = attribute;
            return true;
        }

        return false;
    }

    private void ProcessMethodDefinition(MethodDefinition method)
    {
        MethodBody body = method.Body;
        body.SimplifyMacros();
        ILProcessor ilProcessor = body.GetILProcessor();

        if (body.HasExceptionHandlers)
        {
            foreach (var exceptionHandler in body.ExceptionHandlers)
            {
                Console.WriteLine("TryStart: {0} {1}", exceptionHandler.TryStart.OpCode, exceptionHandler.TryStart.Operand);
                Console.WriteLine("TryStart: {0} {1}", exceptionHandler.TryStart.Next.OpCode, exceptionHandler.TryStart.Next.Operand);
                Console.WriteLine("TryEnd: {0} {1}", exceptionHandler.TryEnd.OpCode, exceptionHandler.TryEnd.Operand);
                Console.WriteLine("TryEnd: {0} {1}", exceptionHandler.TryEnd.Next.OpCode, exceptionHandler.TryEnd.Next.Operand);
                Console.WriteLine("HandlerStart: {0} {1}", exceptionHandler.HandlerStart.Next.OpCode, exceptionHandler.HandlerStart.Next.Operand);
                Console.WriteLine("HandlerEnd: {0} {1}", exceptionHandler.HandlerEnd.Next.OpCode, exceptionHandler.HandlerEnd.Next.Operand);
            }
            //ilProcessor.Remove(body.ExceptionHandlers[0].TryStart);
            //ilProcessor.Remove(body.ExceptionHandlers[0].TryEnd);
            //ilProcessor.Remove(body.ExceptionHandlers[0].HandlerStart.Previous, Instruction.Create(OpCodes.Nop));
            //ilProcessor.Remove(body.ExceptionHandlers[0].HandlerEnd.Next, Instruction.Create(OpCodes.Nop));
        }
        body.InitLocals = true;
        body.OptimizeMacros();
    }
}