#pragma warning disable CS8632
using CC.CodeGenerato.NotifyPropertyChangeds.CodeBuilds;
using CC.CodeGenerato.Validations;

namespace CC.CodeGenerato.NotifyPropertyChangeds.TargetValidations;
/// <summary>
/// 负责验证NotifyProperty
/// </summary>
internal abstract class TargetValidationBase : ITargetValidation
{
    private static readonly HashSet<char> ignoreSymbol =
       new(@"~!@#$%^&*()_+{}|:<>?""`-=\[];',./0123456789 ".ToArray());

    public TargetValidationBase(NodeData nodeData) => NodeData = nodeData;

    public NodeData NodeData { get; }

    /// <summary>
    /// 成员关联的符号
    /// </summary>
    public ISymbol MemberSymbol => TargetValidation.MemberSymbol;

    /// <summary>
    /// 包含字段的类型
    /// </summary>
    public INamedTypeSymbol ContainingType => TargetValidation.ContainingType;

    public abstract MemberAttributeTargetValidation TargetValidation { get; }

    public abstract SyntaxNode MemberSyntaxNode { get; }

    public bool IsOk() => TargetValidation.IsOk();

    /// <summary>
    /// 合并到相同的类型中
    /// </summary>
    public TargetValidationBase MergeType(NotifyPropertyTypeContainer typeContainer)
    {
        typeContainer.GetItem(ContainingType).AddMember(this);
        return this;
    }

    /// <summary>
    /// 返回代码创建器
    /// </summary>
    public abstract IEnumerable<CodeBuilderBase> CreateCodeBuilders();

    public static TargetValidationBase? TargetFactory(NodeData nodeParam
       , MemberDeclarationSyntax member) => member switch
       {
           FieldDeclarationSyntax field =>
               new FieldTargetValidation(nodeParam, field),

           ClassDeclarationSyntax or RecordDeclarationSyntax =>
               new TypeTargetValidation(nodeParam, member),

           _ => default
       };


    /// <summary>
    /// 首字母大小写
    /// </summary>
    /// <param name="isLower">是否小写</param>
    protected string? GetInitialLower(string? value, bool isLower = true)
    {
        if (value is null) return default;
        var first = value[0].ToString();
        first = isLower ? first.ToLower() : first.ToUpper();
        return first + value.Substring(1);
    }

    public static string? FormatName(string? name)
    {
        if (name is null) return default;
        for (int i = 0; i < name.Length; i++)
        {
            if (ignoreSymbol.Contains(name[i])) continue;
            return $"{name[i]}{name.Substring(i + 1)}";
        }
        return default;
    } 
}

internal abstract class TargetValidationBase<T> : TargetValidationBase
    where T : MemberDeclarationSyntax
{
    protected Lazy<MemberAttributeTargetValidation> targetValidation;

    protected TargetValidationBase(NodeData nodeData, T member) : base(nodeData)
    {
        Member = member;
        targetValidation = new(CreateTargetValidation);
    }

    public T Member { get; }

    public override SyntaxNode MemberSyntaxNode => Member;

    public override MemberAttributeTargetValidation TargetValidation => targetValidation.Value;

    protected virtual MemberAttributeTargetValidation CreateTargetValidation() => new(NodeData)
    {
        AttributeProvider = GetAttributeProvider(),
        AttributeType = NodeData.TargetAttribute,
    };

    protected virtual SyntaxNode GetAttributeProvider() => Member;
}
