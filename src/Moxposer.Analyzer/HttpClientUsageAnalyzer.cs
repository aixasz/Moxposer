using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Moxposer.Analyzer;

/// <summary>
/// A diagnostic analyzer that detects suspicious usage of <see cref="HttpClient"/> methods that send data (e.g., POST, PUT, PATCH),
/// and reports potential security issues, such as unintentional data leaks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpClientUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The unique identifier for this diagnostic analyzer.
    /// </summary>
    public const string DiagnosticId = "HttpClientUsage";

    private static readonly LocalizableString Title = "Suspicious HttpClient Usage Detected";
    private static readonly LocalizableString MessageFormat = "HttpClient might be sending data to {0}";
    private static readonly LocalizableString Description = "Detects suspicious usage of HttpClient which might leak data.";
    private const string Category = "Usage";

    /// <summary>
    /// The diagnostic rule that describes the problem being analyzed (i.e., suspicious HttpClient usage).
    /// </summary>
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    /// <summary>
    /// The set of diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <summary>
    /// Methods in HttpClient that are used for sending data and are of interest for this analysis (e.g., POST, PUT, PATCH).
    /// </summary>
    private static readonly ImmutableArray<string> SendingMethods = ImmutableArray.Create(
        "PostAsync", "PutAsync", "PatchAsync"
    );

    /// <summary>
    /// Initializes the analyzer and registers the syntax node actions to perform the analysis.
    /// </summary>
    /// <param name="context">The analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        // Disable analysis of generated code
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution for performance
        context.EnableConcurrentExecution();

        // Register action to analyze method invocation expressions
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// Analyzes an invocation expression to detect suspicious usage of HttpClient methods.
    /// </summary>
    /// <param name="context">The context of the syntax node being analyzed.</param>
    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        // Check if the expression is a member access (e.g., HttpClient.PostAsync())
        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
            return;

        // Ensure the method is one of the HTTP sending methods (e.g., PostAsync, PutAsync)
        if (!IsSendingMethod(memberAccessExpr))
            return;

        // Ensure the method is called on an HttpClient instance
        if (!IsHttpClientInvocation(memberAccessExpr, context))
            return;

        // Retrieve the URL argument (first argument)
        var urlArgument = invocationExpr.ArgumentList.Arguments.FirstOrDefault()?.Expression;
        if (urlArgument == null)
            return;

        ReportDiagnosticForUrl(context, urlArgument);
    }

    /// <summary>
    /// Validates if the given invocation expression is a member access expression.
    /// </summary>
    /// <param name="invocationExpr">The invocation expression to validate.</param>
    /// <param name="memberAccessExpr">Outputs the member access expression if valid.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    private static bool IsValidMemberAccess(InvocationExpressionSyntax invocationExpr, out MemberAccessExpressionSyntax memberAccessExpr)
    {
        memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
        return memberAccessExpr != null;
    }

    /// <summary>
    /// Checks if the method being invoked is one of the HTTP sending methods (PostAsync, PutAsync, PatchAsync).
    /// </summary>
    /// <param name="memberAccessExpr">The member access expression.</param>
    /// <returns>True if the method is a sending method; otherwise, false.</returns>
    private static bool IsSendingMethod(MemberAccessExpressionSyntax memberAccessExpr)
    {
        var methodName = memberAccessExpr.Name.Identifier.Text;
        return SendingMethods.Contains(methodName);
    }

    /// <summary>
    /// Validates if the invocation is being made on an instance of HttpClient.
    /// </summary>
    /// <param name="memberAccessExpr">The member access expression.</param>
    /// <param name="context">The analysis context.</param>
    /// <returns>True if it's an HttpClient invocation; otherwise, false.</returns>
    private static bool IsHttpClientInvocation(MemberAccessExpressionSyntax memberAccessExpr, SyntaxNodeAnalysisContext context)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccessExpr.Expression);
        return typeInfo.Type?.ToDisplayString() == "System.Net.Http.HttpClient";
    }

    /// <summary>
    /// Reports a diagnostic based on the URL argument (literal or unknown destination).
    /// </summary>
    /// <param name="context">The analysis context.</param>
    /// <param name="urlArgument">The URL argument to analyze.</param>
    private void ReportDiagnosticForUrl(SyntaxNodeAnalysisContext context, ExpressionSyntax urlArgument)
    {
        if (urlArgument is LiteralExpressionSyntax urlLiteral && urlLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            // Report diagnostic with the URL
            var diagnostic = Diagnostic.Create(Rule, urlLiteral.GetLocation(), urlLiteral.Token.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
        else
        {
            // Report diagnostic with unknown destination
            var unknownDiagnostic = Diagnostic.Create(Rule, urlArgument.GetLocation(), "unknown destination");
            context.ReportDiagnostic(unknownDiagnostic);
        }
    }

}
