using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

namespace Moxposer.Runner.Services;

public interface IDecompilerService
{
    bool IsObfuscated(string dllPath);
    string Decompile(string dllPath);
}

public class DecompilerService : IDecompilerService
{
    public bool IsObfuscated(string dllPath)
    {
        var decompilerSettings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
        var decompiler = new CSharpDecompiler(dllPath, decompilerSettings);
        var types = decompiler.TypeSystem.MainModule.TypeDefinitions;

        int suspectTypes = types.Count(t => t.Name.Length <= 2);
        return suspectTypes > types.Count() / 2;
    }

    public string Decompile(string dllPath)
    {
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings());
        return decompiler.DecompileWholeModuleAsString();
    }
}