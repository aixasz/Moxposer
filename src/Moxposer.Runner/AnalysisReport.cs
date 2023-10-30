namespace Moxposer.Runner;

public class AnalysisReport
{
    public int SuspiciousDlls { get; private set; }
    public int AnalyzedSuccessfully { get; private set; }
    public int FailedAnalyses { get; private set; }

    public void IncrementTotalSuspicious() => SuspiciousDlls++;
    public void IncrementAnalyzedSuccessfully() => AnalyzedSuccessfully++;
    public void IncrementFailedAnalyses() => FailedAnalyses++;

    public override string ToString() => @$"Total Suspicious DLLs: {SuspiciousDlls}
Successfully Analyzed: {AnalyzedSuccessfully}
Failed Analyses: {FailedAnalyses}";
}