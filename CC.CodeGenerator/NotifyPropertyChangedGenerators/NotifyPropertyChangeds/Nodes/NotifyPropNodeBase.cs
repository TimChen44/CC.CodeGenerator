#pragma warning disable CS8632 
using CC.CodeGenerator.NotifyPropertyChangedGenerators;
using CC.CodeGenerator.NotifyPropertyChangeds.CodeBuilds;
namespace CC.CodeGenerator.NotifyPropertyChangeds.Nodes;
public abstract class NotifyPropNodeBase : NodeBase
{
    private static readonly HashSet<char> ignoreSymbol = 
        new(@"~!@#$%^&*()_+{}|:<>?""`-=\[];',./0123456789 ".ToArray());

    public MemberAttributeTargetValidation TargetData { get; private set; } = null!;

    public override bool IsTarget() => TargetData.IsTarget();

    public override NodeBase SetContext(ContextData context)
    {
        TargetData = new MemberAttributeTargetValidation(context)
        {
            AttributeProvider = GetAttributeProvider()
        };
        return base.SetContext(context);
    }

    protected virtual SyntaxNode GetAttributeProvider() => SyntaxNode;

    internal abstract IEnumerable<NotifyPropCodeBuilderBase> CreateCodeBuilders();

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