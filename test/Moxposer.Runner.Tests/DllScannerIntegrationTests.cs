namespace Moxposer.Runner.Tests;

/// <summary>
/// <para>Integration tests for the DllScanner class.</para>
/// <para>
/// Why is this an integration test and not a unit test?
/// - We are testing the interaction between the DllScanner and the file system.
/// - We're not mocking or isolating individual components. Instead, we're testing
///   the system's behavior as a whole when it interacts with the real file system.
/// - We are setting up a realistic test environment, including the creation of
///   actual directories, files, and .csproj content.
/// - The output of the test depends on both the logic inside DllScanner and the
///   state/configuration of the file system, making it an integration of these components.
/// </para>
/// <para>
/// These tests will typically be slower than unit tests and may require special
/// setup or cleanup to ensure repeatability.
/// </para>
/// </summary>
public class DllScannerIntegrationTests : IDisposable
{
    private readonly string _testRootDirectory;

    public DllScannerIntegrationTests()
    {
        _testRootDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testRootDirectory);

        // Setup your test environment here. Create directories, .csproj files, DLLs etc.
        CreateTestProject(_testRootDirectory, "TestProject1");
    }

    /// <summary>
    /// Test that the ScanProjects method correctly identifies DLLs for analysis
    /// and those to be skipped based on the whitelist attribute in the .csproj file.
    /// </summary>
    [Fact]
    public void ScanProjects_CorrectlyIdentifiesDllsToAnalyzeAndSkip()
    {
        // Arrange
        var scanner = new DllScanner();

        // Act
        var results = scanner.ScanProjects(_testRootDirectory).ToList();

        // Assert
        Assert.Single(results);  // Assuming only one .csproj file for simplicity

        var scannedDllResult = results[0];
        // whitelist
        Assert.Contains(scannedDllResult.SkippedDlls, path => path.Contains("MockLib1"));

        // normal package reference
        Assert.Contains(scannedDllResult.DllsToAnalyze, path => path.Contains("MockLib2"));
        Assert.Contains(scannedDllResult.DllsToAnalyze, path => path.Contains("MockLib3"));
    }

    public void Dispose()
    {
        // Cleanup the created directories and files.
        if (Directory.Exists(_testRootDirectory))
        {
            Directory.Delete(_testRootDirectory, true);
        }
    }

    private void CreateTestProject(string rootDirectory, string projectName)
    {
        var projectDir = Path.Combine(rootDirectory, projectName);
        Directory.CreateDirectory(projectDir);

        // Create .csproj file with specific content.
        const string csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net5.0</TargetFramework>
    </PropertyGroup>
    <ItemGroup DllAnalyzerWhitelist=""true"">
        <PackageReference Include=""MockLib1"" Version=""1.0.0"" />
    </ItemGroup>
    <ItemGroup>
        <PackageReference Include=""MockLib2"" Version=""1.0.1"" />
        <PackageReference Include=""MockLib3"" Version=""1.0.2"" />
    </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectDir, $"{projectName}.csproj"), csprojContent);

        // Create a bin directory and place some mock .dll files
        var binDir = Path.Combine(projectDir, "bin");
        Directory.CreateDirectory(binDir);

        File.Create(Path.Combine(binDir, "MockLib1.dll")).Dispose();
        File.Create(Path.Combine(binDir, "MockLib2.dll")).Dispose();
        File.Create(Path.Combine(binDir, "MockLib3.dll")).Dispose();
        File.Create(Path.Combine(binDir, "UnreferencedMockLib.dll")).Dispose(); // This is an unreferenced mock DLL.
    }


}
