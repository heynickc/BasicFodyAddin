using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class WeaverTests
{
    Assembly assembly;
    string newAssemblyPath;
    string assemblyPath;

    [TestFixtureSetUp]
    public void Setup()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
        assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

        newAssemblyPath = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssemblyPath);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition
        };

        weavingTask.Execute();
        moduleDefinition.Write(newAssemblyPath);

        assembly = Assembly.LoadFile(newAssemblyPath);
    }

    [Test]
    public void Pre_weaved_exception_gets_swallowed()
    {
        var type = assembly.GetType("AssemblyToProcess.OnException");
        var instance = (dynamic) Activator.CreateInstance(type);

        Assert.DoesNotThrow(() => instance.Swallowed_exception());
    }

    [Test]
    public void Expected_weaved_method_throws()
    {
        var type = assembly.GetType("AssemblyToProcess.OnException");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.Throws<Exception>(() => instance.Expected_unswallowed_exception());
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assemblyPath,newAssemblyPath);
    }
#endif
}