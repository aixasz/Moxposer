using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Moxposer.Runner;

var serviceProvider = DependencyInjection.GetServiceProvider();

var rootDirectory = Directory.GetCurrentDirectory();

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(options => rootDirectory = options.Path)
    .WithNotParsed(errors =>
    {
        foreach (var error in errors)
        {
            Console.WriteLine(error.ToString());
        }
        Environment.Exit(1);
    });

var scanner = serviceProvider.GetService<IDllScanner>()
    ?? throw new InvalidOperationException($"Unable to resolve {nameof(IDllScanner)} from the service provider.");

var scannedResults = scanner.ScanProjects(rootDirectory);

bool hasIssues = false;

var dllAnalyzer = serviceProvider.GetService<IDllAnalyzer>()
    ?? throw new InvalidOperationException($"Unable to resolve {nameof(IDllAnalyzer)} from the service provider.");

var report = new AnalysisReport();
foreach (var result in scannedResults)
{
    if (result.SkippedDlls.Any())
    {
        Console.WriteLine("Skipped the following whitelisted DLLs:");
        foreach (var skippedDll in result.SkippedDlls)
        {
            Console.WriteLine(skippedDll);
        }
    }

    foreach (var dllResult in result.DllsToAnalyze)
    {
        Console.WriteLine($"Analyzing DLL: {dllResult}");

        var analysisResult = dllAnalyzer.AnalyzeDll(dllResult);

        if (analysisResult.IsObfuscated)
        {
            Console.WriteLine($"Package {analysisResult.DllPath} appears to be obfuscated.");
            hasIssues = true;
        }

        if (analysisResult.HasSuspiciousCode)
        {
            Console.WriteLine($"Suspicious usage detected in {analysisResult.DllPath}:");
            foreach (var diagnostic in analysisResult.Diagnostics)
            {
                Console.WriteLine(diagnostic);
            }
            hasIssues = true;

            report.IncrementTotalSuspicious();
        }

        if (!analysisResult.AnalyzedSuccessfully)
        {
            Console.WriteLine($"Failed during analyze in {analysisResult.DllPath}:");
            Console.WriteLine(analysisResult.ErrorMessage);
            report.IncrementFailedAnalyses();
        }
        else
        {
            report.IncrementAnalyzedSuccessfully();
        }
    }
}

Console.WriteLine(report);

if (hasIssues)
{
    Environment.Exit(1);
}