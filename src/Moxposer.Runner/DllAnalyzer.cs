using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Moxposer.Analyzer;
using Moxposer.Runner.Services;
using System.Collections.Immutable;

namespace Moxposer.Runner;

public interface IDllAnalyzer
{
    AnalysisResult AnalyzeDll(string dllPath);
}

public class DllAnalyzer : IDllAnalyzer
{
    private readonly IDecompilerService _decompilerService;

    public DllAnalyzer(IDecompilerService decompilerService)
    {
        _decompilerService = decompilerService;
    }

    public AnalysisResult AnalyzeDll(string dllPath)
    {
        var result = new AnalysisResult
        {
            DllPath = dllPath
        };

        if (_decompilerService.IsObfuscated(dllPath))
        {
            result.IsObfuscated = true;
            return result;
        }

        var decompiledCode = _decompilerService.Decompile(dllPath);
        var diagnostics = AnalyzeCode(decompiledCode);

        if (diagnostics.Any())
        {
            result.HasSuspiciousCode = true;
            result.Diagnostics.AddRange(diagnostics);
        }

        return result;
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
}
