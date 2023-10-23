using CommandLine;
using Moxposer.Runner;

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

var scanner = new DllScanner();
var scannedResults = scanner.ScanProjects(rootDirectory);

bool hasIssues = false;

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
        Console.WriteLine($"Analyzing project: {result.ProjectPath}");

        var analysisResult = DllAnalyzer.AnalyzeDll(dllResult);

        if (analysisResult.IsObfuscated)
        {
            Console.WriteLine($"Package {analysisResult.DllPath} appears to be obfuscated.");
            hasIssues = true;
        }
        else if (analysisResult.HasSuspiciousCode)
        {
            Console.WriteLine($"Suspicious usage detected in {analysisResult.DllPath}:");
            foreach (var diagnostic in analysisResult.Diagnostics)
            {
                Console.WriteLine(diagnostic);
            }
            hasIssues = true;
        }
    }
}

if (hasIssues)
{
    Environment.Exit(1);
}