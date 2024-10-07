namespace Moxposer.Runner;

/// <summary>
/// Represents a report of the analysis results for a collection of DLLs.
/// This report tracks the number of suspicious DLLs, successful analyses, and failed analyses.
/// </summary>
public class AnalysisReport
{
    /// <summary>
    /// Gets the total number of DLLs that were identified as suspicious during the analysis process.
    /// </summary>
    public int SuspiciousDlls { get; private set; }

    /// <summary>
    /// Gets the total number of DLLs that were successfully analyzed without any issues.
    /// </summary>
    public int AnalyzedSuccessfully { get; private set; }

    /// <summary>
    /// Gets the total number of DLLs that failed during the analysis process.
    /// </summary>
    public int FailedAnalyses { get; private set; }

    /// <summary>
    /// Increments the count of suspicious DLLs by one.
    /// </summary>
    public void IncrementTotalSuspicious() => SuspiciousDlls++;

    /// <summary>
    /// Increments the count of successfully analyzed DLLs by one.
    /// </summary>
    public void IncrementAnalyzedSuccessfully() => AnalyzedSuccessfully++;

    /// <summary>
    /// Increments the count of failed DLL analyses by one.
    /// </summary>
    public void IncrementFailedAnalyses() => FailedAnalyses++;

    /// <summary>
    /// Returns a string representation of the analysis report, summarizing the results.
    /// </summary>
    /// <returns>A formatted string showing the total number of suspicious DLLs, successful analyses, and failed analyses.</returns>
    public override string ToString() => @$"Total Suspicious DLLs: {SuspiciousDlls}
Successfully Analyzed: {AnalyzedSuccessfully}
Failed Analyses: {FailedAnalyses}";
}
