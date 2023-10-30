using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Moxposer.Runner;

public interface IDllScanner
{
    IEnumerable<ScannedDllResult> ScanProjects(string rootDirectory);
}

public class DllScanner : IDllScanner
{
    public IEnumerable<ScannedDllResult> ScanProjects(string rootDirectory)
    {
        var results = new List<ScannedDllResult>();
        var csprojFiles = FindCsprojFiles(rootDirectory);
        if (csprojFiles.Any())
        {
            foreach (var projectPath in csprojFiles)
            {
                var result = new ScannedDllResult { ScanPath = projectPath };
                var whitelist = ExtractWhitelistedPackages(projectPath);
                var dllsInProject = GetDllsInProject(projectPath);

                foreach (var dllPath in dllsInProject)
                {
                    // Verify the authenticity of a DLL using its digital signature.
                    if (IsDllSignatureValid(dllPath))
                    {
                        result.DllsToAnalyze.Add(dllPath);
                        continue;
                    }

                    var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);

                    if (IsWhitelisted(dllNameWithoutExtension))
                    {
                        // Verify the authenticity of a DLL using its digital signature.
                        if (!IsDllSignatureValid(dllPath))
                        {
                            result.DllsToAnalyze.Add(dllPath);
                        }
                        else
                        {
                            result.SkippedDlls.Add(dllPath);
                        }
                    }
                    else if (whitelist.Contains(dllNameWithoutExtension))
                    {
                        result.SkippedDlls.Add(dllPath);
                    }
                    else
                    {
                        result.DllsToAnalyze.Add(dllPath);
                    }
                }
                results.Add(result);
            }
        }
        else
        {
            var result = new ScannedDllResult { ScanPath = rootDirectory };
            var dllWithPaths = FindAllDllFiles(rootDirectory);
            foreach (var dllPath in dllWithPaths)
            {
                var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);

                if (IsWhitelisted(dllNameWithoutExtension))
                {
                    // Verify the authenticity of a DLL using its digital signature.
                    if (!IsDllSignatureValid(dllPath))
                    {
                        result.DllsToAnalyze.Add(dllPath);
                    }
                    else
                    {
                        result.SkippedDlls.Add(dllPath);
                    }
                }
                else
                {
                    result.DllsToAnalyze.Add(dllPath);
                }
            }
            results.Add(result);
        }
        return results;
    }

    public static bool IsWhitelisted(string dllName)
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

    public static bool IsDllSignatureValid(string dllPath)
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

    private static IEnumerable<string> FindAllDllFiles(string rootDirectory)
    {
        return Directory.EnumerateFiles(rootDirectory, "*.dll", SearchOption.AllDirectories);
    }

    private static IEnumerable<string> FindCsprojFiles(string rootDirectory)
    {
        return Directory.EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);
    }

    private static HashSet<string> ExtractWhitelistedPackages(string csprojPath)
    {
        var whitelist = new HashSet<string>();
        var doc = XDocument.Load(csprojPath);

        // Extract packages from ItemGroup with DllAnalyzerWhitelist=true attribute
        var whitelistedItemGroups = doc.Descendants()
            .Where(d => d.Name.LocalName == "ItemGroup" && (string)d.Attribute("DllAnalyzerWhitelist") == "true");

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
            .Where(d => d.Name.LocalName == "PackageReference" && (string)d.Attribute("DllAnalyzerWhitelist") == "true")
            .Select(pr => pr.Attribute("Include").Value);

        foreach (var package in whitelistedPackages)
        {
            whitelist.Add(package);
        }

        return whitelist;
    }

    private static IEnumerable<string> GetDllsInProject(string projectPath)
    {
        var binPath = Path.Combine(Path.GetDirectoryName(projectPath), "bin");

        return Directory.Exists(binPath)
            ? Directory.EnumerateFiles(binPath, "*.dll", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();
    }
}
