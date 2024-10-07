namespace Moxposer.Runner;

/// <summary>
/// Represents the result of scanning a project or directory for DLL files.
/// This includes the path that was scanned, the list of DLLs that were skipped, and the list of DLLs that require further analysis.
/// </summary>
public class ScannedDllResult
{
    /// <summary>
    /// Gets or sets the path of the project or directory that was scanned.
    /// </summary>
    public string ScanPath { get; set; }

    /// <summary>
    /// Gets the list of DLL files that were skipped during the scanning process.
    /// Skipped DLLs are typically those that are whitelisted or have valid signatures.
    /// </summary>
    public List<string> SkippedDlls { get; } = [];

    /// <summary>
    /// Gets the list of DLL files that require further analysis.
    /// These DLLs may have been identified as suspicious or require deeper inspection.
    /// </summary>
    public List<string> DllsToAnalyze { get; } = [];
}