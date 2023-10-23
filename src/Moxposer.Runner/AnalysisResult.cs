using Microsoft.CodeAnalysis;

namespace Moxposer.Runner;

public class AnalysisResult
{
    public string DllPath { get; set; }
    public bool IsObfuscated { get; set; }
    public bool HasSuspiciousCode { get; set; }
    public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();
}