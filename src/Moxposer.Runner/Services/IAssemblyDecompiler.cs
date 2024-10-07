using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;

namespace Moxposer.Runner.Services;

/// <summary>
/// Provides functionality to decompile assemblies and detect if they are obfuscated.
/// </summary>
public interface IAssemblyDecompiler
{
    /// <summary>
    /// Checks if the provided assembly is obfuscated by analyzing the assembly's type names.
    /// </summary>
    /// <param name="dllPath">The file path of the assembly (DLL) to check.</param>
    /// <returns>True if the assembly is suspected to be obfuscated, otherwise false.</returns>
    bool IsObfuscated(string dllPath);

    /// <summary>
    /// Decompiles the provided assembly and returns its source code as a string.
    /// </summary>
    /// <param name="dllPath">The file path of the assembly (DLL) to decompile.</param>
    /// <returns>A string containing the decompiled source code of the assembly.</returns>
    string Decompile(string dllPath);
}

/// <summary>
/// Implements the functionality to decompile assemblies and detect obfuscation.
/// </summary>
public class AssemblyDecompiler : IAssemblyDecompiler
{
    /// <inheritdoc/>
    public bool IsObfuscated(string dllPath)
    {
        var decompilerSettings = new DecompilerSettings { ThrowOnAssemblyResolveErrors = false };
        var decompiler = new CSharpDecompiler(dllPath, decompilerSettings);
        var types = decompiler.TypeSystem.MainModule.TypeDefinitions;

        // Suspect obfuscation if more than half of the types have very short names
        int suspectTypes = types.Count(type => type.Name.Length <= 2);
        return suspectTypes > types.Count() / 2;
    }

    /// <inheritdoc/>
    public string Decompile(string dllPath)
    {
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings());
        return decompiler.DecompileWholeModuleAsString();
    }
}
