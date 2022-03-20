#pragma warning disable CS8632 
namespace CC.CodeGenerator;

/// <summary>
/// 用于成员的特性验证
/// </summary>
public partial class MemberAttributeTargetValidation
{
    private readonly Lazy<AttributeData[]> attributeDatas;

    public MemberAttributeTargetValidation(ContextData nodeData)
    {
        attributeDatas = new(GetAttributes);
        NodeData = nodeData;
    }

    public bool IsTarget() => Attributes.Length > 0;

    /// <summary>
    /// 特性的提供者
    /// </summary>
    public SyntaxNode AttributeProvider { get; set; } = null!;

    public ContextData NodeData { get; }

    /// <summary>
    /// 目标特性类型
    /// </summary>
    public INamedTypeSymbol? AttributeType => NodeData.TargetAttribute;

    /// <summary>
    /// 成员关联的符号
    /// </summary>
    public ISymbol Symbol { get; private set; } = null!;

    /// <summary>
    /// 字段 : 包含字段的类型<br/>
    /// 类型 : 类型
    /// </summary>
    public INamedTypeSymbol ContainingType { get; private set; } = null!;

    public AttributeData[] Attributes => attributeDatas.Value;


    /// <summary>
    /// 返回和目标一致的特性集合
    /// </summary>
    private AttributeData[] GetAttributes()
    {
        Symbol = AttributeProvider.GetDeclaredSymbol(NodeData.Compilation)!;
        return Symbol is null || Symbol.IsStatic
            ? Array.Empty<AttributeData>()
            : GetTargetAttributeDatas();
    }

    private AttributeData[] GetTargetAttributeDatas()
    {
        var res = Symbol.GetTargetAttributes(AttributeType);
        if (res is null) return Array.Empty<AttributeData>();
        ContainingType = Symbol switch
        {
            IFieldSymbol field => field.ContainingType,
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol,
            _ => throw new Exception("非预期类型")
        };
        return res;
    }
}
