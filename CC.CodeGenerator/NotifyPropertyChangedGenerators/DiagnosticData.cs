#pragma warning disable CS8632
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CC.CodeGenerato;

/// <summary>
/// 诊断信息
/// </summary>
internal partial class DiagnosticData
{
    public DiagnosticData(string id, string error)
    {
        Error = error;
        Id = id;
    }

    public DiagnosticData(Location? location, string id, string error) : this(id, error)
    {
        Location = location;
    }

    public DiagnosticData(SyntaxNode syntaxNode, string id, string error)
        : this(syntaxNode.GetLocation(), id, error) { }

    public Location? Location { get; set; }

    public string Error { get; }
    public string Id { get; }


    public DiagnosticSeverity DiagnosticSeverity { get; set; } = DiagnosticSeverity.Error;

    public string? HelpLinkUri { get; set; }

    public int LocationOffset { get; set; } = 1;


    private Location? GetLocation(int locationOffset)
    {
        if (Location is null || Location.SourceTree is null) return default;
        var span = Location.SourceSpan;
        var textSpan = TextSpan.FromBounds(span.Start - locationOffset, span.End);
        return Location.Create(Location.SourceTree, textSpan);
    }

    public void ReportDiagnostic(GeneratorExecutionContext context)
    {
        var diagnosticDescriptor = new DiagnosticDescriptor
       (
           Id,
           "CC.CodeGenerator",
           messageFormat: "{0}",
           category: "CC.CodeGenerator",
           defaultSeverity: DiagnosticSeverity,
           isEnabledByDefault: true,
           helpLinkUri: HelpLinkUri
        );

        var location = GetLocation(LocationOffset);
        var diagnostic = Diagnostic.Create(diagnosticDescriptor, location, Error);
        context.ReportDiagnostic(diagnostic);
    }
}
