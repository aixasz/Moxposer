using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Moxposer.Analyzer.Test;

public abstract class AnalyzerTest
{
    protected static async Task<List<Diagnostic>> GetProducedDiagnosticsAsync(string source)
    {
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new HttpClientUsageAnalyzer());
        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location)
        };
        var project = CreateProject(source, references);
        var compilation = await project.GetCompilationAsync();

        // Run the analyzer on this in-memory representation
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

        return [.. diagnostics];
    }

    private static Project CreateProject(string source, IEnumerable<MetadataReference> references)
    {
        // Create a unique name for each project to avoid any potential conflicts
        string projectName = Guid.NewGuid().ToString();

        // Create an AdhocWorkspace, which is an in-memory representation of a workspace.
        var workspace = new AdhocWorkspace();

        // Add a new project to the workspace.
        var projectId = ProjectId.CreateNewId();
        var projectInfo = ProjectInfo.Create(
            projectId,
            VersionStamp.Create(),
            projectName,
            projectName,
            LanguageNames.CSharp,
            metadataReferences: references);

        var newProject = workspace.AddProject(projectInfo);

        // Add the provided source code as a new document to the project.
        var document = newProject.AddDocument(projectName + ".cs", SourceText.From(source));

        // Return the updated project containing the source code document.
        return document.Project;
    }
}
