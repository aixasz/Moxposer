using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Moxposer.Analyzer;
using Moxposer.Runner.Services;
using System.Collections.Immutable;

namespace Moxposer.Runner;

/// <summary>
/// Provides methods for analyzing DLL files to detect obfuscation and potential security risks or suspicious code patterns.
/// </summary>
public interface IDllAnalyzer
{
    /// <summary>
    /// Analyzes the specified DLL for security issues, including obfuscation and suspicious code patterns.
    /// </summary>
    /// <param name="dllPath">The file path of the DLL to analyze.</param>
    /// <returns>An <see cref="AnalysisResult"/> containing the results of the security analysis.</returns>
    AnalysisResult AnalyzeDll(string dllPath);
}

/// <summary>
/// Implements the functionality for analyzing DLL files to detect obfuscation and potential security risks.
/// </summary>
public class DllAnalyzer : IDllAnalyzer
{
    private readonly IAssemblyDecompiler decompilerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DllAnalyzer"/> class.
    /// </summary>
    /// <param name="decompilerService">The decompiler service used to decompile and analyze the DLL.</param>
    public DllAnalyzer(IAssemblyDecompiler decompilerService)
    {
        this.decompilerService = decompilerService;
    }

    /// <summary>
    /// Analyzes the specified DLL for obfuscation and suspicious code patterns by decompiling the code.
    /// </summary>
    /// <param name="dllPath">The file path of the DLL to analyze.</param>
    /// <returns>An <see cref="AnalysisResult"/> containing the analysis results, such as obfuscation detection and potential security risks.</returns>
    public AnalysisResult AnalyzeDll(string dllPath)
    {
        var result = new AnalysisResult
        {
            DllPath = dllPath,
            AnalyzedSuccessfully = true
        };

        try
        {
            // Check if the DLL is obfuscated
            if (decompilerService.IsObfuscated(dllPath))
            {
                result.IsObfuscated = true;
                return result; // Early return if obfuscated
            }

            // Decompile the DLL and analyze the decompiled code
            var decompiledCode = decompilerService.Decompile(dllPath);
            var diagnostics = AnalyzeCode(decompiledCode);

            if (diagnostics.Any())
            {
                result.HasSuspiciousCode = true;
                result.Diagnostics.AddRange(diagnostics);
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that may occur during analysis
            result.AnalyzedSuccessfully = false;
            result.ErrorMessage = $"Error analyzing DLL: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Analyzes the decompiled code for potential security risks or suspicious usage patterns.
    /// </summary>
    /// <param name="code">The decompiled code to analyze.</param>
    /// <returns>An enumerable collection of <see cref="Diagnostic"/> objects representing suspicious code patterns.</returns>
    private static IEnumerable<Diagnostic> AnalyzeCode(string code)
    {
        // Parse the code into a syntax tree for further analysis
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Create a compilation for the syntax tree
        var compilation = CSharpCompilation.Create("TempAssembly")
            .AddSyntaxTrees(syntaxTree);

        // Define a set of diagnostic analyzers to use
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new HttpClientUsageAnalyzer());

        // Analyze the code and return any diagnostics found
        var compilationWithAnalyzer = compilation.WithAnalyzers(analyzers);
        var diagnostics = compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;

        return diagnostics;
    }
}
