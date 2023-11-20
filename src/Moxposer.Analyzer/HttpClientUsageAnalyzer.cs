using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Moxposer.Analyzer;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpClientUsageAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "HttpClientUsage";

    private static readonly LocalizableString Title = "Suspicious HttpClient Usage Detected";
    private static readonly LocalizableString MessageFormat = "HttpClient might be sending data to {0}";
    private static readonly LocalizableString Description = "Detects suspicious usage of HttpClient which might leak data.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static readonly ImmutableArray<string> SendingMethods = ImmutableArray.Create(
        "PostAsync", "PutAsync", "PatchAsync"
    );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        if (!(invocationExpr.Expression is MemberAccessExpressionSyntax memberAccessExpr))
            return;

        var methodName = memberAccessExpr.Name.Identifier.Text;

        if (!SendingMethods.Contains(methodName))
            return;

        // Get the type of the expression making the method call
        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression);

        // Check if it's of type HttpClient
        if (typeInfo.Type?.ToDisplayString() != "System.Net.Http.HttpClient")
            return;

        var arguments = invocationExpr.ArgumentList.Arguments;
        if (arguments.Count == 0)
            return;

        // Assuming the first argument is the URL
        var urlArgument = arguments[0].Expression;

        if (urlArgument is LiteralExpressionSyntax urlLiteral && urlLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            var diagnostic = Diagnostic.Create(Rule, urlLiteral.GetLocation(), urlLiteral.Token.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
        else
        {
            var unknownDiagnostic = Diagnostic.Create(Rule, urlArgument.GetLocation(), "unknown destination");
            context.ReportDiagnostic(unknownDiagnostic);
        }
    }

}
