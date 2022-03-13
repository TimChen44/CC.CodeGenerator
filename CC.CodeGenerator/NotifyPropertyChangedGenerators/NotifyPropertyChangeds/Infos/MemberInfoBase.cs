#nullable enable
namespace CC.CodeGenerator.NotifyPropertyChangeds.Infos;

internal abstract class MemberInfoBase : MemberDataBase
{
    public const string attributeName = "AddNotifyPropertyChangedAttribute";
    public const string attributePath = $"CC.CodeGenerator.{attributeName}";
    public const string attributeCtor = $"{attributePath}.{attributeName}";
    protected static readonly string[] handlerNames = new[]
    {
        "SetPropertyMethodName",
        "OnPropertyChangedMethodName"
    };

    public NotifyPropChangedCodeManager CodeManager { get; private set; } = null!;

    public MemberInfoBase SetValue(GeneratorExecutionContext context
       , Compilation compilation
       , INamedTypeSymbol targetAttribute
       , NotifyPropChangedCodeManager codeManager)
    {
        base.SetValue(context, compilation, targetAttribute);
        CodeManager = codeManager;
        return this;
    }

    /// <summary>
    /// 返回对应的TypeItem
    /// </summary>
    protected TypeShadowCode GetTypeDefinition() =>
        CodeManager.GetTypeDefinition(MemberTypeDefinition);
}

internal abstract class MemberInfoBase<T> : MemberInfoBase where T : MemberDeclarationSyntax
{
    private static readonly HashSet<char> filters = new() { '_', '@', };

    public MemberInfoBase(T member) => Member = member;

    public T Member { get; }

    protected override SyntaxNode GetCurrentMemberNode() => Member;

    protected string? GetPropName(string name)
    {
        var index = name
            .Select((x, i) => (value: x, index: i + 1))
            .Where(x => !filters.Contains(x.value))
            .FirstOrDefault().index;
        if (index is 0) return default;
        return name.Substring(index - 1);
    }
}