
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Moxposer.Analyzer;
using System.Collections.Immutable;

namespace Moxposer.Runner;
public static class DllAnalyzer
{
    private static bool IsObfuscated(string dllPath)
    {
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
        var types = decompiler.TypeSystem.MainModule.TypeDefinitions;

        int suspectTypes = types.Count(t => t.Name.Length <= 2);
        return suspectTypes > types.Count() / 2;
    }

    private static string Decompile(string dllPath)
    {
        var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings());
        return decompiler.DecompileWholeModuleAsString();
    }

    private static IEnumerable<Diagnostic> AnalyzeCode(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create("TempAssembly")
            .AddSyntaxTrees(syntaxTree);

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new HttpClientUsageAnalyzer());

        var compilationWithAnalyzer = compilation.WithAnalyzers(analyzers);
        var diagnostics = compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;

        return diagnostics;
    }

    public static AnalysisResult AnalyzeDll(string dllPath)
    {
        var result = new AnalysisResult
        {
            DllPath = dllPath
        };

        if (IsObfuscated(dllPath))
        {
            result.IsObfuscated = true;
            return result;
        }

        var decompiledCode = Decompile(dllPath);
        var diagnostics = AnalyzeCode(decompiledCode);

        if (diagnostics.Any())
        {
            result.HasSuspiciousCode = true;
            result.Diagnostics.AddRange(diagnostics);
        }

        return result;
    }
}
