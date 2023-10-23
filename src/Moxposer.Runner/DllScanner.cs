using System.Xml.Linq;

namespace Moxposer.Runner;

public class DllScanner
{
    public static IEnumerable<ScannedDllResult> ScanProjects(string rootDirectory)
    {
        var results = new List<ScannedDllResult>();
        var csprojFiles = FindCsprojFiles(rootDirectory);

        foreach (var projectPath in csprojFiles)
        {
            var result = new ScannedDllResult { ProjectPath = projectPath };
            var whitelist = ExtractWhitelistedPackages(projectPath);
            var dllsInProject = GetDllsInProject(projectPath);

            foreach (var dllPath in dllsInProject)
            {
                var dllNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);

                if (whitelist.Contains(dllNameWithoutExtension))
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
        return results;
    }

    private static IEnumerable<string> FindCsprojFiles(string rootDirectory)
    {
        return Directory.EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);
    }

    private static HashSet<string> ExtractWhitelistedPackages(string csprojPath)
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

    private static IEnumerable<string> GetDllsInProject(string projectPath)
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
}
