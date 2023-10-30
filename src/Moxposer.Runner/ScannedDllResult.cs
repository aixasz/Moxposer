namespace Moxposer.Runner;

public class ScannedDllResult
{
    public string ScanPath { get; set; }
    public List<string> SkippedDlls { get; } = new List<string>();
    public List<string> DllsToAnalyze { get; } = new List<string>();
}