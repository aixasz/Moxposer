using Microsoft.CodeAnalysis;
using Xunit;

namespace Moxposer.Analyzer.Test;

public class HttpClientUsageAnalyzerTests : AnalyzerTest
{
    [Fact]
    public async void AnalyzeNode_WhenHttpClientPostsHardcodedUrl_DetectsSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class TestClass
{
    HttpClient client = new HttpClient();
    void TestMethod()
    {
        client.PostAsync(""http://example.com"", null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Single(producedDiagnostics); // Ensure there's exactly one diagnostic produced
        var actualDiagnostic = producedDiagnostics[0];
        Assert.Equal(HttpClientUsageAnalyzer.DiagnosticId, actualDiagnostic.Id);
        Assert.Equal("HttpClient might be sending data to http://example.com", actualDiagnostic.GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, actualDiagnostic.Severity);
    }

    [Fact]
    public async void AnalyzeNode_WhenHttpClientPutsHardcodedUrl_DetectsSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class TestClass
{
    HttpClient client = new HttpClient();
    void TestMethod()
    {
        client.PutAsync(""http://example.com"", null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Single(producedDiagnostics);
        var actualDiagnostic = producedDiagnostics[0];
        Assert.Equal(HttpClientUsageAnalyzer.DiagnosticId, actualDiagnostic.Id);
        Assert.Equal("HttpClient might be sending data to http://example.com", actualDiagnostic.GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, actualDiagnostic.Severity);
    }

    [Fact]
    public async void AnalyzeNode_WhenHttpClientPatchesHardcodedUrl_DetectsSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class TestClass
{
    HttpClient client = new HttpClient();
    void TestMethod()
    {
        client.PatchAsync(""http://example.com"", null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Single(producedDiagnostics);
        var actualDiagnostic = producedDiagnostics[0];
        Assert.Equal(HttpClientUsageAnalyzer.DiagnosticId, actualDiagnostic.Id);
        Assert.Equal("HttpClient might be sending data to http://example.com", actualDiagnostic.GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, actualDiagnostic.Severity);
    }

    [Fact]
    public async void AnalyzeNode_WhenHttpClientPostsVariableUrl_DoesNotDetectSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class TestClass
{
    HttpClient client = new HttpClient();
    void TestMethod()
    {
        string url = ""http://example.com"";
        client.PostAsync(url, null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Single(producedDiagnostics);
        var actualDiagnostic = producedDiagnostics[0];
        Assert.Equal(HttpClientUsageAnalyzer.DiagnosticId, actualDiagnostic.Id);
        Assert.Equal("HttpClient might be sending data to unknown destination", actualDiagnostic.GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, actualDiagnostic.Severity);
    }

    [Fact]
    public async void AnalyzeNode_WhenDifferentClassUsesPostAsync_DoesNotDetectSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class FakeHttpClient 
{
    public void PostAsync(string url, object content) {}
}

class TestClass
{
    FakeHttpClient client = new FakeHttpClient();
    void TestMethod()
    {
        client.PostAsync(""http://example.com"", null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Empty(producedDiagnostics);
    }

    [Fact]
    public async void AnalyzeNode_WhenPostAsyncCalledOnDifferentObject_DoesNotDetectSuspiciousUsage()
    {
        // Arrange
        const string testCode = @"
using System.Net.Http;
class TestClass
{
    void TestMethod(HttpClient someOtherClient)
    {
        someOtherClient.PostAsync(""http://example.com"", null);
    }
}";

        // Act
        var producedDiagnostics = await GetProducedDiagnosticsAsync(testCode);

        // Assert
        Assert.Single(producedDiagnostics);
        var actualDiagnostic = producedDiagnostics[0];
        Assert.Equal(HttpClientUsageAnalyzer.DiagnosticId, actualDiagnostic.Id);
        Assert.Equal("HttpClient might be sending data to http://example.com", actualDiagnostic.GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, actualDiagnostic.Severity);
    }
}