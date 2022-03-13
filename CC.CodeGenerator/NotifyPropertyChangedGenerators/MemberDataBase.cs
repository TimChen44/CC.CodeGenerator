#nullable enable
using CC.CodeGenerator.NotifyPropertyChangeds;

namespace CC.CodeGenerator;

/// <summary>
/// 成员数据
/// </summary>
internal abstract class MemberDataBase
{
    private readonly Lazy<AttributeData[]> attributeDatas;
    private readonly Lazy<SyntaxNode> attributeProvider;

    public MemberDataBase()
    {
        attributeDatas = new(GetTargetAttributeDatas);
        attributeProvider = new(GetAttributeProvider);
    }

    public GeneratorExecutionContext Context { get; private set; }

    public Compilation Compilation { get; private set; } = null!;

    /// <summary>
    /// 成员所属的类型定义
    /// </summary>
    public INamedTypeSymbol MemberTypeDefinition { get; private set; } = null!;

    /// <summary>
    /// 目标特性
    /// </summary>
    public INamedTypeSymbol TargetAttribute { get; private set; } = null!;

    public AttributeData[] AttributeDatas => attributeDatas.Value;

    public SyntaxNode AttributeProvider => attributeProvider.Value;

    //返回和目标一致的特性
    private AttributeData[] GetTargetAttributeDatas()
    {
        var symbol = GetAttributeProvider().GetDeclaredSymbol(Compilation);
        return symbol is null || symbol.IsStatic
            ? Array.Empty<AttributeData>()
            : GetTargetAttributeDatas(symbol);
    }

    private AttributeData[] GetTargetAttributeDatas(ISymbol symbol)
    {
        var res = symbol.GetTargetAttributes(TargetAttribute);
        if (res is null) return Array.Empty<AttributeData>();
        MemberTypeDefinition = symbol switch
        {
            IFieldSymbol field => field.ContainingType,
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol,
            _ => throw new Exception("非预期类型")
        };
        return res;
    }


    /// <summary>
    /// 返回当前特性的提供者
    /// </summary>
    protected abstract SyntaxNode GetAttributeProvider();

    public virtual void SetValue(GeneratorExecutionContext context
        , Compilation compilation
        , INamedTypeSymbol targetAttribute)
    {
        Context = context;
        Compilation = compilation;
        TargetAttribute = targetAttribute;
    }

    /// <summary>
    /// 解析代码
    /// </summary>
    public virtual void ParsingCode()
    {
        if (AttributeDatas.Length > 0) Run();
    }

    /// <summary>
    /// 返回当前的成员
    /// </summary>
    protected abstract SyntaxNode GetCurrentMemberNode();

    protected abstract void Run();

    /// <summary>
    /// 首字母大小写
    /// </summary>
    /// <param name="isLower">是否小写</param>
    protected string GetInitialLower(string value, bool isLower = true)
    {
        var first = value[0].ToString();
        first = isLower ? first.ToLower() : first.ToUpper();
        return first + value.Substring(1);
    }

    public virtual bool ReportDiagnostic(string id, string errorMsg
      , string? helpLinkUri = null
      , SyntaxNode? node = null
      , DiagnosticSeverity diagnosticSeverity = DiagnosticSeverity.Error
      , int offset = 1
      , TypeShadowCode? typeCode = null)
    {
        node ??= GetCurrentMemberNode();
        typeCode?.AddError($"{id} : {errorMsg}");
        Context.ReportDiagnostic(node, DiagnosticSeverity.Error, id, errorMsg, helpLinkUri, offset);
        return false;
    }

    /// <summary>
    /// 命名冲突检测
    /// </summary>
    protected void TestNamingConflict(string? msg, AttributeData source)
    {
        if (msg is null) return;
        var node = source.GetSyntaxNode();
        if (node is null) return;
        ReportDiagnostic("DuplicateName", msg, node: node);
    }    
}