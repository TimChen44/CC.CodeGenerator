#pragma warning disable CS8632
namespace CC.CodeGenerato;

internal partial class NodeData
{
    public NodeData(GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol targetAttribute)
    {
        Context = context;
        Compilation = compilation;
        TargetAttribute = targetAttribute;
    }


    public GeneratorExecutionContext Context { get; }

    public Compilation Compilation { get; }

    public INamedTypeSymbol TargetAttribute { get; }

}

