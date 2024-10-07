using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Moxposer.Runner;

/// <summary>
/// Provides functionality to scan projects for DLL files and validate them against whitelist rules and digital signatures.
/// </summary>
public interface IDllScanner
{
    /// <summary>
    /// Scans all projects in the specified root directory for DLL files and returns the results of the scan, including valid and analyzed DLLs.
    /// </summary>
    /// <param name="rootDirectory">The root directory containing projects to scan.</param>
    /// <returns>An enumeration of <see cref="ScannedDllResult"/> containing the scan results.</returns>
    IEnumerable<ScannedDllResult> ScanProjects(string rootDirectory);

    /// <summary>
    /// Scans a specific project for DLLs and categorizes them into valid, analyzed, and skipped.
    /// </summary>
    /// <param name="projectPath">The path of the project to scan.</param>
    /// <param name="whitelist">The whitelist of package names extracted from the project file.</param>
    /// <returns>A result containing DLLs to analyze and those skipped.</returns>
    ScannedDllResult ScanProject(string projectPath, HashSet<string> whitelist);

    /// <summary>
    /// Scans the root directory for DLLs and categorizes them into valid, analyzed, and skipped.
    /// </summary>
    /// <param name="rootDirectory">The root directory to scan for DLLs.</param>
    /// <returns>A result containing DLLs to analyze and those skipped.</returns>
    ScannedDllResult ScanDirectory(string rootDirectory);
}

/// <summary>
/// Implements the scanning of projects for DLL files and validates them against a whitelist and digital signatures.
/// </summary>
public class DllScanner : IDllScanner
{
    /// <inheritdoc/>
    public IEnumerable<ScannedDllResult> ScanProjects(string rootDirectory)
    {
        var results = new List<ScannedDllResult>();
        var csprojFiles = FindCsprojFiles(rootDirectory);

        if (csprojFiles.Any())
        {
            foreach (var projectPath in csprojFiles)
            {
                var result = ScanProject(projectPath, ExtractWhitelistedPackages(projectPath));
                results.Add(result);
            }
        }
        else
        {
            var result = ScanDirectory(rootDirectory);
            results.Add(result);
        }

        return results;
    }

    /// <inheritdoc/>
    public ScannedDllResult ScanProject(string projectPath, HashSet<string> whitelist)
    {
        var result = new ScannedDllResult { ScanPath = projectPath };
        var dllsInProject = GetDllsInProject(projectPath);

        foreach (var dllPath in dllsInProject)
        {
            ProcessDll(dllPath, result, whitelist);
        }

        return result;
    }

    //// <inheritdoc/>
    public ScannedDllResult ScanDirectory(string rootDirectory)
    {
        var result = new ScannedDllResult { ScanPath = rootDirectory };
        var dllWithPaths = FindAllDllFiles(rootDirectory);

        foreach (var dllPath in dllWithPaths)
        {
            ProcessDll(dllPath, result, []);
        }

        return result;
    }

    /// <summary>
    /// Determines if the specified DLL name is present in the global whitelist.
    /// </summary>
    /// <param name="dllName">The name of the DLL to check.</param>
    /// <returns>True if the DLL is whitelisted, otherwise false.</returns>
    private static bool IsWhitelisted(string dllName)
    {
        foreach (var whitelist in AppSettings.GlobalWhitelist)
        {
            var regexPattern = "^" + Regex.Escape(whitelist).Replace("\\*", ".*") + "$";
            if (Regex.IsMatch(dllName, regexPattern))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifies the digital signature of the specified DLL file.
    /// </summary>
    /// <param name="dllPath">The file path of the DLL to check.</param>
    /// <returns>True if the DLL signature is valid, otherwise false.</returns>
    private static bool IsDllSignatureValid(string dllPath)
    {
        try
        {
            using var certificate = new X509Certificate2(dllPath);
            var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.Online,
                    RevocationFlag = X509RevocationFlag.ExcludeRoot
                }
            };

            return chain.Build(certificate);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Processes a DLL and determines whether it should be analyzed or skipped.
    /// </summary>
    /// <param name="dllPath">The path of the DLL to process.</param>
    /// <param name="result">The result object that tracks analyzed and skipped DLLs.</param>
    /// <param name="whitelist">The whitelist of package names for the project.</param>
    private static void ProcessDll(string dllPath, ScannedDllResult result, HashSet<string> whitelist)
    {
        var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);

        if (IsDllSignatureValid(dllPath))
        {
            result.DllsToAnalyze.Add(dllPath);
        }
        else if (IsWhitelisted(dllNameWithoutExtension) || whitelist.Contains(dllNameWithoutExtension))
        {
            result.SkippedDlls.Add(dllPath);
        }
        else
        {
            result.DllsToAnalyze.Add(dllPath);
        }
    }

    /// <summary>
    /// Finds all DLL files in the specified directory.
    /// </summary>
    /// <param name="rootDirectory">The directory to search for DLL files.</param>
    /// <returns>An enumeration of file paths of DLLs found in the directory.</returns>
    private static IEnumerable<string> FindAllDllFiles(string rootDirectory)
    {
        return Directory.EnumerateFiles(rootDirectory, "*.dll", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Finds all .csproj files in the specified directory.
    /// </summary>
    /// <param name="rootDirectory">The directory to search for .csproj files.</param>
    /// <returns>An enumeration of file paths of .csproj files found in the directory.</returns>
    private static IEnumerable<string> FindCsprojFiles(string rootDirectory)
    {
        return Directory.EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);
    }

    /// <summary>
    /// Extracts the whitelisted packages from the specified .csproj file.
    /// </summary>
    /// <param name="csprojPath">The file path of the .csproj file to analyze.</param>
    /// <returns>A set of whitelisted package names.</returns>
    private static HashSet<string> ExtractWhitelistedPackages(string csprojPath)
    {
        var whitelist = new HashSet<string>();
        var doc = XDocument.Load(csprojPath);

        // Extract packages from ItemGroup with DllAnalyzerWhitelist=true attribute
        var whitelistedItemGroups = doc.Descendants()
            .Where(element => element.Name.LocalName == "ItemGroup"
                           && (string)element.Attribute("DllAnalyzerWhitelist") == "true");

        foreach (var itemGroup in whitelistedItemGroups)
        {
            var packages = itemGroup.Descendants()
                .Where(d => d.Name.LocalName == "PackageReference")
                .Select(pr => pr.Attribute("Include").Value);

            foreach (var package in packages)
            {
                whitelist.Add(package);
            }
        }

        // Extract individual packages with DllAnalyzerWhitelist=true attribute
        var whitelistedPackages = doc.Descendants()
            .Where(element => element.Name.LocalName == "PackageReference" && (string)element.Attribute("DllAnalyzerWhitelist") == "true")
            .Select(pr => pr.Attribute("Include").Value);

        foreach (var package in whitelistedPackages)
        {
            whitelist.Add(package);
        }

        return whitelist;
    }

    /// <summary>
    /// Retrieves the DLL files located in the project's bin directory.
    /// </summary>
    /// <param name="projectPath">The file path of the project to analyze.</param>
    /// <returns>An enumeration of file paths of DLL files found in the project's bin directory.</returns>
    private static IEnumerable<string> GetDllsInProject(string projectPath)
    {
        var binPath = Path.Combine(Path.GetDirectoryName(projectPath), "bin");

        return Directory.Exists(binPath)
            ? Directory.EnumerateFiles(binPath, "*.dll", SearchOption.AllDirectories)
            : [];
    }
}
