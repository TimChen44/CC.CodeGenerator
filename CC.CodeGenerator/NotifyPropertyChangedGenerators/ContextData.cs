namespace CC.CodeGenerator;

public partial class ContextData
{
    public ContextData(GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol targetAttribute)
    {
        Context = context;
        Compilation = compilation;
        TargetAttribute = targetAttribute;
    }


    public GeneratorExecutionContext Context { get; }

    public Compilation Compilation { get; }

    /// <summary>
    /// 目标特性
    /// </summary>
    public INamedTypeSymbol TargetAttribute { get; }

}

