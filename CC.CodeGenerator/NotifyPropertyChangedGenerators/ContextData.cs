#pragma warning disable CS8632 
namespace CC.CodeGenerator.NotifyPropertyChangedGenerators;

public class ContextData
{
    public ContextData(GeneratorExecutionContext context
        , INamedTypeSymbol? targetAttribute)
    {
        Context = context;
        TargetAttribute = targetAttribute;
    }

    public GeneratorExecutionContext Context { get; }

    public Compilation Compilation { get; set; } = null!;

    /// <summary>
    /// 目标特性
    /// </summary>
    public INamedTypeSymbol? TargetAttribute { get; }

    public ISymbol? GetSymbolInfo(SyntaxNode? node) => node?.GetInfoSymbol(Compilation);

    public ISymbol? GetDeclaredSymbol(SyntaxNode? node) => node?.GetDeclaredSymbol(Compilation);

    internal bool ReportError(DiagnosticData data, bool result = false)
    {
        data.ReportDiagnostic(Context);
        return result;
    }
}