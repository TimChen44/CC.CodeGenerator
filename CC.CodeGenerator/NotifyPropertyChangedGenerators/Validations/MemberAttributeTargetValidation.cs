#pragma warning disable CS8632
namespace CC.CodeGenerato.Validations;

/// <summary>
/// 用于成员的特性验证
/// </summary>
internal partial class MemberAttributeTargetValidation : ITargetValidation
{
    private readonly Lazy<AttributeData[]> attributeDatas;

    public MemberAttributeTargetValidation(NodeData nodeData)
    {
        attributeDatas = new(GetAttributes);
        NodeData = nodeData;
    }

    public bool IsOk() => Attributes.Length > 0;

    public NodeData NodeData { get; }

    /// <summary>
    /// 特性的提供者
    /// </summary>
    public SyntaxNode AttributeProvider { get; set; } = null!;

    /// <summary>
    /// 目标特性类型
    /// </summary>
    public INamedTypeSymbol AttributeType { get; set; } = null!;

    /// <summary>
    /// 成员关联的符号
    /// </summary>
    public ISymbol MemberSymbol { get; private set; } = null!;

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
        MemberSymbol = AttributeProvider.GetDeclaredSymbol(NodeData.Compilation)!;
        return MemberSymbol is null || MemberSymbol.IsStatic
            ? Array.Empty<AttributeData>()
            : GetTargetAttributeDatas();
    }

    private AttributeData[] GetTargetAttributeDatas()
    {
        var res = MemberSymbol.GetTargetAttributes(AttributeType);
        if (res is null) return Array.Empty<AttributeData>();
        ContainingType = MemberSymbol switch
        {
            IFieldSymbol field => field.ContainingType,
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol,
            _ => throw new Exception("非预期类型")
        };
        return res;
    }    
}
