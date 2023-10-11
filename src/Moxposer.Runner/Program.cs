using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Moxposer.Analyzer;
using System.Collections.Immutable;
using System.Xml.Linq;

var rootDirectory = Directory.GetCurrentDirectory();
var csprojFiles = FindCsprojFiles(rootDirectory).ToList();

if (csprojFiles.Any())
{

    foreach (var projectPath in csprojFiles)
    {
        Console.WriteLine($"Analyzing project: {projectPath}");

        var whitelist = ExtractWhitelistedPackages(projectPath);

        // Let's assume you have a function GetDllsInProject or similar to get all DLLs associated with the project
        foreach (var dllPath in GetDllsInProject(projectPath))
        {
            var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);

            // If the DLL is in the whitelist, skip analysis
            if (whitelist.Contains(dllNameWithoutExtension))
            {
                Console.WriteLine($"Skipping analysis for whitelisted package: {dllPath}");
                continue;
            }

            // Check if the DLL is obfuscated
            var isObfuscated = IsObfuscated(dllPath);
            if (isObfuscated)
            {
                Console.WriteLine($"Package {dllPath} is obfuscated. Skipping analysis.");
                Environment.Exit(1);
            }

            var code = Decompile(dllPath);
            var diagnostics = AnalyzeCode(code);
            if (diagnostics.Any())
            {
                Console.WriteLine($"Suspicious usage detected in {dllPath}:");
                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
                Environment.Exit(1);
            }
        }
    }
}
else
{
    Console.WriteLine("No .csproj files found. Exiting.");
}

var dllDirectory = Directory.GetCurrentDirectory();
var dllFiles = Directory.GetFiles(dllDirectory, "*.dll");

if (dllFiles.Length == 0)
{
    Console.WriteLine("No DLLs found for analysis.");
    Environment.Exit(0);
}

bool hasIssues = false;

foreach (var dllPath in dllFiles)
{
    if (IsObfuscated(dllPath))
    {
        Console.WriteLine($"Package {dllPath} appears to be obfuscated.");
        Environment.Exit(1);
    }

    var decompiledCode = Decompile(dllPath);
    var diagnostics = AnalyzeCode(decompiledCode);

    foreach (var diagnostic in diagnostics)
    {
        hasIssues = true;
        Console.WriteLine($"In {dllPath}: {diagnostic}");
    }
}

if (hasIssues)
{
    Environment.Exit(1);
}

static IEnumerable<string> GetDllsInProject(string projectPath)
{
    // For this example, I'm just getting all DLLs in the bin directory of the project.
    // Adjust this as needed.
    var binPath = Path.Combine(Path.GetDirectoryName(projectPath), "bin");
    if (Directory.Exists(binPath))
    {
        return Directory.EnumerateFiles(binPath, "*.dll", SearchOption.AllDirectories);
    }
    return Enumerable.Empty<string>();
}

bool IsObfuscated(string dllPath)
{
    var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings { ThrowOnAssemblyResolveErrors = false });
    var types = decompiler.TypeSystem.MainModule.TypeDefinitions;

    int suspectTypes = types.Count(t => t.Name.Length <= 2);
    return suspectTypes > types.Count() / 2;
}

HashSet<string> ExtractWhitelistedPackages(string csprojPath)
{
    var whitelist = new HashSet<string>();
    var doc = XDocument.Load(csprojPath);
    XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

    // Extract packages from ItemGroup with DllAnalyzerWhitelist=true attribute
    var whitelistedItemGroups = doc.Descendants(msbuild + "ItemGroup")
                                  .Where(ig => (string)ig.Attribute("DllAnalyzerWhitelist") == "true");

    foreach (var itemGroup in whitelistedItemGroups)
    {
        var packages = itemGroup.Descendants(msbuild + "PackageReference")
                                .Select(pr => pr.Attribute("Include").Value);
        foreach (var package in packages)
        {
            whitelist.Add(package);
        }
    }

    // Extract individual packages with DllAnalyzerWhitelist=true attribute
    var whitelistedPackages = doc.Descendants(msbuild + "PackageReference")
                                 .Where(pr => (string)pr.Attribute("DllAnalyzerWhitelist") == "true")
                                 .Select(pr => pr.Attribute("Include").Value);
    foreach (var package in whitelistedPackages)
    {
        whitelist.Add(package);
    }

    return whitelist;
}

IEnumerable<string> FindCsprojFiles(string rootDirectory)
{
    return Directory.EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);
}

string Decompile(string dllPath)
{
    var decompiler = new CSharpDecompiler(dllPath, new DecompilerSettings());
    return decompiler.DecompileWholeModuleAsString();
}

IEnumerable<Diagnostic> AnalyzeCode(string code)
{
    var syntaxTree = CSharpSyntaxTree.ParseText(code);

    var compilation = CSharpCompilation.Create("TempAssembly")
        .AddReferences(/* necessary references for the decompiled code */)
        .AddSyntaxTrees(syntaxTree);

    var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new HttpClientUsageAnalyzer());

    var compilationWithAnalyzer = compilation.WithAnalyzers(analyzers);
    var diagnostics = compilationWithAnalyzer.GetAnalyzerDiagnosticsAsync().Result;

    return diagnostics;
}